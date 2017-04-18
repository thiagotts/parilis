using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class TableDescriptionTests {
        [Test]
        public void WhenTablesHaveDifferentFullNames_EqualsMustReturnFalse() {
            var table1 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2"}
                }
            };

            var table2 = new TableDescription {
                Schema = "dbo",
                Name = "Table2",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2"}
                }
            };

            var result = table1.Equals(table2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenTablesHaveADifferentAmountOfColumns_EqualsMustReturnFalse() {
            var table1 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2"}
                }
            };

            var table2 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"}
                }
            };

            var result = table1.Equals(table2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenTablesHaveDifferentColumns_EqualsMustReturnFalse() {
            var table1 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2"}
                }
            };

            var table2 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column3"}
                }
            };

            var result = table1.Equals(table2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenTablesHaveTheSameFullNameAndColumns_EqualsMustReturnTrue() {
            var table1 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1", Type = "int"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2", Type = "int"}
                }
            };

            var table2 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1", Type = "int"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2", Type = "int"}
                }
            };

            var result = table1.Equals(table2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfTheTablesIsNull_EqualsMustReturnFalse() {
            var table1 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2"}
                }
            };

            var result = table1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotATableDescription_EqualsMustReturnFalse() {
            var table1 = new TableDescription {
                Schema = "dbo",
                Name = "Table1",
                Columns =  {
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column1"},
                    new ColumnDescription{Schema = "dbo", TableName = "Table1", Name = "column2"}
                }
            };

            var unique = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1"
            };

            var result = table1.Equals(unique);

            Assert.False(result);
        }
    }
}