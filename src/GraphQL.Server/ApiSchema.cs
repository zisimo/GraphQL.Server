using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Client;
using GraphQL.Server.Operation;
using GraphQL.Server.Types;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ApiSchema : Schema
    {
        public IContainer Container { get; private set; }

        public ApiOperation Query
        {
            get
            {
                if (base.Query == null) base.Query = new ApiOperation(Container, "Query");
                return (ApiOperation)base.Query;
            }
        }

        public ApiOperation Mutation
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

        public void MapOutput(Type outputType)
        {
            var type = typeof(GraphObjectMap<>).MakeGenericType(outputType);
            TypeLoader.AddType(outputType, type);
        }

        public void MapOutput<TOutput>()
            where TOutput : class
        {
            MapOutput(typeof(TOutput));
        }

        public void MapOutput<TInput, TOutput>()
            where TInput : class
            where TOutput : class
        {
            var type = typeof(GraphObjectMap<,>).MakeGenericType(typeof(TInput), typeof(TOutput));
            TypeLoader.AddType(typeof(TInput), type);
        }

        public void MapOutputNamespace(Assembly assembly, string ns)
        {
            foreach (var type in assembly.ExportedTypes.Where(t => t.Namespace == ns))
            {
                var graphType = typeof(GraphObjectMap<>).MakeGenericType(type);
                TypeLoader.AddType(type, graphType);
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
                TypeLoader.AddType(type.BaseType.GenericTypeArguments.First(), type);
            }
            foreach (var type in operationTypes)
            {
                if (!Container.HasRegistration(type)) continue;
                var operation = (IOperation)Container.GetInstance(type);
                operation.Register(this);
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

        public ProxyOperation<T> Proxy<T>(Func<GraphClient<T>> getGraphClient) where T : class, IOperation
        {
            var proxy = new ProxyOperation<T>(getGraphClient);
            proxy.Register(this);
            return proxy;
        }

        public void MapOperation<TOperation>() where TOperation : IOperation
        {
            var type = typeof(TOperation);
            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in methods)
            {
                if (methodInfo.IsSpecialName ||
                    methodInfo.Name == nameof(IOperation.Register) ||
                    methodInfo.ReturnType == typeof(void) ||
                    methodInfo.GetParameters().Length != 1) continue;

                MapOutput(methodInfo.ReturnType);
            }
            var operation = (IOperation)Container.GetInstance(type);
            operation.Register(this);
        }
    }
}