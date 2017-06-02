using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class PropertyFilterManager
    {
        private List<Func<ResolveFieldContext<object>, PropertyInfo, string, object, object>> PropertyFilters { get; set; }

        public PropertyFilterManager()
        {
            PropertyFilters = new List<Func<ResolveFieldContext<object>, PropertyInfo, string, object, object>>();
        }

        public void AddPropertyFilter(Func<ResolveFieldContext<object>, PropertyInfo, string, object, object> filter)
        {
            PropertyFilters.Add(filter);
        }

        public void AddPropertyFilter<T>(Func<ResolveFieldContext<object>, PropertyInfo, string, T, T> filter)
        {
            PropertyFilters.Add((context, propertyInfo, name, value) =>
            {
                var filterType = typeof(T);
                var valueType = value != null ? value.GetType() : typeof(object);
                var isArray = valueType.IsArray;
                var isEnumerable = valueType != typeof(string) && valueType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable"));
                
                // Direct cast
                if (TypeCompatible(filterType, valueType, value))
                {
                    value = filter(context, propertyInfo, name, (T)value);
                }
                // Nullable
                else if (valueType.IsGenericType
                    && typeof(Nullable<>).IsAssignableFrom(valueType.GetGenericTypeDefinition())
                    && filterType.IsAssignableFrom(valueType.GenericTypeArguments[0])
                    && value != null)
                {
                    value = filter(context, propertyInfo, name, (T)value);
                }
                // IEnumerable
                else if (isArray || isEnumerable)
                {
                    var baseType = isArray ? valueType.GetElementType() : valueType.GenericTypeArguments[0];

                    if (TypeCompatible(filterType, baseType, value))
                    {
                        var readList = value as IEnumerable<object>;
                        var listType = typeof(List<>).MakeGenericType(baseType);
                        var writeList = Activator.CreateInstance(listType);
                        foreach (var o in readList)
                        {
                            var item = (object) filter(context, propertyInfo, name, (T) o);
                            listType.GetMethod("Add").Invoke(writeList, new[] { item });
                        }
                        value = (IEnumerable<T>)writeList;
                    }
                }
                return value;
            });
        }

        private bool TypeCompatible(Type filterType, Type valueType, object value)
        {
            // Direct
            if (filterType.IsAssignableFrom(valueType)) return true;
            // Nullable
            if (filterType.IsGenericType
                && typeof(Nullable<>).IsAssignableFrom(valueType.GetGenericTypeDefinition())
                && filterType.IsAssignableFrom(valueType.GenericTypeArguments[0])
                && value != null) return true;
            return false;
        }

        public object Filter(ResolveFieldContext<object> context, PropertyInfo propertyInfo, string name, object value)
        {
            foreach (var filter in PropertyFilters)
            {
                value = filter(context, propertyInfo, name, value);
            }
            return value;
        }
    }
}
