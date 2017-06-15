using System;

namespace GraphQL.Client
{
    [Serializable]
    public class GraphClientException : Exception
    {
        public GraphClientException()
        {
        }

        public GraphClientException(string message) : base(message)
        {
        }

        public GraphClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
