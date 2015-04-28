using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Yaml.Serialization;

namespace Nent
{
    public abstract class Component
    {
        /// <summary>
        /// The gameobject this is attached to. cached result.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// if the gameobject has been destroyed
        /// </exception>
        [YamlSerialize(YamlSerializeMethod.Never)]
        public GameObject GameObject
        {
            get
            {
                if (_gameObject == null) throw new ObjectDisposedException(GetType().Name);
                return _gameObject;
            }
            internal set { _gameObject = value; }
        }

        [YamlSerialize(YamlSerializeMethod.Never)]
        public GameState GameState { get { return GameObject.GameState; } }

        /// <summary>
        /// safely get the gameobject's name, instead of having to worry about an error
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string SafeGameObjectName { get { return _gameObject == null ? "OBJECT_DISPOSED" : _gameObject.Name; } }

        #region coroutine

        /// <summary>
        /// Start a coroutine
        /// Not thread safe.
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            routine.MoveNext();
            _shouldRunNextFrame.Add(routine);
            return new Coroutine(routine);
        }

        List<IEnumerator> _unblockedCoroutines = new List<IEnumerator>();
        List<IEnumerator> _shouldRunNextFrame = new List<IEnumerator>();
        private GameObject _gameObject;

        internal void RunCoroutines()
        {
            Coroutine.Run(ref _unblockedCoroutines, ref _shouldRunNextFrame);
        }

        #endregion

        /// <summary>
        /// Get the first component on the GameObject of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
            where T : Component
        {
            return GameObject.GetComponent<T>();
        }

        /// <summary>
        /// Get all components of the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>()
            where T : Component
        {
            return GameObject.GetComponents<T>();
        }

        internal void InternalAwakeCall()
        {
            try { Awake(); }
            catch (Exception e) { Debug.LogException(e, "Object {0}", SafeGameObjectName); } 
            //register to call start
            GameObject.GameState.QueueStart(this);
        }
        protected virtual void Awake() { }
        internal void InternalStartCall() { try { Start(); } catch (Exception e) { Debug.LogException(e, "Object {0}", SafeGameObjectName); } }
        protected virtual void Start() { }
        internal void InternalUpdateCall() { try { Update(); } catch (Exception e) { Debug.LogException(e, "Object {0}", SafeGameObjectName); } }
        protected virtual void Update() { }
        internal void InternalLateUpdateCall() { try { LateUpdate(); } catch (Exception e) { Debug.LogException(e, "Object {0}", SafeGameObjectName); } }
        protected virtual void LateUpdate() { }
        internal void InternalOnComponentAddedCall(Component component) { try { OnComponentAdded(component); } catch (Exception e) { Debug.LogException(e, "Object {0}", SafeGameObjectName); } }
        protected virtual void OnComponentAdded(Component component) { }
        internal void InternalOnDestroyCall() { try { OnDestroy(); } catch (Exception e) { Debug.LogException(e, "Object {0}", SafeGameObjectName); } }
        protected virtual void OnDestroy() { }

        internal void Dispose()
        {
            try
            { Disposing(); }
            catch (Exception e)
            {
                Debug.LogException(e, "Disposing {0}", SafeGameObjectName);
            }
            //help prevent bad use of the library from keeping the other components around.
            GameObject = null;
        }
        /// <summary>
        /// The object is being deleted
        /// </summary>
        protected virtual void Disposing() { }
    }
}
