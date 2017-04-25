using System;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server.Types
{
    public class UriGraphType : ScalarGraphType
    {
        public UriGraphType()
        {
            Name = "Uri";
        }

        public override object Serialize(object value)
        {
            return ParseValue(value);
        }

        public override object ParseValue(object value)
        {
            Uri output;
            if (Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out output))
            {
                return output;
            }
            return null;
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