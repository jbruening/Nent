using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Yaml.Serialization;
using JetBrains.Annotations;

namespace Nent
{
    public sealed partial class GameObject
    {
        [YamlSerialize(YamlSerializeMethod.Assign)] private List<Component> components = new List<Component>(4);

        internal void OnComponentAfterDeserialization()
        {
            foreach (var component in components)
            {
                OnComponentAdded(component);
            }
        }

        /// <summary>
        /// Get the first component of type T attached to the gameobject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
            where T : class
        {
            if (components == null)
                return null;

// ReSharper disable once ForCanBeConvertedToForeach - speed is necessary
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                var unknown = component as T;
                if (unknown != null) return unknown;
            }
            return null;
        }

        /// <summary>
        /// Get the component of the specified type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Component GetComponent(Type t)
        {
// ReSharper disable once ForCanBeConvertedToForeach - speed is necessary
            for (int i = 0; i < components.Count; i++)
            {
                var c = components[i];
                if (c.GetType().IsSubclassOf(t)) return c;
            }
            return null;
        }

        /// <summary>
        /// Get all the components of type T attached to the GameObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>()
            where T : class
        {
            var list = new List<T>();
// ReSharper disable once ForCanBeConvertedToForeach - speed is necessary
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                var unknown = component as T;
                if (unknown != null) list.Add(unknown);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get all the components of the specified type attached to the GameObject
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Component[] GetComponents(Type t)
        {
            var list = new List<Component>();
// ReSharper disable once ForCanBeConvertedToForeach - speed is necessary
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (t.IsInstanceOfType(component)) list.Add(component);
            }
            return list.ToArray();
        }

        private Component InitialAddComponent(Type componentType)
        {
            if (GameState.InvokeRequired)
                throw new ThreadStateException("Cannot add components not on the gamestate thread. Use GameState.InvokeIfRequired.");

            var component = Activator.CreateInstance(componentType) as Component;

            if (component == null)
            {
                Debug.LogError("Could not add component of type {0}. Does it have a parameterless contructor?", componentType.Name);
                return null;
            }

            component.GameObject = this;
            components.Add(component);
            //GameState.AddStart(component.InternalStartCall);
            return component;
        }

        /// <summary>
        /// Attach the component of type T to the gameobject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>instance of T that was attached to the GameObject</returns>
        public T AddComponent<T>()
            where T : Component
        {
            var component = InitialAddComponent(typeof (T)) as T;
            if (component == null)
                return null;
            component.InternalAwakeCall();
            OnComponentAdded(component);
            return component;
        }

        /// <summary>
        /// Add a component of the specified type to the GameObject
        /// </summary>
        /// <param name="componentType">Must be an inheriting class of Component</param>
        /// <returns>instance of the component that was added</returns>
        public Component AddComponent(Type componentType)
        {
            var component = InitialAddComponent(componentType);
            if (component == null)
                return null;
            component.InternalAwakeCall();
            OnComponentAdded(component);
            return component;
        }

        /// <summary>
        /// add a component, but do not call awake or OnComponentAdded
        /// </summary>
        /// <param name="componentType"></param>
        /// <returns></returns>
        internal Component DeserializeAddComponent(Type componentType)
        {
            return InitialAddComponent(componentType);
        }

        /// <summary>
        /// Add the specified types of components to the gameobject.
        /// Awake is run after all the components have been initialized, so you can safely do GetComponent on other types passed into this during Awake
        /// The awake order is also run in the order at which the components are ordered in the parameters
        /// OnComponentsAdded is also called, after the awakes, in the same order.
        /// </summary>
        /// <param name="componentTypes"></param>
        /// <returns></returns>
        public Component[] AddComponents(params Type[] componentTypes)
        {
            var length = componentTypes.Length;
            var addedComponents = new Component[length];
            for (int i = 0; i < length; ++i)
            {
                var component = InitialAddComponent(componentTypes[i]);
                if (component != null)
                {
                    addedComponents[i] = component;
                }
            }

            for (int i = 0; i < length; ++i)
            {
                var comp = addedComponents[i];
                if (comp != null)
                {
                    comp.InternalAwakeCall();
                }
            }

            for (int i = 0; i < length; ++i)
            {
                var comp = addedComponents[i];
                if (comp != null)
                {
                    OnComponentAdded(comp);
                }
            }

            return addedComponents;
        }


        /// <summary>
        /// Remove the specified component. Only by actual object.Reference
        /// </summary>
        /// <param name="target"></param>
        public void RemoveComponent(Component target)
        {
            if (GameState.InvokeRequired)
                throw new ThreadStateException("Cannot add components not on the gamestate thread. Use GameState.InvokeIfRequired.");

            var ind = components.FindIndex(c => object.ReferenceEquals(c, target));

            if (ind != -1)
            {
                var c = components[ind];
                GameState.UnsubscribeComponent(c, this);
                components.RemoveAt(ind);
                target.Dispose();
            }
        }
    }
}