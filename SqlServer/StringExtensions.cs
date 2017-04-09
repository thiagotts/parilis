using System.Text.RegularExpressions;

namespace SqlServer {
    public static class StringExtensions {
        public static string DoubleQuoted(this string text) {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return Regex.Replace(text, @"'+", "''");           
        }
    }
}