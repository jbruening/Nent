using System.Collections.Generic;

namespace Nent
{
    public partial class GameObject
    {
        private readonly Dictionary<int, object> _properties = new Dictionary<int, object>();

        /// <summary>
        /// Set the value of the property with propertyName's hashcode as the key
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetProperty(string propertyName, object value)
        {
            SetProperty(propertyName.GetHashCode(), value);
        }

        /// <summary>
        /// Set the value of the property with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetProperty(int key, object value)
        {
            _properties[key] = value;
        }

        /// <summary>
        /// get a property with propertyName's hashcode as the key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue">the value to return, if the property doesn't exist</param>
        /// <returns></returns>
        public T Property<T>(string propertyName, T defaultValue = default(T))
        {
            return Property(propertyName.GetHashCode(), defaultValue);
        }

        /// <summary>
        /// attempt to get a property with propertyName's hashcode as the key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryProperty<T>(string propertyName, out T value)
        {
            return TryProperty(propertyName.GetHashCode(), out value);
        }

        /// <summary>
        /// get a property of the specified type for the specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue">the value to return,  if the key doesn't exist</param>
        /// <returns></returns>
        public T Property<T>(int key, T defaultValue = default(T))
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
        public bool TryProperty<T>(int key, out T value)
        {
            object val;
            var res = _properties.TryGetValue(key, out val);
            if (!res)
                value = default(T);
            else if (val != null)
                value = (T)val;
            else
                value = default(T);
            return res;
        }
    }
}
