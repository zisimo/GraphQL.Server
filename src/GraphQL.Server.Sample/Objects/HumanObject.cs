using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Objects
{
    public class HumanObject : GraphObject<Human>
    {
        public int Id { get; set; }
        public ICharacter[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
        public string HomePlanet { get; set; }

        public HumanObject(IContainer container) : base(container)
        {
            Interface<Character>();
        }

        public ICharacter[] GetFriends(ICharacter character)
        {
            return Character.GetFriends(Container, character);
        }
    }
}
