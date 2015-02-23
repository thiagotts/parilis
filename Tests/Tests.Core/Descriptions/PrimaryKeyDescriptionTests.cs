using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class PrimaryKeyDescriptionTests {
        [Test]
        public void WhenPrimaryKeysHaveDifferentFullNames_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> {"column1", "column2"}
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary2",
                ColumnNames = new List<string> { "column1", "column2" }
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenPrimaryKeysHaveADifferentAmountOfColumnNames_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> { "column1", "column2" }
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> { "column1" }
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenPrimaryKeysHaveDifferentColumnNames_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> {"column1", "column2"}
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> {"column1", "column3"}
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenPrimaryKeysHaveTheSameFullNameAndColumnNames_EqualsMustReturnTrue() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> { "column1", "column2" }
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> { "column1", "column2" }
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfThePrimaryKeysIsNull_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> { "column1", "column2" }
            };

            var result = primaryKey1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotAPrimaryKeyDescription_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                ColumnNames = new List<string> { "column1", "column2" }
            };

            var unique = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1"
            };

            var result = primaryKey1.Equals(unique);

            Assert.False(result);
        }
    }
}