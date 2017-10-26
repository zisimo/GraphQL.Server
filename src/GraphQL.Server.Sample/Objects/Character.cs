using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Objects
{
    public class Character : GraphInterface<ICharacter>
    {
        public int Id { get; set; }
        public ICharacter[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }

        public Character(IContainer container) : base(container)
        {
            AddType<HumanObject>();
            AddType<DroidObject>();
        }

        public static ICharacter[] GetFriends(IContainer container, ICharacter character)
        {
            return container.GetInstance<Data>().GetFriends(character);
        }
    }
}
