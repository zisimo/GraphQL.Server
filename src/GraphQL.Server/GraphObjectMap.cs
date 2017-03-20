using GraphQL.Types;

namespace GraphQL.Server
{
    public class GraphObjectMap<TInput, TOutput> : ObjectGraphType
        where TInput : class
        where TOutput : class
    {
        public IContainer Container { get; set; }

        public GraphObjectMap(IContainer container)
        {
            Container = container;
            Name = typeof(TInput).Name;
            if (container != null) FieldMapper.AddFields<TOutput>(container, this);
            IsTypeOf = o => o is TInput;
        }
    }

    public class GraphObjectMap<TOutput> : ObjectGraphType
        where TOutput : class
    {
        public IContainer Container { get; set; }

        public GraphObjectMap(IContainer container)
        {
            Container = container;
            Name = typeof(TOutput).Name;
            if (container != null) FieldMapper.AddFields<TOutput>(container, this);
            IsTypeOf = o => o is TOutput;
        }
    }
}
