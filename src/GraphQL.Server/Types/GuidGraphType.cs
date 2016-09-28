using System;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server.Types
{
    public class GuidGraphType : ScalarGraphType
    {
        public GuidGraphType()
        {
            Name = "Guid";
        }

        public override object Serialize(object value)
        {
            return ParseValue(value);
        }

        public override object ParseValue(object value)
        {
            Guid output;
            if (Guid.TryParse(value.ToString().Trim('\"'), out output))
            {
                return output;
            }
            return Guid.Empty;
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is StringValue)
            {
                return ParseValue(((StringValue)value).Value);
            }
            return null;
        }
    }
}