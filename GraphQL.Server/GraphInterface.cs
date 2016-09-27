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
            ResolveType = o => (ObjectGraphType)Activator.CreateInstance(TypeLoader.GetGraphType(o.GetType()), Container);
        }

        protected void AddType<T>() where T : ObjectGraphType
        {
            var obj = (ObjectGraphType)Activator.CreateInstance(typeof(T), Container);
            AddPossibleType(obj);
        }
    }
}