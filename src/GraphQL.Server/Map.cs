﻿using System;
using System.Collections.Generic;
using System.Linq;

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
                var propName = StringExtensions.CamelCase(field.Name);
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
                    if (extensionProp.PropertyType.IsEnum && typeof (string).IsAssignableFrom(sourceProp.PropertyType))
                    {
                        sourceProp.SetValue(source, extensionProp.GetValue(extension).ToString());
                        continue;
                    }

                    var sourceObj = sourceProp.GetValue(source) ?? Activator.CreateInstance(sourceProp.PropertyType);
                    var isArray = sourceProp.PropertyType.IsArray;
                    var isEnumerable = sourceProp.PropertyType != typeof(string) && sourceProp.PropertyType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable"));
                    if (isArray || isEnumerable)
                    {
                        var baseType = isArray ? sourceProp.PropertyType.GetElementType() : sourceProp.PropertyType.GenericTypeArguments[0];
                        var listType = typeof(List<>).MakeGenericType(baseType);
                        var list = Activator.CreateInstance(listType);
                        var extensionList = extensionProp.GetValue(extension) as IEnumerable<object>;
                        var index = 0;
                        foreach (var item in extensionList)
                        {
                            var newItem = Activator.CreateInstance(sourceProp.PropertyType.GenericTypeArguments[0]);
                            newItem = Extend(newItem, item, field.Fields[index].Fields);
                            listType.GetMethod("Add").Invoke(list, new[] { newItem });
                            index++;
                        }
                        sourceObj = list;
                        //sourceProp.PropertyType.GetMethod("Concat").Invoke(sourceObj, new[] { list });;
                        //sourceObj = (sourceObj as IEnumerable<object>).Concat(list);
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
    }
}
