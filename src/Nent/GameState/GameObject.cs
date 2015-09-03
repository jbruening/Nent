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
        [YamlSerialize(YamlSerializeMethod.Never)]
        public readonly GameState GameState;
        /// <summary>
        /// create a new game object
        /// </summary>
        internal GameObject(GameState gameState, int id)
        {
            Id = id;
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
            set { _resource = value; }
        }

        private Vector3 _position;
        private Quaternion _rotation;
        private Matrix _rotationMatrix;
        private Matrix _matrix;
        private Matrix _inverseMatrix;

        /// <summary>
        /// world position
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdatePosition();
            }
        }

        /// <summary>
        /// world rotation
        /// </summary>
        public Quaternion Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                UpdateRotation();
            }
        }

        /// <summary>
        /// localized forward
        /// </summary>
        public Vector3 Forward { get; private set; }

        /// <summary>
        /// localized right
        /// </summary>
        public Vector3 Right { get; private set; }

        /// <summary>
        /// local -> global matrix
        /// </summary>
        public Matrix Matrix
        {
            get { return _matrix; }
        }

        /// <summary>
        /// global -> local matrix
        /// </summary>
        public Matrix InverseMatrix
        {
            get { return _inverseMatrix; }
        }

        /// <summary>
        /// local -> global rotation matrix
        /// </summary>
        public Matrix RotationMatrix
        {
            get { return _rotationMatrix; }
        }

        void UpdatePosition()
        {
            UpdateMatrices();
        }

        void UpdateRotation()
        {
            Matrix.RotationQuaternion(ref _rotation, out _rotationMatrix);
            //todo: is this correct/faster/slower compared to TransformNormal(_rotationMatrix)?
            Forward = _rotation.Multiply(Vector3.UnitZ);
            Right = _rotation.Multiply(Vector3.UnitX);
            
            UpdateMatrices();
        }

        void UpdateMatrices()
        {
            Matrix.Translation(ref _position, out _matrix);
            _matrix = _rotationMatrix * _matrix;
            _inverseMatrix = Matrix.Translation(-_position) * Matrix.Transpose(_rotationMatrix);
        }

        private bool _markedForDestruction;
        /// <summary>
        /// Destroy the specified GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(GameObject gameObject)
        {
            gameObject.Destroy();
        }

        /// <summary>
        /// Destroy this GameObject
        /// </summary>
        public void Destroy()
        {
            //prevent destroy from being called on a gameobject multiple times enqueing
            if (_markedForDestruction) return;
            _markedForDestruction = true;
            GameState.RemoveObject(this);
        }

        internal void DestroyNow()
        {
            _markedForDestruction = true; //just in case
            OnDestroy();
            try
            {
                components.ForEach(g => g.Dispose());
            }
            catch (Exception e)
            {
                Debug.LogException(e, "Error disposing {0}", this);
            }
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
                return "Nent.GameObject";
            return "GameObject " + Name;
        }

        /// <summary>
        /// whether or not this has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
    }
}
