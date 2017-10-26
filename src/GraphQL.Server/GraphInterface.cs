using System;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class GraphInterface<T> : InterfaceGraphType where T : class
    {
        public IContainer Container { get; set; }

        public GraphInterface(IContainer container)
        {
            Container = container;
            Name = typeof(T).Name;
            if (container != null) FieldMapper.AddAllFields(Container, this, GetType(), true);
            ResolveType = o =>
            {
                if (o is T) return (ObjectGraphType)Activator.CreateInstance(TypeLoader.GetGraphType(o.GetType()), Container);
                return null;
            };
        }

        protected void AddType<TType>() where TType : ObjectGraphType
        {
            var obj = (ObjectGraphType)Activator.CreateInstance(typeof(TType), Container);
            AddPossibleType(obj);
        }
    }
}