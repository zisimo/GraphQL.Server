using System;
using System.Linq;

namespace GraphQL.Server.Exceptions
{
    public class NotFoundException<T> : GraphException where T : class
    {
        public T[] Entities { get; private set; }
        public int?[] Ids { get; private set; }

        public NotFoundException(int?[] ids) : base($"{typeof(T).Name}s not found[{string.Join(", ", ids)}]")
        {
            Ids = ids;
        }
        public NotFoundException(int[] ids) : base($"{typeof(T).Name}s not found[{string.Join(", ", ids)}]")
        {
            Ids = ids.OfType<int?>().ToArray();
        }
        public NotFoundException(int? id) : base($"{typeof(T).Name} not found[{id}]")
        {
            Ids = new[] { id };
        }
    }
}