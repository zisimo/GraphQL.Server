using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server.Types
{
    public class DoubleGraphType : ScalarGraphType
    {
        public DoubleGraphType()
        {
            Name = "Double";
        }

        public override object Serialize(object value)
        {
            return ParseValue(value);
        }

        public override object ParseValue(object value)
        {
            double result;
            if (double.TryParse(value?.ToString() ?? string.Empty, out result))
            {
                return result;
            }
            return null;
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is IntValue)
            {
                return ((IntValue)value).Value;
            }
            if (value is LongValue)
            {
                return ((LongValue)value).Value;
            }
            if (value is FloatValue)
            {
                return ((FloatValue)value).Value;
            }
            return null;
        }
    }
}