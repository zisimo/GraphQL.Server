using GraphQL.Server.Exceptions;
using System.ComponentModel.DataAnnotations;
using GraphQL.Server.Operation;
using GraphQL.Server.Sample.Objects;
using GraphQL.Server.Sample.Output;
using GraphQL.Server.Sample.Repository;
using GraphQL.Server.Types;
using GraphQL.Types;

namespace GraphQL.Server.Sample.Operations
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
            // Queries
            schema.Query.AddQuery<HumanObject, IdInput>(GetHuman);
            schema.Query.AddQuery<DroidObject, IdInput>(GetDroid);
            schema.Query.AddQuery<Character, IdInput>(GetHero);
            schema.Query.AddQuery<ListGraphType<Character>, SearchHeroesInput>(SearchHeroes);

            //Mutations
            schema.Mutation.AddQuery<HumanObject, CreateHumanInput>(CreateHuman);
        }

        // ==================== Queries ====================
        private Human GetHuman(IdInput input, InputField[] fields)
        {
            var human = Container.GetInstance<Data>().GetHuman(input.Id);
            if (human == null) throw new NotFoundException<Human>(input.Id);
            return human;
        }
        private Droid GetDroid(IdInput input, InputField[] fields)
        {
            var droid = Container.GetInstance<Data>().GetDroid(input.Id);
            if (droid == null) throw new NotFoundException<Droid>(input.Id);
            return droid;
        }
        private ICharacter GetHero(IdInput input, InputField[] fields)
        {
            var hero = Container.GetInstance<Data>().GetHero(input.Id);
            if (hero == null) throw new NotFoundException<ICharacter>(input.Id);
            return hero;
        }
        private ICharacter[] SearchHeroes(SearchHeroesInput input, InputField[] fields)
        {
            return Container.GetInstance<Data>().SearchHeroes(input.Text ?? string.Empty);
        }
        private Robot GetRobot(IdInput input, InputField[] fields)
        {
            var robot = Container.GetInstance<Data>().GetRobot(input.Id);
            if (robot == null) throw new NotFoundException<Robot>(input.Id);
            return robot;
        }

        // ==================== Mutations ====================
        private Human CreateHuman(CreateHumanInput input, InputField[] fields)
        {
            var human = Map.Extend<Human>(input, fields);
            return Container.GetInstance<Data>().CreateHuman(human);
        }

        // ==================== Inputs ====================
        public class SearchHeroesInput
        {
            public string Text { get; set; }
            public Order Order { get; set; }
        }
        public class Order
        {
            public string OrderBy { get; set; }
            public OrderByDirection? Direction { get; set; }
        }
        public enum OrderByDirection
        {
            Ascending,
            Descending
        }
        public class OrderByDirectionEnum : GraphEnum<OrderByDirection>
        {

        }
        public class CreateHumanInput
        {
            [Required]
            public string Name { get; set; }
            public int[] Friends { get; set; }
            public Episodes[] AppearsIn { get; set; }
            public string HomePlanet { get; set; }

            public Address[] Address { get; set; }
        }
        public class Address
        {
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public string PostCode { get; set; }
            public string Country { get; set; }
        }
    }
}
