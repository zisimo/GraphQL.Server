using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GraphQL.Server
{
    public class Map
    {
        public static T Extend<T>(object extension, InputField[] fields) where T : class, new()
        {
            return Extend<T>(new T(), extension, fields);
        }

        public static T Extend<T>(T source, object extension, InputField[] fields) where T : class, new()
        {
            return Extend((object)source, extension, fields) as T;
        }
        private static object Extend(object source, object extension, InputField[] fields)
        {
            if (fields == null) return null;
            var sourceProps = source.GetType().GetProperties();
            var extensionProps = extension.GetType().GetProperties();
            foreach (var field in fields)
            {
                var propName = field.Name.ToPascalCase();
                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == propName);
                if (sourceProp == null && propName.EndsWith("Id"))
                {
                    sourceProp = sourceProps.FirstOrDefault(p => p.Name == propName.Replace("Id", "_Id"));
                }
                var extensionProp = extensionProps.FirstOrDefault(p => p.Name == propName);
                if (sourceProp != null && extensionProp != null)
                {
                    if (sourceProp.PropertyType.IsAssignableFrom(extensionProp.PropertyType))
                    {
                        sourceProp.SetValue(source, extensionProp.GetValue(extension));
                        continue;
                    }
                    if (extensionProp.PropertyType.IsGenericType
                        && typeof(Nullable<>).IsAssignableFrom(extensionProp.PropertyType.GetGenericTypeDefinition())
                        && sourceProp.PropertyType.IsAssignableFrom(extensionProp.PropertyType.GenericTypeArguments[0])
                        && extensionProp.GetValue(extension) != null)
                    {
                        sourceProp.SetValue(source, extensionProp.GetValue(extension));
                        continue;
                    }

                    if (sourceProp.PropertyType.IsPrimitive) continue;
                    if (extensionProp.PropertyType.IsEnum && typeof(string).IsAssignableFrom(sourceProp.PropertyType))
                    {
                        sourceProp.SetValue(source, extensionProp.GetValue(extension).ToString());
                        continue;
                    }

                    var sourceObj = sourceProp.GetValue(source) ?? Activator.CreateInstance(sourceProp.PropertyType);
                    var isArray = sourceProp.PropertyType.IsArray;
                    var isEnumerable = sourceProp.PropertyType != typeof(string) && sourceProp.PropertyType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable"));
                    if (isArray || isEnumerable)
                    {
                        if (field.Fields.Length == 0) continue;
                        var baseType = isArray ? sourceProp.PropertyType.GetElementType() : sourceProp.PropertyType.GenericTypeArguments[0];
                        var listType = typeof(List<>).MakeGenericType(baseType);
                        var list = Activator.CreateInstance(listType);
                        var sourceList = sourceObj as IEnumerable<object>;
                        var extensionList = extensionProp.GetValue(extension) as IEnumerable<object>;
                        var index = 0;
                        var idFieldAvailable = field.Fields[index].Fields.Any(fv => fv.Name.ToLower() == "id");
                        foreach (var item in extensionList)
                        {
                            object newItem = null;
                            if (idFieldAvailable)
                            {
                                newItem = FindMatchingObjectById(sourceList, baseType, item);
                            }
                            if (newItem == null)
                            {
                                newItem = Activator.CreateInstance(baseType);
                            }
                            newItem = Extend(newItem, item, field.Fields[index].Fields);
                            listType.GetMethod("Add").Invoke(list, new[] { newItem });
                            index++;
                        }
                        sourceObj = list;
                    }
                    else
                    {
                        sourceObj = Extend(sourceObj, extensionProp.GetValue(extension), field.Fields);
                    }
                    sourceProp.SetValue(source, sourceObj);
                }
            }
            return source;
        }

        public static T Extend<T>(object obj)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, jsonSerializerSettings));
        }

        private static object FindMatchingObjectById(IEnumerable<object> sourceList, Type sourceType, object item)
        {
            var sourceProp = sourceType.GetProperties().FirstOrDefault(p => p.Name.ToLower() == "id");
            var itemProp = item.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == "id");
            if (sourceProp == null || itemProp == null) return null;
            var itemVal = itemProp.GetValue(item);
            return sourceList.FirstOrDefault(sourceItem => sourceProp.GetValue(sourceItem).Equals(itemVal));
        }
    }
}
