using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Client;
using GraphQL.Server.Exceptions;
using GraphQL.Server.Operation;
using GraphQL.Server.Types;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ApiSchema : Schema
    {
        public IContainer Container { get; private set; }

        public new ApiOperation Query
        {
            get
            {
                if (base.Query == null) base.Query = new ApiOperation(Container, "Query");
                return (ApiOperation)base.Query;
            }
        }

        public new ApiOperation Mutation
        {
            get
            {
                if (base.Mutation == null) base.Mutation = new ApiOperation(Container, "Mutation");
                return (ApiOperation)base.Mutation;
            }
        }

        public PropertyFilterManager PropertyFilterManager { get; set; }

        public ApiSchema(IContainer container) : base(type => (GraphType)container.GetInstance(type))
        {
            Container = container;
            PropertyFilterManager = new PropertyFilterManager();
        }

        public void MapOutput(Type outputType, bool autoMapChildren, bool overwriteMap)
        {
            MapOutput(outputType, autoMapChildren, overwriteMap, new List<string>());
        }

        public void MapOutput(Type inputType, Type outputType, bool autoMapChildren, bool overwriteMap)
        {
            MapOutput(inputType, outputType, autoMapChildren, overwriteMap, new List<string>());
        }

        private void MapOutput(Type outputType, bool autoMapChildren, bool overwriteMap, List<string> typeNamesLoaded)
        {
            Type graphType, baseType;
            if (typeof(GraphObjectMap<,>).IsAssignableFrom(outputType))
            {
                graphType = outputType;
                baseType = outputType.BaseType.GenericTypeArguments.First();
                MapOutput(baseType, graphType, autoMapChildren, overwriteMap, typeNamesLoaded);
            }
            else
            {
                baseType = TypeLoader.GetBaseType(outputType, out bool isList);
                if (baseType.IsEnum || baseType.IsValueType) return;
                graphType = typeof(GraphObjectMap<>).MakeGenericType(baseType);
                MapOutput(baseType, graphType, autoMapChildren, overwriteMap, typeNamesLoaded);
            }
        }

        private void MapOutput(Type inputType, Type outputType, bool autoMapChildren, bool overwriteMap, List<string> typeNamesLoaded)
        {
            if (TypeLoader.IsBasicType(inputType) || typeNamesLoaded.Contains(inputType.FullName))
            {
                return;
            }
            if (!TypeLoader.TypeLoaded(inputType) || overwriteMap)
            {
                TypeLoader.AddType(inputType, outputType);
            }
            typeNamesLoaded.Add(inputType.FullName);
            if (autoMapChildren && inputType != typeof(string))
            {
                var filter = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
                foreach (var propertyInfo in inputType.GetProperties(filter))
                {
                    var propertyBaseType = TypeLoader.GetBaseType(propertyInfo.PropertyType, out bool isList);
                    if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic && propertyBaseType.FullName != inputType.FullName)
                    {
                        MapOutput(propertyInfo.PropertyType, autoMapChildren, overwriteMap, typeNamesLoaded);
                    }
                }
            }
        }

        public void MapOutput<TOutput>(bool autoMapChildren, bool overwriteMap)
            where TOutput : class
        {
            MapOutput(typeof(TOutput), autoMapChildren, overwriteMap);
        }

        public void MapOutput<TInput, TOutput>()
            where TInput : class
            where TOutput : class
        {
            MapOutput(typeof(TInput), typeof(TOutput), true, true);
        }

        public void MapOutputNamespace(Assembly assembly, string ns)
        {
            foreach (var type in assembly.ExportedTypes.Where(t => t.Namespace == ns))
            {
                MapOutput(type, true, true);
            }
        }

        public void MapAssemblies(params Assembly[] assemblies)
        {
            var types = new List<Type>();
            var operationTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                // GraphObject
                types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphObject<>) == t.BaseType.GetGenericTypeDefinition()));
                // GraphObjectMap
                types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphObjectMap<>) == t.BaseType.GetGenericTypeDefinition()));
                // GraphInputObject
                types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphInputObject<>) == t.BaseType.GetGenericTypeDefinition()));
                // GraphEnum
                types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphEnum<>) == t.BaseType.GetGenericTypeDefinition()));
                // GraphInterface
                types.AddRange(assembly.ExportedTypes.Where(t => t.BaseType != null && t.BaseType.IsGenericType && typeof(GraphInterface<>) == t.BaseType.GetGenericTypeDefinition()));

                // Operations
                operationTypes.AddRange(assembly.ExportedTypes.Where(t => typeof(IOperation).IsAssignableFrom(t)));
            }
            foreach (var type in types)
            {
                MapOutput(type.BaseType.GenericTypeArguments.First(), type, false, false);
            }
            foreach (var type in operationTypes)
            {
                MapOperation(type);
            }
        }

        public void Lock()
        {
            RegisterTypes(TypeLoader.TypeMappings.Values.Select(v => v.GraphType).ToArray());
        }

        public void AddPropertyFilter(Func<ResolveFieldContext<object>, PropertyInfo, string, object, object> filter)
        {
            PropertyFilterManager.AddPropertyFilter(filter);
        }

        public void AddPropertyFilter<T>(Func<ResolveFieldContext<object>, PropertyInfo, string, T, T> filter)
        {
            PropertyFilterManager.AddPropertyFilter(filter);
        }

        public ProxyOperation<T> Proxy<T>(Func<GraphClient> getGraphClient) where T : class
        {
            var proxy = new ProxyOperation<T>(getGraphClient);
            proxy.Register(this);
            return proxy;
        }

        public void MapOperation<TInterface>()
        {
            MapOperation(typeof(TInterface));
        }

        public void MapOperation(Type type)
        {
            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in methods)
            {
                if (methodInfo.IsSpecialName ||
                    methodInfo.Name == nameof(IOperation.Register) ||
                    methodInfo.ReturnType == typeof(void) ||
                    methodInfo.GetParameters().Length != 1) continue;

                MapOutput(methodInfo.ReturnType, true, false);
            }
            if (!Container.HasRegistration(type)) return;
            var instance = Container.GetInstance(type);
            if (!(instance is IOperation))
            {
                throw new GraphException($"The implementation of interface {type.Name} needs to implement interface {nameof(IOperation)}");
            }
            var operation = (IOperation)Container.GetInstance(type);
            operation.Register(this);
        }
    }
}