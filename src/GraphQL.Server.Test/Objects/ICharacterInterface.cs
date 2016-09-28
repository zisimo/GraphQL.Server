using GraphQL.Server.Test.Data;

namespace GraphQL.Server.Test.Objects
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

        public ICharacter[] GetFriends(Human human)
        {
            return Container.GetInstance<Data.Data>().GetFriends(human);
        }
    }
}
