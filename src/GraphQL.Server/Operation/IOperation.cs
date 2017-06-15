namespace GraphQL.Server.Operation
{
    public interface IOperation
    {
        void Register(ApiSchema schema);
    }
}
