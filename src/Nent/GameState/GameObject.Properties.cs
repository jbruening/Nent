using System.Collections.Generic;

namespace Nent
{
    public partial class GameObject
    {
        private readonly Dictionary<int, object> _properties = new Dictionary<int, object>();

        /// <summary>
        /// Set the value of the property with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetProperty<T>(GameObjectProperty<T> key, T value)
        {
            _properties[key.Key] = value;
        }

        /// <summary>
        /// get a property of the specified type for the specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue">the value to return,  if the key doesn't exist</param>
        /// <returns></returns>
        public T Property<T>(GameObjectProperty<T> key, T defaultValue = default(T))
        {
            T val;
            return TryProperty(key, out val) ? val : defaultValue;
        }

        /// <summary>
        /// attempt to get a property of the specified type for the specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryProperty<T>(GameObjectProperty<T> key, out T value)
        {
            object val;
            var res = _properties.TryGetValue(key.Key, out val);
            if (!res)
                value = default(T);
            else if (val != null)
                value = (T)val;
            else
                value = default(T);
            return res;
        }
    }

    public class GameObjectProperty<T>
    {
        public int Key { get; private set; }

        /// <summary>
        /// create the property with the specified key
        /// </summary>
        /// <param name="key"></param>
        public GameObjectProperty(int key)
        {
            Key = key;
        }

        /// <summary>
        /// create the property with propertyName's hashcode as the key
        /// </summary>
        /// <param name="propertyName"></param>
        public GameObjectProperty(string propertyName)
        {
            Key = propertyName.GetHashCode();
        }
    }
}
