using System.Collections.Generic;

namespace GraphQL.Server.Sample.Repository
{
    public class Human : ICharacter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
        public string HomePlanet { get; set; }

        public Address Address { get; set; }
    }
}
