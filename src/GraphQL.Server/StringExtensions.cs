using System;

namespace GraphQL.Server
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string text)
        {
            ValidateStringInput(text);

            return $"{char.ToLower(text[0])}{text.Substring(1)}";
        }

        public static string ToPascalCase(this string text)
        {
            ValidateStringInput(text);

            return $"{char.ToUpper(text[0])}{text.Substring(1)}";
        }

        private static void ValidateStringInput(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException(nameof(text));
            }
        }
    }
}
