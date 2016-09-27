using NUnit.Framework;

namespace GraphQL.Server.Test
{
    public class OperationTests : TestBase
    {

        [Test]
        public void TestGetHuman()
        {
            var query = @"query{getHuman(id: 1){id}}";
            var expected = @"{
    ""data"": {
        ""getHuman"": {
            ""id"": 1
        }
    }
}";
            AssertQuery(query, null, expected);
        }
    }
}
