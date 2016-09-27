using GraphQL.Server.Exceptions;
using GraphQL.Server.Test.Data;
using GraphQL.Server.Test.Objects;

namespace GraphQL.Server.Test.Operations
{
    public class AllOperations : IOperation
    {
        public IContainer Container { get; set; }

        public AllOperations(IContainer container)
        {
            Container = container;
        }

        public void Register(ApiSchema schema)
        {
            schema.Query.AddQuery<HumanObject, IdInput>(GetHuman);
        }

        private Human GetHuman(IdInput input, InputField[] fields)
        {
            var human = Container.GetInstance<Data.Data>().GetHuman(input.Id);
            if (human == null) throw new NotFoundException<Human>(input.Id);
            return human;
        }
    }
}
