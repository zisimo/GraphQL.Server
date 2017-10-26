using System.Collections.Generic;
using System.Linq;
using GraphQL.Server.Sample.Interface.Lego.Output;

namespace GraphQL.Server.Sample.Service.Repository
{
    public class Data
    {
        private readonly List<Lego> _legos = new List<Lego>();

        public Data()
        {
            _legos.Add(new Lego
            {
                Color = "Blue",
                Id = 1,
            });
            _legos.Add(new Lego
            {
                Color = "Red",
                Id = 2
            });
        }

        public Lego GetLego(int? id)
        {
            return _legos.FirstOrDefault(l => l.Id == id);
        }
    }
}
