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

        public ApiSchema(IContainer container, params Assembly[] assemblies) : base(type => (GraphType)container.GetInstance(type))
        {
            Container = container;
            base.Query = Query = new ApiOperation(container, "Query");
            base.Mutation = Mutation = new ApiOperation(container, "Mutation");
            foreach (var assembly in assemblies)
            {
                TypeLoader.LoadTypes(assembly);
                TypeLoader.LoadOperations(container, assembly, this);
            }
            
        }

        public void MapOutput<TInput, TOutput>()
            where TInput : class
            where TOutput : class
        {
            var type = typeof(GraphObjectMap<,>).MakeGenericType(typeof(TInput), typeof(TOutput));
            TypeLoader.AddType(typeof(TInput), type);
        }

        public void Lock()
        {
            RegisterTypes(TypeLoader.TypeMappings.Values.Select(v => v.GraphType).ToArray());
        }
    }
}