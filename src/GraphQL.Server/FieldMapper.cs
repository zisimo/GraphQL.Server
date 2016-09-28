using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GraphQL.Server.Security;

namespace GraphQL.Server
{
    public class FieldMapper
    {
        public static void AddAllFields(IContainer container, ComplexGraphType<Object> obj, Type type, bool declaredPropertiesOnly)
        {
            var filter = BindingFlags.Instance | BindingFlags.Public;
            if (declaredPropertiesOnly) filter = filter | BindingFlags.DeclaredOnly;
            var methods = type.GetMethods(filter);
            foreach (var propertyInfo in type.GetProperties(filter))
            {
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    AddField(container, obj, type, propertyInfo, methods.FirstOrDefault(m => m.Name == $"Get{propertyInfo.Name}"));
                }
            }
        }

        public static void AddField(IContainer container, ComplexGraphType<Object> obj, Type type, PropertyInfo propertyInfo, MethodInfo methodInfo)
        {
            if (propertyInfo.PropertyType == typeof(IContainer)) return;
            var fieldType = propertyInfo.PropertyType;
            var fieldName = StringExtensions.PascalCase(propertyInfo.Name);
            var fieldDescription = "";
            var authFieldName = $"{type.FullName}.{propertyInfo.Name}";

            Func<ResolveFieldContext<object>, object> contextResolve;
            if (methodInfo != null)
            {
                // Custom mapping of property
                contextResolve = context =>
                {
                    AuthorizeProperty(container, authFieldName);
                    return methodInfo.Invoke(obj, GetArgumentsForMethod(methodInfo, container, context));
                };
            }
            else
            {
                // 1 to 1 mapping of property to source
                contextResolve = context =>
                {
                    AuthorizeProperty(container, authFieldName);
                    var properties = context.Source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var sourceProp = properties.FirstOrDefault(p => p.Name == propertyInfo.Name);
                    if (sourceProp == null) throw new ArgumentException($"No matching source property found for GraphObject. Type: {type.Name} Property: {propertyInfo.Name}");
                    return sourceProp.GetValue(context.Source);
                };
            }
            var graphType = TypeLoader.GetGraphType(fieldType);
            var nonNull = Enumerable.Any(propertyInfo.CustomAttributes, a => a.AttributeType == typeof(RequiredAttribute));
            if (nonNull)
            {
                graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
            }
            obj.Field(graphType, fieldName, fieldDescription, null, contextResolve);
            container.GetInstance<AuthorizationMap>().AddAuthorization(type, propertyInfo);
        }

        private static void AuthorizeProperty(IContainer container, string authFieldName)
        {
            if (!container.GetInstance<AuthorizationMap>().Authorize(authFieldName))
            {
                throw new AuthorizationException(authFieldName);
            }
        }

        private static object[] GetArgumentsForMethod(MethodInfo methodInfo, IContainer container, ResolveFieldContext<object> context)
        {
            var arguments = new List<object>();
            var sourceType = context.Source.GetType();
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(IContainer)) arguments.Add(container);
                else if (parameterInfo.ParameterType.IsAssignableFrom(sourceType)) arguments.Add(context.Source);
                else arguments.Add(null);
            }
            return arguments.ToArray();
        }
    }
}
