using System;
using System.Collections.Generic;
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
        [YamlSerialize(YamlSerializeMethod.Never)]
        public GameObject gameObject { get; internal set; }

        /// <summary>
        /// in order to actually start a coroutine chain, you need to set IsRootRoutine to true on the first call in a coroutine call chain.
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Coroutine StartCoroutine(IEnumerator<YieldInstruction> routine)
        {
            //if (IsRootRoutine)
            //{
            //    GameState.AddRoutine(routine);
            //    rootRoutines.Add(routine);
            //}
            return new Coroutine(routine);
        }
        
        [YamlSerialize(YamlSerializeMethod.Never)]
        internal List<IEnumerator<YieldInstruction>> rootRoutines = new List<IEnumerator<YieldInstruction>>();

        /// <summary>
        /// Get the first component on the gameObject of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
            where T : Component
        {
            return gameObject.GetComponent<T>();
        }

        /// <summary>
        /// Get all components of the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>()
            where T : Component
        {
            return gameObject.GetComponents<T>();
        }

        internal void InternalAwakeCall() { try { Awake(); } catch (Exception e) { Debug.LogError(e.ToString()); } }
        protected virtual void Awake() { }
        internal void InternalStartCall() { try { Start(); } catch (Exception e) { Debug.LogError(e.ToString()); } }
        protected virtual void Start() { }
        internal void InternalUpdateCall() { try { Update(); } catch (Exception e) { Debug.LogError(e.ToString()); } }
        protected virtual void Update() { }
        internal void InternalLateUpdateCall() { try { LateUpdate(); } catch (Exception e) { Debug.LogError(e.ToString()); } }
        protected virtual void LateUpdate() { }
        internal void InternalOnComponentAddedCall(Component component) { try { OnComponentAdded(component); } catch (Exception e) { Debug.LogError(e.ToString()); } }
        protected virtual void OnComponentAdded(Component component) { }
        internal void InternalOnDestroyCall() { try { OnDestroy(); } catch (Exception e) { Debug.LogError(e.ToString()); } }
        protected virtual void OnDestroy() { }

        internal void Dispose()
        {
            try
            { Disposing(); }
            catch (Exception e)
            {
                Debug.LogError("[Disposing {0}] {1}", gameObject.Name, e);
            }
            foreach (var routine in rootRoutines)
            {
                //GameState.RemoveRoutine(routine);
            }

            rootRoutines = null;
        }
        /// <summary>
        /// The object is being deleted
        /// </summary>
        protected virtual void Disposing() { }
    }
}
