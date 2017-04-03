using System.Collections;
using System.Linq;

namespace GraphQL.Server.Exceptions
{
    public class NotFoundException<T> : GraphException where T : class
    {
        public object[] Ids { get; private set; }
        
        public NotFoundException(object id) : base($"{typeof(T).Name} not found[{GetIdString(id)}]")
        {
            Ids = new[] { id };
        }

        private static string GetIdString(object id)
        {
            if (id is IEnumerable)
            {
                return string.Join(", ", (id as IEnumerable).OfType<object>().Select(i => $"{i}"));
            }
            return $"{id}";
        }
    }
}