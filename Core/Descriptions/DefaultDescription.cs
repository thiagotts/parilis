using System;

namespace Core.Descriptions {
    public class DefaultDescription : ConstraintDescription {
        public string ColumnName { get; set; }
        public string DefaultValue { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is DefaultDescription)) return false;
            var defaultDescription = other as DefaultDescription;
            return base.Equals(other) &&
                   ColumnName.Equals(defaultDescription.ColumnName, StringComparison.InvariantCultureIgnoreCase) &&
                   DefaultValue.Equals(defaultDescription.DefaultValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^
                   ColumnName.GetHashCode() ^
                   DefaultValue.GetHashCode();
        }
    }
}