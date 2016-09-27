using GraphQL.Server.Exceptions;
using GraphQL.Server.Test.Data;
using GraphQL.Server.Test.Objects;
using System.ComponentModel.DataAnnotations;
using GraphQL.Types;

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
            schema.Query.AddQuery<DroidObject, IdInput>(GetDroid);
            schema.Query.AddQuery<ICharacterInterface, IdInput>(GetHero);
            schema.Query.AddQuery<ListGraphType<ICharacterInterface>, SearchHeroesInput>(SearchHeroes);
        }

        private Human GetHuman(IdInput input, InputField[] fields)
        {
            var human = Container.GetInstance<Data.Data>().GetHuman(input.Id);
            if (human == null) throw new NotFoundException<Human>(input.Id);
            return human;
        }

        private Droid GetDroid(IdInput input, InputField[] fields)
        {
            var droid = Container.GetInstance<Data.Data>().GetDroid(input.Id);
            if (droid == null) throw new NotFoundException<Droid>(input.Id);
            return droid;
        }

        private ICharacter GetHero(IdInput input, InputField[] fields)
        {
            var hero = Container.GetInstance<Data.Data>().GetHero(input.Id);
            if (hero == null) throw new NotFoundException<ICharacter>(input.Id);
            return hero;
        }
        private ICharacter[] SearchHeroes(SearchHeroesInput input, InputField[] fields)
        {
            return Container.GetInstance<Data.Data>().SearchHeroes(input.Text);
        }

        public class SearchHeroesInput
        {
            [Required]
            public string Text { get; set; }
        }
    }
}
