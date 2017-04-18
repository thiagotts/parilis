using System;

namespace Core.Descriptions {
    public class DefaultDescription : ConstraintDescription {
        public ColumnDescription Column { get; set; }
        public string DefaultValue { get; set; }

        public override string FullName => string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";

        public override bool Equals(object other) {
            if (!(other is DefaultDescription)) return false;
            var defaultDescription = (DefaultDescription) other;
            return base.Equals(other) &&
                   Column != null &&
                   Column.Equals(defaultDescription.Column) &&
                   !string.IsNullOrWhiteSpace(DefaultValue) &&
                   DefaultValue.Equals(defaultDescription.DefaultValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode() {
            return (base.GetHashCode() ^
                   Column?.GetHashCode() ^
                   DefaultValue?.GetHashCode()).Value;
        }
    }
}