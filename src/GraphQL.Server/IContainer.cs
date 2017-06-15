using System;

namespace GraphQL.Server
{
    public interface IContainer
    {
        T GetInstance<T>() where T : class;
        object GetInstance(Type type);
        void Register(Type type);

        void Register<TConcrete>()
            where TConcrete : class;

        void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        bool HasRegistration(Type type);
    }
}
