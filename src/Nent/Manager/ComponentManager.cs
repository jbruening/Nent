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
        private readonly List<Callee> _disabled = new List<Callee>();

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
            _callees = _callees.RemoveAll(c => c.Object, gobj);
            _disabled.RemoveAll(c => c.Object == gobj);
        }

        public void Remove(Component component)
        {
            _callees = _callees.RemoveAll(c => c.Component, component);
            _disabled.RemoveAll(c => c.Component == component);
        }

        public void Enable(Component component)
        {
            for (int i = _disabled.Count - 1; i >= 0; i--)
            {
                var idx = _disabled[i];
                if (idx.Component != component) continue;
                
                _disabled.RemoveAt(i);
                _callees = _callees.Add(idx);
            }
        }
        public void Disable(Component component)
        {
            _callees = _callees.RemoveAll(c => c.Component, component, _disabled);
        }
    }
}
