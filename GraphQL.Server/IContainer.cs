using System;

namespace GraphQL.Server
{
    public interface IContainer
    {
        T GetInstance<T>();
        object GetInstance(Type type);
    }
}
