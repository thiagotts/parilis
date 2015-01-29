using System.Linq;
using Core.Descriptions;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class SqlServerDatabaseTests : DatabaseTest {

        [TearDown]
        public void FinishTest() {
            var table = Database.Tables["TEST_TABLE"];
            if(table != null) table.Drop();
        }

        [Test]
        public void WhenTableHasAPrimaryKeyReferringASingleColumn_MustReturnThePrimaryKeyDescriptionWithTheColumnName() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription { Schema = "dbo", Name = "TEST_TABLE" });

            Assert.IsNotNull(result);
            Assert.AreEqual("dbo", result.Schema);
            Assert.AreEqual("TEST_TABLE", result.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", result.Name);
            Assert.AreEqual(1, result.ColumnNames.Count());
            Assert.AreEqual("id", result.ColumnNames.Single());
        }

        [Test]
        public void WhenTableHasAPrimaryKeyReferringMultipleColumns_MustReturnThePrimaryKeyDescriptionWithAllColumnNames() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id, id2))");

            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription { Schema = "dbo", Name = "TEST_TABLE" });

            Assert.IsNotNull(result);
            Assert.AreEqual("dbo", result.Schema);
            Assert.AreEqual("TEST_TABLE", result.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", result.Name);
            Assert.AreEqual(2, result.ColumnNames.Count());
            Assert.AreEqual("id", result.ColumnNames.First());
            Assert.AreEqual("id2", result.ColumnNames.Last());
        }

        [Test]
        public void WhenTableDowsNotHaveAPrimaryKey_MustReturnNull() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            SqlServerDatabase sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNull(result);
        }
    }
}