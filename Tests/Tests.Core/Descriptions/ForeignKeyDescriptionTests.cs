using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class ForeignKeyDescriptionTests {
        [Test]
        public void WhenForeignKeysHaveDifferentFullNames_EqualsMustReturnFalse() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Foreign1",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var foreign2 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Foreign2",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var result = foreign1.Equals(foreign2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenForeignKeysHaveADifferentAmountOfColumns_EqualsMustReturnFalse() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var foreign2 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}}
                }
            };

            var result = foreign1.Equals(foreign2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenForeignKeysHaveADifferentColumnName_EqualsMustReturnFalse() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var foreign2 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column3", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var result = foreign1.Equals(foreign2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenForeignKeysHaveADifferentReferencedColumn_EqualsMustReturnFalse() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var foreign2 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn3"}}
                }
            };

            var result = foreign1.Equals(foreign2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenForeignKeysHaveTheSameFullNameAndColumns_EqualsMustReturnTrue() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1", Type = "int"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2", Type = "int"}}
                }
            };

            var foreign2 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1", Type = "int"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2", Type = "int"}}
                }
            };

            var result = foreign1.Equals(foreign2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfTheForeignKeysIsNull_EqualsMustReturnFalse() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var result = foreign1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotAForeignKeyDescription_EqualsMustReturnFalse() {
            var foreign1 = new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign",
                Columns = new Dictionary<string, ColumnDescription> {
                    {"column1", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn1"}},
                    {"column2", new ColumnDescription{Schema = "dbo", TableName = "table1", Name = "rcolumn2"}}
                }
            };

            var unique = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table",
                Name = "Foreign"
            };

            var result = foreign1.Equals(unique);

            Assert.False(result);
        }
    }
}