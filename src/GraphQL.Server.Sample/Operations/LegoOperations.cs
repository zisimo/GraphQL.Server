using GraphQL.Server.Exceptions;
using GraphQL.Server.Operation;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Operations
{
    public class LegoOperations : IOperation
    {
        public IContainer Container { get; set; }

        public LegoOperations(IContainer container)
        {
            Container = container;
        }
        public void Register(ApiSchema schema)
        {
            // Queries
            schema.Query.AddQuery<Lego, IdInput>(GetLego);

            //Mutations
        }

        // ==================== Queries ====================
        private Lego GetLego(IdInput input)
        {
            var lego = Container.GetInstance<Data>().GetLego(input.Id);
            if (lego == null) throw new NotFoundException<Lego>(input.Id);
            return lego;
        }

        // ==================== Mutations ====================

        // ==================== Inputs ====================
    }
}
