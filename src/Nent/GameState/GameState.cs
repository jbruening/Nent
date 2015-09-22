﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace Nent
{
    public class GameState
    {
        private const int LoopTightness = 3;

        readonly Stopwatch _watch = new Stopwatch();
        private Thread _createdThread;
        bool _quit = true;
        private double _frameTime;
        private readonly Queue<Action> _invokeQueue = new Queue<Action>();
        private readonly object _invokeLocker = new object();
        private readonly Queue<Component> _queuedStarts = new Queue<Component>();

        /// <summary>
        /// all the gameobjects. Warning: some of the values will be null. You need to check for that before using them.
        /// </summary>
        public IEnumerable<GameObject> GameObjects
        {
            get { return _gameObjects; }
        }
        private GameObject[] _gameObjects;
        private IStateContainer _container;

        private struct GobjCaller
        {
            public Component Component;
            public GameObject Object;
            public Action Method;
        }

        private List<GobjCaller> _gUpdates = new List<GobjCaller>(32);
        private List<GobjCaller> _gLUpdates = new List<GobjCaller>(32);

        /// <summary>
        /// whether or not the current thread is the same thread as what the game state is running on
        /// </summary>
        public bool InvokeRequired
        {
            get { return Thread.CurrentThread != _createdThread; }
        }

        public void StartOnOtherThread()
        {
            _createdThread = new Thread(CreateThreadStart);
            _createdThread.Start();
        }

        void CreateThreadStart()
        {
            Start();
        }

        public void Start(double frameTime = 0.02d)
        {
            _gameObjects = new GameObject[10];

            if (!_quit) return;

            _frameTime = frameTime;
            _watch.Start();
            _createdThread = Thread.CurrentThread;

            _quit = false;

            while (!_quit)
            {
                if (_watch.Elapsed.TotalSeconds - Time >= _frameTime)
                    Loop();
                Thread.Sleep(LoopTightness);
            }
        }

        public double Time { get; private set; }
        public double DeltaTime { get; private set; }
        internal double PreviousFrameTime { get; private set; }

        private void Loop()
        {
            PreviousFrameTime = Time;
            Time = _watch.Elapsed.TotalSeconds;
            DeltaTime = Time - PreviousFrameTime;

            while (_queuedStarts.Count > 0)
            {
                _queuedStarts.Dequeue().InternalStartCall();
            }

            try
            {
                PreUpdate.Raise();
            }
            catch (Exception e)
            {
                Debug.LogException(e, "GameState.PreUpdate");
            }

// ReSharper disable once ForCanBeConvertedToForeach for is faster with lists
            for (int i = 0; i < _gUpdates.Count; i++)
            {
                try
                {
                    _gUpdates[i].Method();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, "Object {0}", _gUpdates[i].Object.Name);
                }
            }

            try
            {
                Update.Raise();
            }
            catch (Exception e)
            {
                Debug.LogException(e, "GameState.Update");
            }

            foreach (GameObject t in _gameObjects)
            {
                if (t == null) continue;
                t.RunCoroutines();
            }

// ReSharper disable once ForCanBeConvertedToForeach for is faster with lists
            for (int i = 0; i < _gLUpdates.Count; i++)
            {
                try
                {
                    _gLUpdates[i].Method();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, "Object {0}", _gLUpdates[i].Object.Name);
                }
            }

            try
            {
                LateUpdate.Raise();
            }
            catch (Exception e)
            {
                Debug.LogException(e, "GameState.LateUpdate");
            }


            Action[] invokes;
            lock (_invokeLocker)
            {
                invokes = _invokeQueue.ToArray();
                _invokeQueue.Clear();
            }

            foreach (var act in invokes)
            {
                try
                {
                    act();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, "Invoke Queued");
                }
            }

// ReSharper disable once ForCanBeConvertedToForeach for loop is faster with lists...
            for (var i = 0; i < _gameObjectsToCleanup.Count; i++)
            {
                var g = _gameObjectsToCleanup[i];
                g.DestroyNow();
            }
            _gameObjectsToCleanup.Clear();
        }

        /// <summary>
        /// Run an action on the gamestate's thread, next update
        /// </summary>
        /// <param name="action"></param>
        public void Invoke(Action action)
        {
            lock (_invokeLocker)
            {
                _invokeQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Invoke the specified action if it needs to be invoked. Otherwise, run it now.
        /// </summary>
        /// <param name="action"></param>
        public void InvokeIfRequired(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        /// <summary>
        /// Creates a new gameobject
        /// </summary>
        /// <returns>a new gameobject</returns>
        /// <exception cref="ThreadStateException">
        /// if this function is not run on the gamestate thread.
        /// </exception>
        public GameObject CreateNewGameObject()
        {
            if (InvokeRequired)
                throw new ThreadStateException("Cannot make gameobjects not on the gamestate thread. Use GameState.InvokeIfRequired.");
            int id = -1;
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i] == null)
                {
                    id = i;
                    break;
                }
            }
            
            if (id == -1)
            {
                Array.Resize(ref _gameObjects, _gameObjects.Length + 1);
                id = _gameObjects.Length - 1;
            }

            _gameObjects[id] = new GameObject(this, id);
            return _gameObjects[id];
        }

        internal void RemoveObject(GameObject gameObject)
        {
            if (InvokeRequired)
                throw new ThreadStateException("Cannot destroy gameobjects not on the gamestate thread. Use GameState.InvokeIfRequired.");

            if (gameObject.Id == -1)
            {
                Debug.LogWarning("Attempted to destroy {0}, which is already pending destruction", gameObject);
                return;
            }

            _gameObjectsToCleanup.Add(_gameObjects[gameObject.Id]);
            _gameObjects[gameObject.Id] = null;
            gameObject.Id = -1;

            int remC = 0;
            for (int i = _gameObjects.Length - 1; i >= 0; i--)
            {
                if (_gameObjects[i] != null)
                    break;
                remC++;
            }

            if (remC > 0)
            {
                Array.Resize(ref _gameObjects, _gameObjects.Length - remC);
            }

            _gUpdates.RemoveAll(g => g.Object == gameObject);
            _gLUpdates.RemoveAll(g => g.Object == gameObject);
        }

        private readonly List<GameObject> _gameObjectsToCleanup = new List<GameObject>();

        internal void SubscribeComponent(Component component, GameObject sender)
        {
            SubscribeComponentMethod("Update", component, sender, _gUpdates);
            SubscribeComponentMethod("LateUpdate", component, sender, _gLUpdates);
        }

        private void SubscribeComponentMethod(string method, Component component, GameObject sender, List<GobjCaller> callers)
        {
            var ctype = component.GetType();

            var uinfo = ctype
                .GetMethod(method, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
            if (uinfo == null) return;
            if (uinfo.DeclaringType == ctype)
            {
                callers.Add(new GobjCaller
                {
                    Method = Delegate.CreateDelegate(typeof(Action), component, uinfo) as Action,
                    Object = sender,
                    Component = component
                });
            }
        }

        internal void UnsubscribeComponent(Component component, GameObject sender)
        {
            _gUpdates.RemoveAll(g => g.Object == sender && g.Component == component);
            _gLUpdates.RemoveAll(g => g.Object == sender && g.Component == component);
        }

        internal event Action PreUpdate;
        public event Action Update;
        public event Action LateUpdate;

        public void Stop()
        {
            _quit = true;
        }

        public void QueueStart(Component component)
        {
            _queuedStarts.Enqueue(component);
        }

        /// <summary>
        /// set the container to be returned by Container`T()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        public void SetContainer<T>(T container) where T : IStateContainer
        {
            _container = container;
        }

        /// <summary>
        /// get the value set by SetContainer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Container<T>() where T : IStateContainer
        {
            return (T) _container;
        }
    }

    public interface IStateContainer
    {
        GameState State { get; }
    }
}