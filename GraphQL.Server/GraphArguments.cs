using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class GraphArguments : Dictionary<string, GraphArgument>
    {
        public static GraphArguments FromModel<TInput>()
            where TInput : class, new()
        {
            var arguments = new GraphArguments();
            foreach (var propertyInfo in typeof(TInput).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var fieldName = char.ToLower(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1);
                if (arguments.ContainsKey(fieldName)) continue;
                var argument = new GraphArgument()
                {
                    Name = fieldName,
                    Type = propertyInfo.PropertyType,
                    GraphType = TypeLoader.GetGraphType(propertyInfo.PropertyType, inputType: true),
                    NonNull = propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute))
                };
                if (argument.NonNull)
                {
                    argument.GraphType = typeof(NonNullGraphType<>).MakeGenericType(argument.GraphType);
                }
                arguments.Add(fieldName, argument);
                LoadChildGraphTypes(propertyInfo.PropertyType);
            }
            return arguments;
        }

        public QueryArguments GetQueryArguments()
        {
            var queryArguments = new List<QueryArgument>();
            foreach (var item in this)
            {
                queryArguments.Add(new QueryArgument(item.Value.GraphType) { Name = item.Value.Name, Description = "" });
            }
            return new QueryArguments(queryArguments);
        }

        /// <summary>
        /// This is to load all child properties as Input Types for GraphQL as all arguments must of of input type
        /// </summary>
        /// <param name="type"></param>
        private static void LoadChildGraphTypes(Type type)
        {
            if (type == typeof(string) || type == typeof(DateTime)) return;
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                TypeLoader.GetGraphType(propertyInfo.PropertyType, inputType: true);
                LoadChildGraphTypes(propertyInfo.PropertyType);
            }
        }
    }

    public class GraphArgument
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public Type GraphType { get; set; }
        public bool NonNull { get; set; }
    }
}