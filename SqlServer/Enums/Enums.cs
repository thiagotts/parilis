using System.ComponentModel;

namespace SqlServer.Enums {
    public static class Enums {
        public static string GetDefaultValue(object value) {
            if (value == null) return string.Empty;
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (DefaultValueAttribute[]) fi.GetCustomAttributes(typeof (DefaultValueAttribute), false);

            return attributes.Length > 0 ? attributes[0].Value.ToString() : value.ToString();
        }
    }
}