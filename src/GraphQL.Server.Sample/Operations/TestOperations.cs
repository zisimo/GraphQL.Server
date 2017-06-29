using GraphQL.Server.Exceptions;
using GraphQL.Server.Operation;
using GraphQL.Server.Sample.Maps;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Operations
{
    public class TestOperations : IOperation
    {
        public IContainer Container { get; set; }

        public TestOperations(IContainer container)
        {
            Container = container;
        }
        public void Register(ApiSchema schema)
        {
            // Queries
            schema.Query.AddQuery<TestMap, IdInput>(GetTest);

            //Mutations
        }

        // ==================== Queries ====================
        private Test GetTest(IdInput input, InputField[] fields)
        {
            var test = Container.GetInstance<Data>().GetTest(input.Id);
            if (test == null) throw new NotFoundException<Human>(input.Id);
            return test;
        }

        // ==================== Mutations ====================

        // ==================== Inputs ====================
    }
}
