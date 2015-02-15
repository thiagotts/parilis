using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SqlServer.Enums {
    public static class Enums {
        public static string GetDefaultValue(object value) {
            if (value == null) return string.Empty;
            var field = value.GetType().GetField(value.ToString());
            var attributes = (DefaultValueAttribute[]) field.GetCustomAttributes(typeof (DefaultValueAttribute), false);

            return attributes.Length > 0 ? attributes[0].Value.ToString() : value.ToString();
        }

        public static IEnumerable<string> GetDefaultValues<T>() {
            var fields = typeof(T).GetFields();
            IList<string> result = new List<string>();

            foreach (var field in fields) {
                var attributes = (DefaultValueAttribute[]) field.GetCustomAttributes(typeof (DefaultValueAttribute), false);
                if (attributes.Length > 0) result.Add(attributes[0].Value.ToString());
            }

            return result;
        }

        private static string GetDescription(object value) {
            if (value == null) return string.Empty;

            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;

            var attributes = (DescriptionAttribute[]) fieldInfo.GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static IEnumerable<string> GetDescriptions<T>() {
            IList<string> descriptions = new List<string>();
            foreach (T value in Enum.GetValues(typeof (T))) {
                descriptions.Add(GetDescription(value));
            }

            return descriptions.ToArray();
        }
    }
}