using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Yaml.Serialization;
using SlimMath;

namespace Nent
{
    public sealed partial class GameObject
    {
        public readonly GameState GameState;
        /// <summary>
        /// create a new game object
        /// </summary>
        internal GameObject(GameState gameState)
        {
            Id = -1;
            GameState = gameState;
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        [YamlSerialize(YamlSerializeMethod.Never)]
        public int Id { get; internal set; }

        [YamlSerialize(YamlSerializeMethod.Assign)]
        private string _resource;

        /// <summary>
        /// Name of this gameobject. Not necessarily unique, just an identifier
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// resource path this gameobject came from
        /// </summary>
        [YamlSerialize(YamlSerializeMethod.Never)]
        public string Resource
        {
            get { return _resource; }
            internal set { _resource = value; }
        }
        /// <summary>
        /// world position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// world rotation
        /// </summary>
        public Quaternion Rotation { get; set; }
        /// <summary>
        /// localized forward
        /// </summary>
        public Vector3 Forward { get { return Rotation.Multiply(Vector3.UnitZ); } }

        /// <summary>
        /// localized right
        /// </summary>
        public Vector3 Right { get { return Rotation.Multiply(Vector3.UnitX); } }


        private bool _markedForDestruction;
        /// <summary>
        /// Destroy this GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(GameObject gameObject)
        {
            //prevent destroy from being called on a gameobject multiple times enqueing
            if (gameObject._markedForDestruction) return;
            gameObject._markedForDestruction = true;
            gameObject.GameState.RemoveObject(gameObject);
        }

        internal void DestroyNow()
        {
            _markedForDestruction = true; //just in case
            OnDestroy();
            components.ForEach(g => g.Dispose());
            components = null;
            IsDisposed = true;
        }

        /// <summary>
        /// serialize this gameobject
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var serializer = new YamlSerializer();
            return serializer.Serialize(this);
        }

        /// <summary>
        /// tostring
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "PnetS.GameObject";
            return "GameObject " + Name;
        }

        /// <summary>
        /// whether or not this has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
    }
}
