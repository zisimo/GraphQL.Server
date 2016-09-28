using System;

namespace GraphQL.Server.Exceptions
{
    [Serializable]
    public class GraphException : ApplicationException
    {
        public GraphException()
            : this("An error occurred")
        {
        }

        public GraphException(string message)
            : this(message, null)
        {
        }

        public GraphException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public bool IsFriendly { get; set; }
    }
}