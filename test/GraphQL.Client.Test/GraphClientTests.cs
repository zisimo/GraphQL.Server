using System.Linq;
using System.Net.Http;
using NUnit.Framework;

namespace GraphQL.Client.Test
{
    [TestFixture]
    public class GraphClientTests
    {
        private const string ApiUrl = "http://testapi.com/api/";
        public GraphClient Client { get; set; }
        public TestMessageHandler TestMessageHandler { get; set; }
        public HttpClient HttpClient { get; set; }



        [SetUp]
        public void SetUp()
        {
            TestMessageHandler = new TestMessageHandler();
            HttpClient = new HttpClient(TestMessageHandler);
            Client = new GraphClient(ApiUrl, HttpClient);
        }

        [Test]
        public void RunQueries()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""course"": {{""id"":1, ""name"":""Test course""}},
        ""user"": {{""id"":1, ""firstname"":""Bill""}}
    }},
    ""errors"":[
    ]
}}";
            TestMessageHandler.SetContent("course", returnContent);
            var queryCourse = Client.AddQuery("course(id:1)", new { id = 1, name = "" });
            var queryUser = Client.AddQuery("user(id:1)", new { id = 1, firstname = "" });
            var output = Client.RunQueriesAsync("q1").Result;
            Assert.AreEqual(@"{""query"":""query q1{course(id:1){id name} user(id:1){id firstname}}"",""variables"":null}", TestMessageHandler.LastRequestContent);
            Assert.IsEmpty(output.Errors);
            Assert.AreEqual(1, queryCourse.Data.id);
            Assert.AreEqual("Test course", queryCourse.Data.name);
            Assert.AreEqual(1, queryUser.Data.id);
            Assert.AreEqual("Bill", queryUser.Data.firstname);
        }

        [Test]
        public void RunQuery()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""course"": {{""id"":1, ""name"":""Test course""}}
    }},
    ""errors"":[
    ]
}}";
            TestMessageHandler.SetContent("course", returnContent);
            var output = Client.RunQueryAsync("course(id:1)", new { id = 1, name = "" }, "q1").Result;
            Assert.AreEqual(@"{""query"":""query q1{course(id:1){id name}}"",""variables"":null}", TestMessageHandler.LastRequestContent);
            Assert.IsEmpty(output.Errors);
            Assert.AreEqual(1, output.Data.id);
            Assert.AreEqual("Test course", output.Data.name);
        }


      
        [Test]
        public void RunQueryArray()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""searchCourses"": [{{""id"":1, ""name"":""Test course""}}]
    }},
    ""errors"":[
    ]
}}";
            TestMessageHandler.SetContent("searchCourses", returnContent);
            var outputType = new { Id = 1, Name = "" };
            var output = Client.RunQueryAsync("searchCourses()", new[] { outputType }.ToList(), "q1").Result;
            Assert.IsEmpty(output.Errors);
            Assert.AreEqual(1, output.Data.FirstOrDefault().Id);
            Assert.AreEqual("Test course", output.Data.FirstOrDefault().Name);

            Assert.AreEqual(@"{""query"":""query q1{searchCourses(){id name}}"",""variables"":null}", TestMessageHandler.LastRequestContent);
        }

        [Test]
        public void RunMutations()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""course"": {{""id"":1, ""name"":""Test course""}},
        ""user"": {{""id"":1, ""firstname"":""Bill""}}
    }},
    ""errors"":[
    ]
}}";
            TestMessageHandler.SetContent("course", returnContent);
            var queryCourse = Client.AddQuery("course(id:1)", new { id = 1, name = "" });
            var queryUser = Client.AddQuery("user(id:1)", new { id = 1, firstname = "" });
            var output = Client.RunMutationsAsync("m1").Result;
            Assert.AreEqual(@"{""query"":""mutation m1{course(id:1){id name} user(id:1){id firstname}}"",""variables"":null}", TestMessageHandler.LastRequestContent);
            Assert.IsEmpty(output.Errors);
            Assert.AreEqual(1, queryCourse.Data.id);
            Assert.AreEqual("Test course", queryCourse.Data.name);
            Assert.AreEqual(1, queryUser.Data.id);
            Assert.AreEqual("Bill", queryUser.Data.firstname);
        }

        [Test]
        public void RunMutation()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""course"": {{""id"":1, ""name"":""Test course""}}
    }},
    ""errors"":[
    ]
}}";
            TestMessageHandler.SetContent("course", returnContent);
            var output = Client.RunMutationAsync("course(id:1)", new { id = 1, name = "" }, "m1").Result;
            Assert.AreEqual(@"{""query"":""mutation m1{course(id:1){id name}}"",""variables"":null}", TestMessageHandler.LastRequestContent);
            Assert.IsEmpty(output.Errors);
            Assert.AreEqual(1, output.Data.id);
            Assert.AreEqual("Test course", output.Data.name);
        }

        [Test]
        public void PopulateErrors()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""course"": null
    }},
    ""errors"":[
        {{
            ""detail"": ""Error detail"",
            ""message"": ""Error message"",
            ""stackTrace"": ""line 173""
        }}
    ]
}}";
            TestMessageHandler.SetContent("course", returnContent);
            var output = Client.RunMutationAsync("course(id:1)", new { id = 1, name = "" }, "m1").Result;
            Assert.AreEqual(@"{""query"":""mutation m1{course(id:1){id name}}"",""variables"":null}", TestMessageHandler.LastRequestContent);
            Assert.AreEqual(1, output.Errors.Length);
            Assert.AreEqual("Error detail", output.Errors[0].Detail);
            Assert.AreEqual("Error message", output.Errors[0].Message);
            Assert.AreEqual("line 173", output.Errors[0].StackTrace);
        }

        [Test]
        public void UseVariables()
        {
            var returnContent = $@"{{
    ""data"": {{
        ""course"": {{""id"":1, ""name"":""Test course""}}
    }},
    ""errors"":[
    ]
}}";
            TestMessageHandler.SetContent("course", returnContent);
            var output = Client.RunMutationAsync("course(id:1)", new { id = 1, name = "" }, "m1", new { id = 5 }).Result;
            Assert.AreEqual(@"{""query"":""mutation m1{course(id:1){id name}}"",""variables"":""{\""id\"":5}""}", TestMessageHandler.LastRequestContent);
            Assert.IsEmpty(output.Errors);
        }
    }
}
