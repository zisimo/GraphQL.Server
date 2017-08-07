using GraphQL.Server.Sample.Interface.Lego.Output;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Maps
{
    public class LegoMap : GraphObjectMap<Lego>
    {
        public LegoMap(IContainer container) : base(container)
        {
        }

        public string GetColor(ResolverInfo info)
        {
            var test = info.GetParent<Test>();
            var source = info.GetSource<Lego>();
            return test != null ? $"TestId:{test.Id}, color:{source.Color}" : source.Color;
        }
    }
}