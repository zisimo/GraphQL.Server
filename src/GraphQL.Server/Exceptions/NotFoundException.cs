namespace GraphQL.Server.Exceptions
{
    public class NotFoundException<T> : GraphException where T : class
    {
        public T[] Entities { get; private set; }
        public object[] Ids { get; private set; }

        public NotFoundException(object[] ids) : base($"{typeof(T).Name}s not found[{string.Join(", ", ids)}]")
        {
            Ids = ids;
        }
        public NotFoundException(object id) : base($"{typeof(T).Name} not found[{id}]")
        {
            Ids = new[] { id };
        }
    }
}