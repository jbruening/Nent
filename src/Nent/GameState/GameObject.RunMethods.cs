using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nent
{

// ReSharper disable ForCanBeConvertedToForeach
    /// <summary>
    /// Basic entity that exists in the gamemachine
    /// </summary>
    public sealed partial class GameObject
    {
        internal void Update()
        {

            for (var i = 0; i < components.Count; i++)
            {
                components[i].InternalUpdateCall();
            }
        }

        internal void LateUpdate()
        {
            for (var i = 0; i < components.Count; i++)
            {
                components[i].InternalLateUpdateCall();
            }
        }

        internal void OnComponentAdded(Component component)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!object.ReferenceEquals(components[i], component))
                    components[i].InternalOnComponentAddedCall(component);
            }
        }

        internal void OnDestroy()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].InternalOnDestroyCall();
            }
        }
    }

// ReSharper restore ForCanBeConvertedToForeach
}
