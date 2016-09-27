using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Server.Test.Data
{
    public class Data
    {
        private readonly List<Human> _humans = new List<Human>();
        private readonly List<Droid> _droids = new List<Droid>();

        public Data()
        {
            _humans.Add(new Human
            {
                Id = 1,
                Name = "Luke",
                Friends = new[] { 3, 4 },
                AppearsIn = new[] {  Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.JEDI },
                HomePlanet = "Tatooine"
            });
            _humans.Add(new Human
            {
                Id = 2,
                Name = "Vader",
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.JEDI },
                HomePlanet = "Tatooine"
            });

            _droids.Add(new Droid
            {
                Id = 3,
                Name = "R2-D2",
                Friends = new[] { 1, 4 },
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.JEDI },
                PrimaryFunction = "Astromech"
            });
            _droids.Add(new Droid
            {
                Id = 4,
                Name = "C-3PO",
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.JEDI },
                PrimaryFunction = "Protocol"
            });
        }

        public ICharacter[] GetFriends(ICharacter character)
        {
            var friends = new List<ICharacter>();
            friends.AddRange(_humans.Where(h => character.Friends.Contains(h.Id)));
            friends.AddRange(_droids.Where(d => character.Friends.Contains(d.Id)));
            return friends.ToArray();
        }

        public Human GetHuman(int? id)
        {
            return _humans.FirstOrDefault(h => h.Id == id);
        }

        public Droid GetDroid(int? id)
        {
            return _droids.FirstOrDefault(d => d.Id == id);
        }
    }
}
