using GraphQL.Server.Operation;

namespace GraphQL.Server.Sample.Interface.Lego
{
    public interface ILegoOperation
    {
        Output.Lego Lego(IdInput input);
    }
}