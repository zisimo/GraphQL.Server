using System;
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
            if (ExcludedTypes.Contains(graphType)) return null;
            var typeMapping = new TypeMapping()
            {
                TypeName = type.FullName,
                Type = type,
                GraphType = graphType
            };
            //TypeMappings.Add(typeMapping.TypeName, typeMapping);
            TypeMappings[typeMapping.TypeName] = typeMapping;
            return typeMapping;
        }

        public class TypeMapping
        {
            public string TypeName { get; set; }
            public Type Type { get; set; }
            public Type GraphType { get; set; }
        }
    }
}