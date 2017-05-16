using System;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Objects
{
    public class TestObject : GraphObject<Test>
    {
        public int Id { get; set; }
        public Uri UriString { get; set; }
        public string ExtraParams { get; set; }

        public TestObject(IContainer container) : base(container)
        {
        }

        public Uri GetUriString(Test test)
        {
            return new Uri(test.UriString);
        }

        public string GetExtraParams(Test test, Guid id)
        {
            return $"Extra[{id}]";
        }
    }
}
