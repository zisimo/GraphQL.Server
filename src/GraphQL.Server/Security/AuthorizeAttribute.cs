using System;

namespace GraphQL.Server.Security
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute
    {
        public string[] Claims { get; protected set; }

        public AuthorizeAttribute(params string[] claims)
        {
            Claims = claims;
        }
    }
}
