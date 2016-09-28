using System;

namespace GraphQL.Server.Exceptions
{
    public class ValidationException : GraphException
    {
        public ValidationException()
        {
        }
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
