using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Server.Types;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ApiSchema : Schema
    {
        public IContainer Container { get; private set; }
        public ApiOperation Query { get; private set; }
        public ApiOperation Mutation { get; private set; }

        public ApiSchema(IContainer container) : base(type => (GraphType)container.GetInstance(type))
        {
            Container = container;
            base.Query = Query = new ApiOperation(container, "Query");
            base.Mutation = Mutation = new ApiOperation(container, "Mutation");
        }

        public void MapOutput<TOutput>()
            where TOutput : class
        {
            var type = typeof(GraphObjectMap<>).MakeGenericType(typeof(TOutput));
            TypeLoader.AddType(typeof(TOutput), type);
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
                var operation = (IOperation)Container.GetInstance(type);
                operation.Register(this);
            }
        }

        public void Lock()
        {
            RegisterTypes(TypeLoader.TypeMappings.Values.Select(v => v.GraphType).ToArray());
        }
    }
}