using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class DefaultDescriptionTests {
        [Test]
        public void WhenDefaultsHaveDifferentFullNames_EqualsMustReturnFalse() {
            var default1 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var default2 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default2",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var result = default1.Equals(default2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenDefaultsHaveDifferentColumnNames_EqualsMustReturnFalse() {
            var default1 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var default2 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column2",
                DefaultValue = "1"
            };

            var result = default1.Equals(default2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenDefaultsHaveDifferentDefaultValues_EqualsMustReturnFalse() {
            var default1 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var default2 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "0"
            };

            var result = default1.Equals(default2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenDefaultsTheSameFullNameAndColumnNameAndDefaultValue_EqualsMustReturnTrue() {
            var default1 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var default2 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var result = default1.Equals(default2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfTheDefaultsIsNull_EqualsMustReturnFalse() {
            var default1 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var result = default1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotADefaultsDescription_EqualsMustReturnFalse() {
            var default1 = new DefaultDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1",
                ColumnName = "Column1",
                DefaultValue = "1"
            };

            var unique = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1"
            };

            var result = default1.Equals(unique);

            Assert.False(result);
        }
    }
}