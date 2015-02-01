using System.Collections.Generic;
using Core.Descriptions;
using Core.Exceptions;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class ConstraintsTests : DatabaseTest {
        [TearDown]
        public void FinishTest() {
            var table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();
        }

        [Test]
        public void WhenTargetTableDoesNotHaveAPrimaryKey_CreateMethodMustCreateThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE",
                ColumnNames = new List<string> {"id"}
            };

            var constraints = new Constraints(Database);
            constraints.CreatePrimaryKey(primaryKey);

            var result = new SqlServerDatabase(Database).GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});
            Assert.IsNotNull(result);
            Assert.AreEqual("PK_dbo_TEST_TABLE", result.Name);
        }

        [Test]
        public void WhenTargetTableAlreadyHasAPrimaryKey_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE",
                ColumnNames = new List<string> {"id"}
            };

            var constraints = new Constraints(Database);
            Assert.Throws<MultiplePrimaryKeysException>(() => constraints.CreatePrimaryKey(primaryKey));
        }

        [Test]
        public void WhenAnotherTableWithinTheSameSchemaHasAPrimaryKeyWithTheSameName_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE_id",
                ColumnNames = new List<string> {"id"}
            };

            var constraints = new Constraints(Database);
            Assert.Throws<MultiplePrimaryKeysException>(() => constraints.CreatePrimaryKey(primaryKey));
        }

        [Test]
        public void WhenTableFromAnotherSchemaHasAPrimaryKeyWithTheSameName_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }
    }
}