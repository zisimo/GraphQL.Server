using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Objects
{
    public class HumanObject : GraphObject<Human>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICharacter[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
        public string HomePlanet { get; set; }

        public HumanObject(IContainer container) : base(container)
        {
            Interface<ICharacterInterface>();
        }

        public ICharacter[] GetFriends(ICharacter character)
        {
            return ICharacterInterface.GetFriends(Container, character);
        }
    }
}
