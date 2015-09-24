﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Nent.Manager;

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

        private readonly ComponentManager _updateManager = new ComponentManager("Update");
        private readonly ComponentManager _lateUpdateManager = new ComponentManager("LateUpdate");
        private readonly ComponentManager _routineManager = new ComponentManager("RunCoroutines", false);

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

            TryRaise(PreUpdate, "GameState.PreUpdate");

            _updateManager.Run();

            TryRaise(Update, "GameState.Update");

            _routineManager.Run();

            _lateUpdateManager.Run();

            TryRaise(LateUpdate, "GameState.LateUpdate");

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

        private void TryRaise(Action action, string error)
        {
            try
            {
                if (action != null)
                    action();
            }
            catch (Exception e)
            {
                Debug.LogException(e, error);
            }
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

            _updateManager.RemoveAll(gameObject);
            _routineManager.RemoveAll(gameObject);
            _lateUpdateManager.RemoveAll(gameObject);
        }

        private readonly List<GameObject> _gameObjectsToCleanup = new List<GameObject>();

        internal void SubscribeComponent(Component component, GameObject sender)
        {
            _updateManager.TryAdd(component, sender);
            _routineManager.TryAdd(component, sender);
            _lateUpdateManager.TryAdd(component, sender);
        }

        internal void UnsubscribeComponent(Component component, GameObject sender)
        {
            _updateManager.Remove(component);
            _routineManager.Remove(component);
            _lateUpdateManager.Remove(component);
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