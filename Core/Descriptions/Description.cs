using System;

namespace Core.Descriptions {
    public abstract class Description {
        public abstract string FullName { get; }

        public override bool Equals(object other) {
            if (ReferenceEquals(null, other)) return false;
            if (!GetType().FullName.Equals(other.GetType().FullName)) return false;
            if (ReferenceEquals(this, other)) return true;
            var description = other as Description;
            return FullName.Equals(description.FullName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode() {
            return GetType().FullName.GetHashCode() ^
                   FullName.GetHashCode();
        }
    }
}