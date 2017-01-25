using System.Linq;

namespace GraphQL.Server.Security
{
    public class Authorization
    {
        public string[] Roles { get; set; }
        public string TargetName { get; set; }

        public static Authorization Create(string name, params string[] roles)
        {
            return new Authorization()
            {
                Roles = roles,
                TargetName = name
            };
        }

        public bool Authorize(string[] roles)
        {
            return Roles.Intersect(roles).Any();
        }
    }
}
