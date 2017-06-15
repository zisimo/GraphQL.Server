using System;
using SimpleInjector;

namespace GraphQL.Server.SimpleInjector
{
    public class GraphQLContainer : Container, IContainer
    {
        public bool HasRegistration(Type type)
        {
            return base.GetRegistration(type) != null;
        }
    }
}
