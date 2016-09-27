using GraphQL.Server.Test.Data;

namespace GraphQL.Server.Test.Objects
{
    public class DroidObject : GraphObject<Droid>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICharacter[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
        public string PrimaryFunction { get; set; }

        public DroidObject(IContainer container) : base(container)
        {
        }
    }
}
