using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GraphQL.Server.Exceptions;

namespace GraphQL.Server
{
    public class TypeLoader
    {
        private static Dictionary<string, Type> _resourceTypes;

        private static Dictionary<string, Assembly> _assemblies;
        private static Dictionary<string, Assembly> Assemblies
        {
            get
            {
                if (_assemblies == null) _assemblies = new Dictionary<string, Assembly>();
                return _assemblies;
            }
        }

        public static Type[] ExcludedTypes = new [] { typeof(GraphInputObject<>), typeof(GraphObject<>), typeof(IContainer) };
        public static Dictionary<string, Type> ResourceTypes
        {
            get
            {
                if (_resourceTypes == null)
                {
                    _resourceTypes = new Dictionary<string, Type>();
                }
                return _resourceTypes;
            }
        }
        public static Dictionary<Type, Type> BasicTypeMappings = new Dictionary<Type, Type>()
        {
            { typeof(object), typeof(StringGraphType) },
            { typeof(string), typeof(StringGraphType) },
            { typeof(bool), typeof(BooleanGraphType) },
            { typeof(int), typeof(IntGraphType) },
            { typeof(DateTime), typeof(DateGraphType) },
            { typeof(Guid), typeof(Types.GuidGraphType) },
            { typeof(long), typeof(Types.LongGraphType) },
            { typeof(double), typeof(Types.DoubleGraphType) },
            { typeof(decimal), typeof(DecimalGraphType) }
        };

        public static Type GetGraphType(Type type, bool inputType = false)
        {
            Type output = null;
            string resourceName = null;
            var isList = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) type = type.GenericTypeArguments[0];
            if (type.IsArray)
            {
                isList = true;
                type = type.GetElementType();
            }
            if (type != typeof(string) && type.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")))
            {
                isList = true;
                type = type.GenericTypeArguments[0];
            }
            if (TypeLoader.BasicTypeMappings.ContainsKey(type)) output = TypeLoader.BasicTypeMappings[type];
            else if (type.IsEnum) resourceName = $"{type.Name}Enum";
            else if (type.IsInterface || type.IsAbstract) resourceName = $"{type.Name}Interface";
            else if (type.IsClass)
            {
                resourceName = $"{type.Name}Object";
                if (!TypeLoader.ResourceTypes.ContainsKey(resourceName) && inputType)
                {
                    TypeLoader.ResourceTypes[resourceName] = typeof(GraphInputObject<>).MakeGenericType(type);
                }
            }

            if (resourceName != null && TypeLoader.ResourceTypes.ContainsKey(resourceName)) output = TypeLoader.ResourceTypes[resourceName];
            if (output == null) throw new GraphException($"No resource mapping found for {type.FullName}");
            if (isList)
            {
                output = typeof(ListGraphType<>).MakeGenericType(output);
            }
            return output;
        }

        public static bool IsGraphClass(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (GraphObject<>);
        }

        public static Type GetGraphClassEntity(Type type)
        {
            return type.GenericTypeArguments[0];
        }

        public static void LoadTypes(Assembly assembly)
        {
            Assemblies[assembly.FullName] = assembly;
            var types = assembly.ExportedTypes.Where(t => typeof(GraphType).IsAssignableFrom(t) && !ExcludedTypes.Contains(t)).ToDictionary(t => t.Name, t => t);
            foreach (var type in types)
            {
                if (ResourceTypes.ContainsKey(type.Key)) continue;
                ResourceTypes.Add(type.Key, type.Value);
            }
            //var tmpTypes = ResourceTypes;
        }

        public static void LoadOperations(IContainer container, Assembly assembly, ApiSchema schema)
        {
            var types = assembly.ExportedTypes.Where(t => typeof(IOperation).IsAssignableFrom(t)).ToDictionary(t => t.Name, t => t);
            foreach (var type in types)
            {
                var operation = (IOperation) container.GetInstance(type.Value);
                operation.Register(schema);
            }
        }

        public static void InitializeTypes(IContainer container)
        {
            foreach (var type in ResourceTypes)
            {
                var baseType = type.Value.BaseType.IsGenericType ? type.Value.BaseType.GetGenericTypeDefinition() : type.Value.BaseType;
                var isGraphObject = baseType == typeof(GraphObject<>) || baseType == typeof(GraphInterface<>);
                var isGraphInput = type.Value.IsGenericType && type.Value.GetGenericTypeDefinition() == typeof(GraphInputObject<>);
                if (isGraphObject || isGraphInput)
                {
                    Activator.CreateInstance(type.Value, container);
                }
                else
                {
                    Activator.CreateInstance(type.Value);
                }
            }
        }
    }
}