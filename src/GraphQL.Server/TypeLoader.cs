using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GraphQL.Server.Exceptions;
using GraphQL.Server.Types;

namespace GraphQL.Server
{
    public class TypeLoader
    {
        private static Dictionary<string, TypeMapping> _typeMappings;

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
        public static Dictionary<string, TypeMapping> TypeMappings
        {
            get
            {
                if (_typeMappings == null)
                {
                    _typeMappings = new Dictionary<string, TypeMapping>();
                }
                return _typeMappings;
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
            if (TypeLoader.BasicTypeMappings.ContainsKey(type)) return TypeLoader.BasicTypeMappings[type];
            if (!TypeMappings.ContainsKey(type.FullName))
            {
                if (type.IsEnum)
                {
                    AddType(type, typeof(GraphEnum<>).MakeGenericType(type));
                }
                else if (inputType)
                {
                    AddType(type, typeof(GraphInputObject<>).MakeGenericType(type));
                }
                else
                {
                    throw new GraphException($"No TypeMapping mapping found for {type.FullName}");
                }
            }
            var typeMapping = TypeMappings[type.FullName];
            return isList ? typeof(ListGraphType<>).MakeGenericType(typeMapping.GraphType) : typeMapping.GraphType;
        }

        public static TypeMapping AddType(Type type, Type graphType)
        {
            var typeMapping = new TypeMapping()
            {
                TypeName = type.FullName,
                Type = type,
                GraphType = graphType
            };
            TypeMappings.Add(typeMapping.TypeName, typeMapping);
            return typeMapping;
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
            List<Type> types = new List<Type>();
            // GraphObject
            types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphObject<>) == t.BaseType.GetGenericTypeDefinition()));
            // GraphInputObject
            types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphInputObject<>) == t.BaseType.GetGenericTypeDefinition()));
            // GraphEnum
            types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphEnum<>) == t.BaseType.GetGenericTypeDefinition()));
            // GraphInterface
            types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphInterface<>) == t.BaseType.GetGenericTypeDefinition()));
            foreach (var type in types)
            {
                if (ExcludedTypes.Contains(type)) continue;
                AddType(type.BaseType.GenericTypeArguments.First(), type);
            }
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

        public class TypeMapping
        {
            public string TypeName { get; set; }
            public Type Type { get; set; }
            public Type GraphType { get; set; }
        }
    }
}