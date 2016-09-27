using NUnit.Framework;

namespace GraphQL.Server.Test
{
    public class OperationTests : TestBase
    {

        [Test]
        public void TestIdInputQueryForObjectReturn()
        {
            var query = @"query{
    getDroid(id: 3){id}
    getHuman(id: 1){id}
}";
            var expected = @"{
    ""data"": {
        ""getDroid"": {
            ""id"": 3
        },
        ""getHuman"": {
            ""id"": 1
        }
    }
}";
            AssertQuery(query, null, expected);
        }

        [Test]
        public void TestIdInputQueryForInterfaceReturn()
        {
            var query = @"query{getHero(id: 3){id}}";
            var expected = @"{
    ""data"": {
        ""getHero"": {
            ""id"": 3
        }
    }
}";
            AssertQuery(query, null, expected);
        }

        [Test]
        public void TestCustomInputQuery()
        {
            var query = @"query{searchHeroes(text:""r""){id}}";
            var expected = @"{
    ""data"": {
        ""searchHeroes"": [
            {
                ""id"": 2
            },
            {
                ""id"": 3
            }
        ]
    }
}";
            AssertQuery(query, null, expected);
        }
    }
}
