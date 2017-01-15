namespace Lillian.Lib
{
    public static class Utils
    {
        public static string ToCamelCase(this string source)
        {
            return char.ToLower(source[0]) + source.Substring(1);
        }
    }
}
