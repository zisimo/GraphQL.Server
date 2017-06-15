using GraphQL.Server.Operation;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Operations
{
    public interface ILegoServiceOperation : IOperation
    {
        Lego GetLegoFromProxy(IdInput input);
    }
}