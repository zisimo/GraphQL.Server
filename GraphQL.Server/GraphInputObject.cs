using GraphQL.Types;

namespace GraphQL.Server
{
    public class GraphInputObject<T> : InputObjectGraphType where T : class
    {
        public IContainer Container { get; set; }

        public GraphInputObject(IContainer container)
        {
            Container = container;
            Name = typeof(T).Name;
            FieldMapper.AddAllFields(Container, this, typeof(T), false);
        }
    }
}
