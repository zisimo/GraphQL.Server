using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Objects
{
    public class DroidObject : GraphObject<Droid>
    {
        public int Id { get; set; }
        public ICharacter[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
        public string PrimaryFunction { get; set; }

        public DroidObject(IContainer container) : base(container)
        {
            Interface<Character>();
        }

        public ICharacter[] GetFriends(ICharacter character)
        {
            return Character.GetFriends(Container, character);
        }
    }
}
