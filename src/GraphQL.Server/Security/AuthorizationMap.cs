using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphQL.Server.Security
{
    public class AuthorizationMap
    {
        public bool AllowMissingAuthorizations { get; set; }
        public List<Authorization> Authorizations { get; set; }

        public AuthorizationMap()
        {
            Authorizations = new List<Authorization>();
        }

        public Authorization AddAuthorization(Type parentType, PropertyInfo propertyInfo)
        {
            var authFieldName = $"{parentType.FullName}.{propertyInfo.Name}";

            var typeAttribute = parentType.GetCustomAttributes(typeof(AuthorizeAttribute)).FirstOrDefault() as AuthorizeAttribute;
            if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic) return null;
            var propertyAttribute = propertyInfo.GetCustomAttributes(typeof(AuthorizeAttribute)).FirstOrDefault() as AuthorizeAttribute;
            if (typeAttribute == null && propertyAttribute == null)
            {
                if (AllowMissingAuthorizations) return null;
                throw new Exception($"Property and it's class does not have any Authorize attribute. Property: {authFieldName}");
            }
            var authorization = Authorization.Create(authFieldName, (propertyAttribute ?? typeAttribute).Claims);
            Authorizations.Add(authorization);
            return authorization;
        }

        public bool Authorize(string name, string[] permissions)
        {
            var authorization = Authorizations.FirstOrDefault(a => a.TargetName == name);
            var authorizationAllowed = authorization != null && authorization.Authorize(permissions);
            if (!authorizationAllowed && authorization == null && AllowMissingAuthorizations)
            {
                authorizationAllowed = true;
            }
            return authorizationAllowed;
        }
    }
}
