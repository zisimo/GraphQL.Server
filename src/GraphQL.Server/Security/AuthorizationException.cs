using System;

namespace GraphQL.Server.Security
{
    public class AuthorizationException : Exception
    {
        public string Field { get; set; }

        public AuthorizationException(string field) : base($"Authorization failed for field: {field}")
        {
            Field = field;
        }
    }
}
