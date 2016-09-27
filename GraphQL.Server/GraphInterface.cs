using GraphQL.Types;

namespace GraphQL.Server
{
    public class GraphInterface<T> : InterfaceGraphType where T : class
    {
        public IContainer Container { get; set; }
        public GraphObject<T> Object { get; set; }

        public GraphInterface(IContainer container)
        {
            Container = container;
            Name = typeof(T).Name;
            Object = new GraphObject<T>(container);
        }
    }
}