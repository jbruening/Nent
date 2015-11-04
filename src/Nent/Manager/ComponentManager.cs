using System;
using System.Collections.Generic;
using System.Reflection;
using Nent.Extensions;

namespace Nent.Manager
{
    class ComponentManager
    {
        private readonly string _methodName;
        private readonly bool _requireDeclaration;

        //struct, to remove pointer lookups
        struct Callee
        {
            public Action Action;
            public Component Component;
            public GameObject Object;

            public Callee(Component component, GameObject @object, Action action)
            {
                Component = component;
                Object = @object;
                Action = action;
            }
        }

        private Callee[] _callees = new Callee[0];

        public ComponentManager(string methodName, bool requireDeclaration = true)
        {
            _methodName = methodName;
            _requireDeclaration = requireDeclaration;
        }

        public void TryAdd(Component component, GameObject gobj)
        {
            var ctype = component.GetType();

            var info = ctype
                .GetMethod(_methodName, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public);
            if (info == null) return;
            if (_requireDeclaration)
                if (info.DeclaringType == typeof(Component)) return;

            _callees = _callees.Add(new Callee(component, gobj, Delegate.CreateDelegate(typeof(Action), component, info) as Action));
        }

        public void Run()
        {
            foreach (var callee in _callees)
            {
                try
                {
                    callee.Action();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, "{1} object {0}", callee.Object.Name, _methodName);
                }
            }
        }

        public void RemoveAll(GameObject gobj)
        {
            _callees = _callees.RemoveAll(callee => callee.Object == gobj);
        }

        public void Remove(Component component)
        {
            _callees = _callees.RemoveAll(callee => callee.Component == component);
        }

        #region remove helper methods
        public static void RemoveAll(GameObject gobj, ref Component[] components, ref GameObject[] objects)
        {
            var gobjs = new List<GameObject>(objects.Length);
            var comps = new List<Component>(components.Length);
            for (var i = 0; i < gobjs.Count; i++)
            {
                if (ReferenceEquals(objects[i], gobj)) continue;
                gobjs.Add(gobjs[i]);
                comps.Add(comps[i]);
            }

            objects = gobjs.ToArray();
            components = comps.ToArray();
        }

        public static void Remove(Component component, ref Component[] components, ref GameObject[] objects)
        {
            var gobjs = new List<GameObject>(objects.Length);
            var comps = new List<Component>(components.Length);
            for (var i = 0; i < gobjs.Count; i++)
            {
                if (ReferenceEquals(component, components[i])) continue;
                gobjs.Add(gobjs[i]);
                comps.Add(comps[i]);
            }

            objects = gobjs.ToArray();
            components = comps.ToArray();
        }
        #endregion
    }
}
