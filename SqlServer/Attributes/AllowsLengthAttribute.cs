using System;

namespace SqlServer.Attributes {
    public class AllowsLengthAttribute : Attribute {
        internal readonly bool AllowsLength;
        internal readonly int MinimumValue;
        internal readonly int MaximumValue;
        internal readonly bool AllowsMax;

        public AllowsLengthAttribute(bool allowsLength) {
            AllowsLength = allowsLength;
        }

        public AllowsLengthAttribute(bool allowsLength, int minimumValue, int maximumValue, bool allowsMax) {
            AllowsLength = allowsLength;
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            AllowsMax = allowsMax;
        }
    }
}