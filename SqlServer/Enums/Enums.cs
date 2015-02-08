using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlServer.Enums {
    public static class Enums {
        public static string GetDefaultValue(object value) {
            if (value == null) return string.Empty;
            var field = value.GetType().GetField(value.ToString());
            var attributes = (DefaultValueAttribute[]) field.GetCustomAttributes(typeof (DefaultValueAttribute), false);

            return attributes.Length > 0 ? attributes[0].Value.ToString() : value.ToString();
        }

        public static IList<string> GetDefaultValues(Type type) {
            var fields = type.GetFields();
            IList<string> result = new List<string>();

            foreach (var field in fields) {
                var attributes = (DefaultValueAttribute[]) field.GetCustomAttributes(typeof (DefaultValueAttribute), false);
                if (attributes.Length > 0) result.Add(attributes[0].Value.ToString());
            }

            return result;
        }
    }
}