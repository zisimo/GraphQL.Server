namespace GraphQL.Server.Sample.Repository
{
    public class Droid : ICharacter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
        public string PrimaryFunction { get; set; }
    }
}
