using System.Linq;
using System.Reflection;
using GraphQL.Server.Sample.Operations;
using GraphQL.Server.Sample.Repository;
using GraphQL.Server.Security;
using GraphQL.Server.SimpleInjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace GraphQL.Server.Test
{
    public class TestBase
    {
        private JsonSerializerSettings SerializerSettings { get; set; }
        private DocumentExecuter Executer { get; set; }
        private GraphQLContainer Container { get; set; }
        private AuthorizationMap AuthorizationMap { get; set; }
        private ApiSchema Schema { get; set; }
        public Data Data { get; set; }

        [SetUp]
        public void SetUp()
        {
            SerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            SerializerSettings.Converters.Add(new StringEnumConverter());
            Container = new GraphQLContainer();
            Data = new Data();
            AuthorizationMap = new AuthorizationMap()
            {
                AllowMissingAuthorizations = true
            };

            Container.Register<IContainer>(() => Container);
            Container.Register(() => Data);
            Container.Register(() => AuthorizationMap);
            Container.Register<AllOperations>();
            Container.Verify();

            Executer = new DocumentExecuter();
            Schema = new ApiSchema(Container, Assembly.GetExecutingAssembly(), typeof(AllOperations).Assembly);
        }

        protected void AssertQuery(string query, Inputs inputs, string expectedJson)
        {
            var actualJson = RunOperation(query, inputs);
            var expectedObject = JsonConvert.DeserializeObject(expectedJson);
            expectedJson = JsonConvert.SerializeObject(expectedObject, SerializerSettings);
            StringAssert.Contains(expectedJson, actualJson);
        }

        private string RunOperation(string query, Inputs inputs)
        {
            var executionResult = Executer.ExecuteAsync(Schema, null, query, null, inputs: inputs).Result;
            var output = new GraphQLOutput(executionResult.Data, executionResult.Errors?.ToArray());
            return JsonConvert.SerializeObject(output, SerializerSettings);
        }
    }
}
