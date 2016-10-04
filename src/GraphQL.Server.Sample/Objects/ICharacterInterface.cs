using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Objects
{
    public class ICharacterInterface : GraphInterface<ICharacter>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICharacter[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }

        public ICharacterInterface(IContainer container) : base(container)
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
