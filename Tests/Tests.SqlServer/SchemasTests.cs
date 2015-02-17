using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Exceptions;
using Core.Interfaces;
using NUnit.Framework;
using SqlServer;
using SqlServer.Enums;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class SchemasTests : DatabaseTest {
        private ISchema schemas;
        private SqlServerDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            schemas = Components.Instance.GetComponent<ISchema>(ConnectionInfo);
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo) as SqlServerDatabase;
        }

        [SetUp]
        public void InitializeTest() {
            Database.Tables.Refresh();
            var table = Database.Tables["TEST_TABLE", "schema1"];
            if (table != null) table.Drop();

            Database.Schemas.Refresh();
            var schema = Database.Schemas["schema1"];
            if (schema != null) schema.Drop();
        }

        private readonly string[] InvalidSchemaNames = Enums.GetDescriptions<Keyword>().Concat(new List<string> {
            "123abc", "abc abc", "%abc", "$abc", "ab*c", "", "  ", null, new string('a', 129)
        }).ToArray();

        [Test]
        public void IfThereIsNotAnotherSchemaWithTheSameName_CreateMethodMustCreateSchema() {
            schemas.Create("schema1");

            var result = sqlServerDatabase.SchemaExists("schema1");

            Assert.IsTrue(result);
        }

        [Test]
        public void IfThereIsAnotherSchemaWithTheSameName_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE SCHEMA schema1");

            Assert.Throws<InvalidSchemaNameException>(() => schemas.Create("schema1"));
        }

        [Test, TestCaseSource("InvalidSchemaNames")]
        public void IfSchemaNameIsInvalid_CreateMethodMustThrowException(string schemaName) {
            Assert.Throws<InvalidSchemaNameException>(() => schemas.Create(schemaName));
        }

        [Test]
        public void IfSchemaExistsAndHasNoTables_RemoveMethodMustRemoveTheSchema() {
            Database.ExecuteNonQuery(@"CREATE SCHEMA schema1");

            schemas.Remove("schema1");

            var result = sqlServerDatabase.SchemaExists("schema1");
            Assert.IsFalse(result);
        }

        [Test]
        public void IfSchemaExistsAndHasTables_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE SCHEMA schema1");
            Database.ExecuteNonQuery(@"CREATE TABLE [schema1].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<ReferencedSchemaException>(() => schemas.Remove("schema1"));
        }

        [Test]
        public void IfSchemaDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<SchemaNotFoundException>(() => schemas.Remove("schema1"));
        }

        [Test]
        public void IfTheSchemaIsDbo_RemoveMethodMustThrowException() {
            Assert.Throws<ReferencedSchemaException>(() => schemas.Remove("dbo"));
        }
    }
}