using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Server.Sample.Repository
{
    public class Data
    {
        private readonly List<Human> _humans = new List<Human>();
        private readonly List<Droid> _droids = new List<Droid>();
        private readonly List<Robot> _robots = new List<Robot>();
        private readonly List<Test> _tests = new List<Test>();

        public Data()
        {
            _humans.Add(new Human
            {
                Id = 1,
                Name = "Luke",
                Friends = new[] { 3, 4 },
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.JEDI },
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
            _robots.Add(new Robot
            {
                Id = 1,
                Name = "Gloo"
            });
            _robots.Add(new Robot
            {
                Id = 2,
                Name = "Mary"
            });
            _tests.Add(new Test
            {
                Id = 1,
                UriString = "http://www.test.com/image.jpg"
            });
        }

        public ICharacter[] GetFriends(ICharacter character)
        {
            var friends = new List<ICharacter>();
            friends.AddRange(_humans.Where(h => character.Friends != null && character.Friends.Contains(h.Id)));
            friends.AddRange(_droids.Where(d => character.Friends != null && character.Friends.Contains(d.Id)));
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

        public ICharacter GetHero(int? id)
        {
            ICharacter hero = _humans.FirstOrDefault(h => h.Id == id);
            if (hero == null) hero = _droids.FirstOrDefault(d => d.Id == id);
            return hero;
        }

        public ICharacter[] SearchHeroes(string text)
        {
            text = text.ToLower();
            var heroes = new List<ICharacter>();
            heroes.AddRange(_humans.Where(h => h.Name.ToLower().Contains(text)));
            heroes.AddRange(_droids.Where(d => d.Name.ToLower().Contains(text)));
            return heroes.OrderBy(h => h.Id).ToArray();
        }

        public Human CreateHuman(Human human)
        {
            human.Id = Math.Max(_humans.Max(h => h.Id), _droids.Max(h => h.Id)) + 1;
            _humans.Add(human);
            return human;
        }

        public Robot GetRobot(int? inputId)
        {
            return _robots.FirstOrDefault(r => r.Id == inputId);
        }

        public Test GetTest(int? id)
        {
            return _tests.FirstOrDefault(r => r.Id == id);
        }
    }
}
