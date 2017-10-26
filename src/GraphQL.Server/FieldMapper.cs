using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using GraphQL.Language.AST;
using GraphQL.Server.Security;
using GraphQL.Types;

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
            var fieldName = propertyInfo.Name.ToCamelCase();
            var fieldDescription = "";
            var authFieldName = $"{type.FullName}.{propertyInfo.Name}";
            var sourceType = type.BaseType?.GenericTypeArguments.FirstOrDefault() ?? type;
            QueryArguments arguments = null;

            Func<ResolveFieldContext<object>, object> contextResolve;
            if (methodInfo != null)
            {
                arguments = GetPropertyArguments(sourceType, methodInfo);
                // Custom mapping of property
                contextResolve = context =>
                {
                    AuthorizeProperty(container, authFieldName);
                    var sourceResolverInfo = container.GetInstance<ResolverInfoManager>().Create(context).First();
                    var output = methodInfo.Invoke(obj, GetArgumentValues(methodInfo, container, context, sourceResolverInfo));
                    output = container.GetInstance<ApiSchema>().PropertyFilterManager.Filter(context, propertyInfo, authFieldName, output);
                    var baseType = TypeLoader.GetBaseType(output?.GetType(), out var isList);
                    if (output != null && !baseType.IsValueType)
                    {
                        container.GetInstance<ResolverInfoManager>().Create(context, output, sourceResolverInfo);
                    }
                    return output;
                };
            }
            else
            {
                // 1 to 1 mapping of property to source
                contextResolve = context =>
                {
                    AuthorizeProperty(container, authFieldName);
                    var properties = context.Source.GetType().GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
                    var sourceProp = properties.FirstOrDefault(p => p.Name == propertyInfo.Name);
                    if (sourceProp == null) throw new ArgumentException($"No matching source property found for GraphObject. Type: {type.Name} Property: {propertyInfo.Name}");
                    var output = sourceProp.GetValue(context.Source);
                    output = container.GetInstance<ApiSchema>().PropertyFilterManager.Filter(context, propertyInfo, authFieldName, output);
                    var baseType = TypeLoader.GetBaseType(output?.GetType(), out var isList);
                    if (output != null && !baseType.IsValueType)
                    {
                        var sourceResolverInfo = container.GetInstance<ResolverInfoManager>().Create(context).First();
                        container.GetInstance<ResolverInfoManager>().Create(context, output, sourceResolverInfo);
                    }
                    return output;
                };
            }
            var graphType = TypeLoader.GetGraphType(fieldType);
            var nonNull = Enumerable.Any(propertyInfo.CustomAttributes, a => a.AttributeType == typeof(RequiredAttribute));
            if (nonNull)
            {
                graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
            }
            var field = obj.Field(graphType, fieldName, fieldDescription, arguments, contextResolve);
            //field.ResolvedType = (IGraphType)Activator.CreateInstance(graphType);
            container.GetInstance<AuthorizationMap>().AddAuthorization(type, propertyInfo);
        }

        private static QueryArguments GetPropertyArguments(Type sourceType, MethodInfo methodInfo)
        {
            var args = new List<QueryArgument>();
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parameterInfo.ParameterType.IsAssignableFrom(sourceType)
                    || parameterInfo.ParameterType.IsAssignableFrom(typeof(IContainer))
                    || parameterInfo.ParameterType.IsAssignableFrom(typeof(IEnumerable<Field>))
                    || typeof(ResolverInfo).IsAssignableFrom(parameterInfo.ParameterType)) continue;
                var parameterGraphType = TypeLoader.GetGraphType(parameterInfo.ParameterType);
                object defaultValue = null;
                if (parameterInfo.HasDefaultValue)
                {
                    defaultValue = parameterInfo.DefaultValue;
                }
                else
                {
                    parameterGraphType = typeof(NonNullGraphType<>).MakeGenericType(parameterGraphType);
                }
                var argument = new QueryArgument(parameterGraphType)
                {
                    Name = parameterInfo.Name.ToCamelCase(),
                    DefaultValue = defaultValue
                };
                args.Add(argument);
            }
            return args.Count > 0 ? new QueryArguments(args) : null;
        }

        private static void AuthorizeProperty(IContainer container, string authFieldName)
        {
            var permissions = container.GetInstance<UserPermissions>().Permissions;
            if (!container.GetInstance<AuthorizationMap>().Authorize(authFieldName, permissions))
            {
                throw new AuthorizationException(authFieldName);
            }
        }

        private static object[] GetArgumentValues(MethodInfo methodInfo, IContainer container, ResolveFieldContext<object> context, ResolverInfo resolverInfo)
        {
            var arguments = new List<object>();
            var sourceType = context.Source.GetType();
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(IContainer)) arguments.Add(container);
                else if (parameterInfo.ParameterType == typeof(IEnumerable<Field>)) arguments.Add(context.FieldAst.SelectionSet.Selections.OfType<Field>());
                else if (parameterInfo.ParameterType.IsAssignableFrom(sourceType)) arguments.Add(context.Source);
                else if (typeof(ResolverInfo).IsAssignableFrom(parameterInfo.ParameterType)) arguments.Add(resolverInfo);
                else
                {
                    var argName = parameterInfo.Name.ToCamelCase();
                    var argValue = context.Arguments.ContainsKey(argName) ? context.Arguments[argName] : null;
                    arguments.Add(argValue);
                }
            }
            return arguments.ToArray();
        }

        public static void AddFields<TOutput>(IContainer container, ComplexGraphType<object> obj) where TOutput : class
        {
            var outputType = typeof(TOutput);
            var objectType = obj.GetType();
            var filter = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            var methods = objectType.GetMethods(filter);
            var objectTypeProperties = objectType.GetProperties(filter);
            foreach (var propertyInfo in objectTypeProperties)
            {
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    AddField(container, obj, outputType, propertyInfo, methods.FirstOrDefault(m => m.Name == $"Get{propertyInfo.Name}"));
                }
            }
            foreach (var propertyInfo in outputType.GetProperties(filter))
            {
                //Skip properties that are already defined in the object type
                if (objectTypeProperties.Any(p => p.Name == propertyInfo.Name)) continue;
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    AddField(container, obj, outputType, propertyInfo, methods.FirstOrDefault(m => m.Name == $"Get{propertyInfo.Name}"));
                }
            }
        }
    }
}
