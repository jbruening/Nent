using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Yaml;
using System.Yaml.Serialization;
using SlimMath;

namespace Nent
{
    /// <summary>
    /// class for loading prefabs
    /// </summary>
    public static class Resources
    {
        private static List<Type> componentTypes = new List<Type>();
        private static IEnumerable<Type> GetComponentTypes()
        {
            if (componentTypes.Count == 0)
            {
                Type componentType = typeof(Component);
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var assemblyComponentTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(componentType));
                        componentTypes.AddRange(assemblyComponentTypes);
                    }
                    catch (ReflectionTypeLoadException exception)
                    {
                        var sb = new StringBuilder();
                        foreach (var type in exception.LoaderExceptions)
                        {
                            sb.AppendLine(type.ToString());
                        }
                        Debug.LogError("Resources GetComponentTypes failed for the following types: {0}", sb);
                    }
                }
            }
            return componentTypes;
        }

        /// <summary>
        /// the folder that Resources.Load pulls from. by default, it is a folder next to PNetS.dll called Resources
        /// </summary>
        public static string ResourceFolder;

        static Resources()
        {
            ResourceFolder = Path.Combine(Assembly.GetAssembly(typeof(Resources)).Location, "Resources");
        }

        /// <summary>
        /// Load a gameobject from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="state">state that the gameobject will be in</param>
        /// <param name="position"> </param>
        /// <param name="rotation"> </param>
        /// <param name="visibleToAll">makes all players in the room subscribed to the object</param>
        /// <returns></returns>
        public static GameObject Load(string filePath, GameState state, Vector3? position = null, Quaternion? rotation = null, bool visibleToAll = true)
        {
            var dser = state.CreateNewGameObject();
            var awakes = new List<Component>();
            var config = new YamlConfig();
            var actualFilePath = Path.Combine(ResourceFolder, filePath + ".prefab");

            config.AddActivator<GameObject>(() =>
            {
                //be returning an object we've already created, the AddComponent will work properly
                return dser;
            });

            config.AddActivator<Component>(() =>
            {
                throw new TypeLoadException(
                    "Attempted to create a Nent.Component in " + filePath + ", but that's not allowed. Did you declare your yaml correctly?");
            });

            config.TypeResolutionError += (yamlConfig, type, arg3) =>
            {
                if (type == typeof (Component))
                {
                    throw new TypeLoadException("Could not find the component type " + arg3.Substring(1) + ". You might have your yaml declared incorrectly, or this is an undefined type");
                }
                else
                {
                    Debug.LogError("Yaml type resolution error. Expecting " + type + " for tag " + arg3);
                }
            };
            
            foreach (Type t in GetComponentTypes())
            {
                Type tLocal = t;
                GameObject dser1 = dser;
                config.AddActivator(tLocal, () =>
                {
                    var ret = dser1.DeserializeAddComponent(tLocal);
                    awakes.Add(ret);
                    return ret;
                });
            }

            var serializer = new YamlSerializer(config);
            serializer.DeserializeFromFile(actualFilePath, typeof(GameObject));

            if (dser.Resource == null)
                dser.Resource = filePath;

            if (position.HasValue)
                dser.Position = position.Value;
            if (rotation.HasValue)
                dser.Rotation = rotation.Value;

            foreach (var awake in awakes)
                if (awake != null) awake.InternalAwakeCall();

            dser.OnComponentAfterDeserialization();

            return dser;
        }
    }
}
