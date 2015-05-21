using Core.Descriptions;

namespace Tests.Core {
    public class Test {
        protected ColumnDescription CreateColumnDescription(string name = null, string type = null, string length = null, bool allowsNull = true) {
            return new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = name ?? "column_test",
                Type = type ?? "type_test",
                Length = length ?? "10",
                AllowsNull = allowsNull,
                IsIdentity = false
            };
        }
    }
}