using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphQL.Server
{
    public class InputField
    {
        public string Name { get; set; }
        public InputField[] Fields { get; set; }

        public static InputField GetField<T>(InputField[] fields, Expression<Func<T, object>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            var fieldName = (memberExpression.Member as PropertyInfo).Name.ToCamelCase();
            return fields.FirstOrDefault(f => f.Name == fieldName);
        }

        public bool NameEquals(string value)
        {
            return Name.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}