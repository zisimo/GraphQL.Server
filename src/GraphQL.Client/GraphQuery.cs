using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class GraphQuery<T> : IGraphQuery
    {
        private Type[] _nonMapClasses = new []{typeof(string), typeof(Uri)};

        public string Operation { get; private set; }
        public IEnumerable<Field> Selections { get; private set; }
        public T Data { get; set; }

        public GraphQuery(string operation)
        {
            Operation = operation;
        }

        public GraphQuery(string operation, IEnumerable<Field> selections)
        {
            Operation = operation;
            Selections = selections;
        }

        public string GetSelectFields()
        {
            var output = Selections != null ? GetFieldsForSelections(Selections) : GetFieldsForType(typeof(T), new string[0]);
            return output;
        }

        public void SetOutput(JToken obj)
        {
            if (obj != null)
            {
                Data = obj.ToObject<T>();
            }
        }

        private string GetFieldsForSelections(IEnumerable<Field> selections)
        {
            var fields = new List<string>();
            foreach (var selection in selections)
            {
                var name = $"{PascalCase(selection.Name)}";
                if (selection.Arguments.Any())
                {
                    var arguments = selection.Arguments.Select(a => $"{PascalCase(a.Name)}:{JsonConvert.SerializeObject(a.Value)}");
                    name = $"{name}({string.Join(",", arguments)})";
                }
                if (selection.SelectionSet.Children.Any())
                {
                    fields.Add($"{name}{GetFieldsForSelections(selection.SelectionSet.Selections.OfType<Field>())}");
                    continue;
                }
                fields.Add(name);
            }
            return fields.Count > 0 ? $"{{{string.Join(" ", fields)}}}" : string.Empty;
        }

        private string GetFieldsForType(Type type, string[] parentTypeNames)
        {
            if (_nonMapClasses.Any(c => c.FullName == type.FullName)) return string.Empty;
            if (type.IsArray) return GetFieldsForType(type.GetElementType(), parentTypeNames);
            if (type != typeof(string) &&
                type.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")) &&
                type.GenericTypeArguments.Length > 0)
            {
                return GetFieldsForType(type.GenericTypeArguments[0], parentTypeNames);
            }
            var fields = new List<string>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                if (propertyType.FullName == type.FullName) continue;
                // Skip already processed types to prevent circular reference issues
                if (parentTypeNames.Contains(propertyType.FullName)) continue;

                if (propertyType.IsArray)
                {
                    fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType.GetElementType(), parentTypeNames)}");
                    continue;
                }
                if (_nonMapClasses.All(c => c.FullName != propertyType.FullName))
                {
                    if (propertyType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")))
                    {
                        fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType.GenericTypeArguments[0], parentTypeNames)}");
                        continue;
                    }
                    if (propertyType.IsClass || propertyType.IsInterface)
                    {
                        // Add the current property type as a parent that has already been mapped
                        var propertyParentTypeNames = new List<string>(parentTypeNames) { propertyType.FullName };
                        fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType, propertyParentTypeNames.ToArray())}");
                        continue;
                    }
                }
                fields.Add(PascalCase(propertyInfo.Name));
            }
            return fields.Count > 0 ? $"{{{string.Join(" ", fields)}}}" : string.Empty;
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
