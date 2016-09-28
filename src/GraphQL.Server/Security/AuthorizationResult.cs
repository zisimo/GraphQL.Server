using System.Collections.Generic;

namespace GraphQL.Server.Security
{
    public class AuthorizationResult
    {
        public List<string> Failures { get; set; }
        public bool Failed => Failures.Count > 0;

        public AuthorizationResult()
        {
            Failures = new List<string>();
        }

        public void AddFailure(string failure)
        {
            Failures.Add(failure);
        }
    }
}
