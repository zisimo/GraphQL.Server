using System.Reflection;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ApiSchema : Schema
    {
        public ApiOperation Query { get; private set; }
        public ApiOperation Mutation { get; private set; }

        public ApiSchema(IContainer container, params Assembly[] assemblies) : base(type => (GraphType)container.GetInstance(type))
        {
            base.Query = Query = new ApiOperation(container, "Query");
            base.Mutation = Mutation = new ApiOperation(container, "Mutation");
            foreach (var assembly in assemblies)
            {
                TypeLoader.LoadTypes(assembly);
                TypeLoader.LoadOperations(container, assembly, this);
            }
            TypeLoader.InitializeTypes(container, this);
        }
    }
}