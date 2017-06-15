using GraphQL.Server.Operation;
using GraphQL.Server.Sample.Output;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Operations
{
    public interface IRobotOperation
    {
        RobotOutput GetRobot(IdInput input);
    }

    public class RobotOperation : IOperation, IRobotOperation
    {
        private IContainer Container { get; set; }

        public RobotOperation(IContainer container)
        {
            Container = container;
        }

        public void Register(ApiSchema schema)
        {
            schema.Query.AddQuery<RobotOutput, IdInput>(GetRobot);
        }

        public RobotOutput GetRobot(IdInput input)
        {
            var robot = Container.GetInstance<Data>().GetRobot(input.Id);
            return Map.Extend<RobotOutput>(robot);
        }
    }
}