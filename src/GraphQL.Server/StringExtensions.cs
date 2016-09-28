namespace GraphQL.Server
{
    public static class StringExtensions
    {
        public static string PascalCase(string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }

        public static string CamelCase(string text)
        {
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}