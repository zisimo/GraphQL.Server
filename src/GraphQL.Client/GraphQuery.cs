using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class GraphQuery<T> : IGraphQuery
    {
        public string Operation { get; private set; }
        public T Data { get; set; }

        public GraphQuery(string operation)
        {
            Operation = operation;
        }

        public string GetSelectFields()
        {
            var output = GetFieldsForType(typeof(T));
            return output;
        }

        public void SetOutput(JToken obj)
        {
            if (obj != null)
            {
                Data = obj.ToObject<T>();
            }
        }

        private string GetFieldsForType(Type type)
        {
            if (type.IsArray) return GetFieldsForType(type.GetElementType());
            if (type != typeof(string) && type.GetInterfaces().Any(t => t.Name.Contains("IEnumerable"))) return GetFieldsForType(type.GenericTypeArguments[0]);
            var fields = new List<string>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                if (propertyType.IsArray)
                {
                    fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType.GetElementType())}");
                    continue;
                }
                if (propertyType != typeof(string))
                {
                    if (propertyType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")))
                    {
                        fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType.GenericTypeArguments[0])}");
                        continue;
                    }
                    if (propertyType.IsClass || propertyType.IsInterface)
                    {
                        fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType)}");
                        continue;
                    }
                }
                fields.Add(PascalCase(propertyInfo.Name));
            }
            return $"{{{string.Join(" ", fields)}}}";
        }

        private static string PascalCase(string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }
    }

    public interface IGraphQuery
    {
        string Operation { get; }
        string GetSelectFields();
        void SetOutput(JToken value);
    }
}
