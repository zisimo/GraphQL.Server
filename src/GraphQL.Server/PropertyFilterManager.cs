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

        public void AddPropertyFilter<T>(Func<ResolveFieldContext<object>, PropertyInfo, string, T, T> filter)
        {
            PropertyFilters.Add((context, propertyInfo, name, value) =>
            {
                var filterType = typeof(T);
                var valueType = value.GetType();
                var isArray = valueType.IsArray;
                var isEnumerable = valueType != typeof(string) && valueType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable"));
                
                // Direct cast
                if (TypeCompatible(context, propertyInfo, filterType, valueType))
                {
                    value = filter(context, propertyInfo, name, (T)value);
                }
                // Nullable
                else if (valueType.IsGenericType
                    && typeof(Nullable<>).IsAssignableFrom(valueType.GetGenericTypeDefinition())
                    && filterType.IsAssignableFrom(valueType.GenericTypeArguments[0])
                    && propertyInfo.GetValue(context.Source) != null)
                {
                    value = filter(context, propertyInfo, name, (T)value);
                }
                // IEnumerable
                else if (isArray || isEnumerable)
                {
                    var baseType = isArray ? valueType.GetElementType() : valueType.GenericTypeArguments[0];

                    if (TypeCompatible(context, propertyInfo, filterType, baseType))
                    {
                        var extensionList = propertyInfo.GetValue(context.Source) as IEnumerable<object>;
                        foreach (var o in extensionList)
                        {
                            value = filter(context, propertyInfo, name, (T) o);
                        }
                    }
                }
                return value;
            });
        }

        private bool TypeCompatible(ResolveFieldContext<object> context, PropertyInfo propertyInfo, Type filterType, Type valueType)
        {
            // Direct
            if (filterType.IsAssignableFrom(valueType)) return true;
            // Nullable
            if (filterType.IsGenericType
                && typeof(Nullable<>).IsAssignableFrom(valueType.GetGenericTypeDefinition())
                && filterType.IsAssignableFrom(valueType.GenericTypeArguments[0])
                && propertyInfo.GetValue(context.Source) != null) return true;
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
