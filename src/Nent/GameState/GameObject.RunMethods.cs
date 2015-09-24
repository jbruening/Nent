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
        internal void OnComponentAdded(Component component)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!object.ReferenceEquals(components[i], component))
                    components[i].InternalOnComponentAddedCall(component);
            }

            GameState.SubscribeComponent(component, this);
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
