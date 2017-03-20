using System.Linq;
using System.Reflection;
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

        public void AutoMap(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                TypeLoader.LoadTypes(assembly);
                TypeLoader.LoadOperations(Container, assembly, this);
            }
        }

        public void Lock()
        {
            RegisterTypes(TypeLoader.TypeMappings.Values.Select(v => v.GraphType).ToArray());
        }
    }
}