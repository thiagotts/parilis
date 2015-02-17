using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using NUnit.Framework;
using SqlServer.Enums;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class TablesTests : DatabaseTest {
        private ITable tables;
        private IDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            tables = Components.Instance.GetComponent<ITable>(ConnectionInfo);
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo);
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
        }

        [SetUp]
        public void InitializeTest() {
            Database.Tables.Refresh();
            var table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE", "testschema"];
            if (table != null) table.Drop();
        }

        private readonly string[] InvalidTableNames = Enums.GetDescriptions<Keyword>().Concat(new List<string> {
            "123abc", "abc abc", "%abc", "$abc", "ab*c", "", "  ", null, new string('a', 129)
        }).ToArray();

        [Test]
        public void IfThereIsNotAnotherTableWithTheSameNameInTheSameSchema_CreateMethodMustCreateTheTable() {
            tables.Create(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns = new List<ColumnDescription> {
                    new ColumnDescription {
                        Name = "id",
                        Type = "varchar",
                        MaximumSize = "150",
                        AllowsNull = true
                    }
                }
            });

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNotNull(table);
            Assert.AreEqual("TEST_TABLE", table.Name);
            Assert.AreEqual(1, table.Columns.Count);
            Assert.AreEqual("id", table.Columns.Single().Name);
            Assert.AreEqual("varchar", table.Columns.Single().Type);
            Assert.AreEqual("150", table.Columns.Single().MaximumSize);
            Assert.IsTrue(table.Columns.Single().AllowsNull);
        }

        [Test]
        public void IfThereIsAnotherTableWithTheSameNameInAnotherSchema_CreateMethodMustCreateTheTable() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            tables.Create(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns = new List<ColumnDescription> {
                    new ColumnDescription {
                        Name = "id",
                        Type = "bigint"
                    }
                }
            });

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNotNull(table);
            Assert.AreEqual("TEST_TABLE", table.Name);
            Assert.AreEqual(1, table.Columns.Count);
            Assert.AreEqual("id", table.Columns.Single().Name);
            Assert.AreEqual("bigint", table.Columns.Single().Type);
            Assert.IsFalse(table.Columns.Single().AllowsNull);
        }

        [Test]
        public void IfThereIsAnotherTableWithTheSameNameInTheSameSchema_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidTableNameException>(() => tables.Create(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns = new List<ColumnDescription> {
                    new ColumnDescription {
                        Name = "id",
                        Type = "bigint"
                    }
                }
            }));
        }

        [Test, TestCaseSource("InvalidTableNames")]
        public void IfTableHasAnInvalidName_CreateMethodMustThrowException(string tableName) {
            Assert.Throws<InvalidTableNameException>(() => tables.Create(new TableDescription {
                Schema = "dbo",
                Name = tableName,
                Columns = new List<ColumnDescription> {
                    new ColumnDescription {
                        Name = "id",
                        Type = "bigint"
                    }
                }
            }));
        }

        [Test]
        public void IfTableExistsWithNoDataAndIsNotReferencedByAnyConstraint_RemoveMethodMustRemoveTheTable() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            tables.Remove("dbo", "TEST_TABLE");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableExistsWithExistentDataAndIsNotReferencedByAnyConstraint_RemoveMethodMustRemoveTheTable() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                INSERT INTO [dbo].[TEST_TABLE] VALUES (1, 'test');");

            tables.Remove("dbo", "TEST_TABLE");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableHasAPrimaryKey_RemoveMethodMustRemoveTheTable() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            tables.Remove("dbo", "TEST_TABLE");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableHasAForeignKey_RemoveMethodMustRemoveTheTable() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT FK_TEST FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            tables.Remove("dbo", "TEST_TABLE_2");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE_2");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableIsReferencedByAForeignKey_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT FK_TEST FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            Assert.Throws<ReferencedTableException>(() => tables.Remove("dbo", "TEST_TABLE"));
        }

        [Test]
        public void IfTableHasAUniqueKey_RemoveMethodMustRemoveTheTable() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            tables.Remove("dbo", "TEST_TABLE");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableHasADefaultConstraint_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            tables.Remove("dbo", "TEST_TABLE");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableHasAnIndex_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL);
                CREATE INDEX idx_TEST_TABLE_id2 ON [dbo].[TEST_TABLE](id2)");

            tables.Remove("dbo", "TEST_TABLE");

            var table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

            Assert.IsNull(table);
        }

        [Test]
        public void IfTableDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<TableNotFoundException>(() => tables.Remove("dbo", "TEST_TABLE"));
        }
    }
}