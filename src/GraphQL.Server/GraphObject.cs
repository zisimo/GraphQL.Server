using GraphQL.Types;

namespace GraphQL.Server
{
    public class GraphObject<T> : ObjectGraphType where T : class
    {
        public IContainer Container { get; set; }

        public GraphObject(IContainer container)
        {
            Container = container;
            Name = typeof(T).Name;
            if (container != null) FieldMapper.AddAllFields(Container, this, GetType(), true);
            IsTypeOf = o => o is T;
        }
    }
}
