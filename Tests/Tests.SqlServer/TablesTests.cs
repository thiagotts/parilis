using System.Collections.Generic;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class TablesTests : DatabaseTest {
        private Tables tables;
        private SqlServerDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            tables = new Tables(Database);
            sqlServerDatabase = new SqlServerDatabase(Database);
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
        }

        [TearDown]
        public void FinishTest() {
            var table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();
        }

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

            TableDescription table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

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

            TableDescription table = sqlServerDatabase.GetTable("dbo", "TEST_TABLE");

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
    }
}