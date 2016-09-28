namespace GraphQL.Server
{
    public interface IOperation
    {
        void Register(ApiSchema schema);
    }
}
