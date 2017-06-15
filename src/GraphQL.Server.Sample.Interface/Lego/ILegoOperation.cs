using GraphQL.Server.Operation;

namespace GraphQL.Server.Sample.Interface.Lego
{
    public interface ILegoOperation : IOperation
    {
        Output.Lego Lego(IdInput input);
    }
}