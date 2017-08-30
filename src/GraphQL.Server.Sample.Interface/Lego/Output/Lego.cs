namespace GraphQL.Server.Sample.Interface.Lego.Output
{
    public class Lego
    {
        public int Id { get; set; }
        public string Color { get; set; }

        public static string StaticColor => "Yellow";
    }
}