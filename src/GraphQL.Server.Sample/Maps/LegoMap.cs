using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Maps
{
    public class LegoMap : GraphObjectMap<Lego>
    {
        public LegoMap(IContainer container) : base(container)
        {
        }

        public string GetColor(Lego lego)
        {
            return $"__{lego.Color}__";
        }
    }
}