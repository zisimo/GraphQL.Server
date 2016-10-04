using System;

namespace GraphQL.Server
{
    public interface IContainer
    {
        T GetInstance<T>() where T : class;
        object GetInstance(Type type);
    }
}
