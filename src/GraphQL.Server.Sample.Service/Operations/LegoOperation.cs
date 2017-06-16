using GraphQL.Server.Exceptions;
using GraphQL.Server.Operation;
using GraphQL.Server.Sample.Interface.Lego;
using GraphQL.Server.Sample.Interface.Lego.Output;
using GraphQL.Server.Sample.Service.Repository;

namespace GraphQL.Server.Sample.Service.Operations
{
    public class LegoOperation : ILegoOperation, IOperation
    {
        public IContainer Container { get; set; }

        public LegoOperation(IContainer container)
        {
            Container = container;
        }

        public void Register(ApiSchema schema)
        {
            schema.Query.AddQuery<Lego, IdInput>(Lego);
        }

        public Lego Lego(IdInput input)
        {
            var lego = Container.GetInstance<Data>().GetLego(input.Id);
            if (lego == null) throw new NotFoundException<Lego>(input.Id);
            return lego;
        }
    }
}