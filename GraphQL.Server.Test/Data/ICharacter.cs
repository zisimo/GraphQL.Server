namespace GraphQL.Server.Test.Data
{
    public interface ICharacter
    {
        int Id { get; set; }
        string Name { get; set; }
        int[] Friends { get; set; }
        Episodes[] AppearsIn { get; set; }
    }
}
