using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            { typeof(Uri), typeof(Types.UriGraphType) },
            { typeof(long), typeof(Types.LongGraphType) },
            { typeof(double), typeof(Types.DoubleGraphType) },
            { typeof(decimal), typeof(DecimalGraphType) }
        };

        public static Type GetGraphType(Type type, bool inputType = false)
        {
            type = GetBaseType(type, out bool isList);

            Type graphType;
            if (TypeLoader.BasicTypeMappings.ContainsKey(type))
            {
                graphType = TypeLoader.BasicTypeMappings[type];
            }
            else
            {
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
                graphType = TypeMappings[type.FullName].GraphType;
            }
            return isList ? typeof(ListGraphType<>).MakeGenericType(graphType) : graphType;
        }

        public static TypeMapping AddType(Type type, Type graphType)
        {
            if (ExcludedTypes.Contains(graphType)) return null;
            if (TypeMappings.ContainsKey(type.FullName))
            {
                Debug.WriteLine($"Overwriting TypeMapping for type {type.FullName}");
            }

            var typeMapping = new TypeMapping()
            {
                TypeName = type.FullName,
                Type = type,
                GraphType = graphType
            };
            TypeMappings[typeMapping.TypeName] = typeMapping;
            return typeMapping;
        }

        public static Type GetBaseType(Type type, out bool isList)
        {
            if (type == null)
            {
                isList = false;
                return null;
            }
            isList = false;
            var baseType = type;
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Nullable<>)) baseType = baseType.GenericTypeArguments[0];
            if (baseType.IsArray)
            {
                isList = true;
                baseType = baseType.GetElementType();
            }
            if (baseType != typeof(string) && baseType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")))
            {
                isList = true;
                baseType = baseType.GenericTypeArguments[0];
            }
            return baseType;
        }

        public class TypeMapping
        {
            public string TypeName { get; set; }
            public Type Type { get; set; }
            public Type GraphType { get; set; }
        }

        public static bool TypeLoaded(Type type)
        {
            return TypeLoader.BasicTypeMappings.ContainsKey(type) || TypeMappings.ContainsKey(type.FullName);
        }

        public static bool IsBasicType(Type type)
        {
            return TypeLoader.BasicTypeMappings.ContainsKey(type);
        }
    }
}