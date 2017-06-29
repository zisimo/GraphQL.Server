using System;
using System.Collections.Generic;
using System.Net.Http;
using GraphQL.Client;
using GraphQL.Language.AST;
using GraphQL.Server.Sample.Interface.Lego;
using GraphQL.Server.Sample.Interface.Lego.Output;
using GraphQL.Server.Sample.Repository;

namespace GraphQL.Server.Sample.Maps
{
    public class TestMap : GraphObjectMap<Test>
    {
        private GraphClient<ILegoOperation> GraphClient { get; set; }

        public int Id { get; set; }
        public Lego Lego { get; set; }
        public Uri UriString { get; set; }
        public string ExtraParams { get; set; }

        public TestMap(IContainer container) : base(container)
        {
            GraphClient = new GraphClient<ILegoOperation>("http://localhost:51365/api", new HttpClient());
        }

        public Uri GetUriString(Test test)
        {
            return new Uri(test.UriString);
        }

        public string GetExtraParams(Test test, Guid id)
        {
            return $"Extra[{id}]";
        }

        public Lego GetLego(Test test, IEnumerable<Field> fields, int id)
        {
            return GraphClient.RunSelection<IdInput, Lego>(i => i.Lego, new IdInput { Id = id }, fields, QueryType.Query);
        }
    }
}
