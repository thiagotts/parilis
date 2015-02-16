using System.Collections.Generic;
using System.Linq;
using Core.Exceptions;
using NUnit.Framework;
using SqlServer;
using SqlServer.Enums;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class SchemasTests : DatabaseTest {
        private Schemas schemas;
        private SqlServerDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            schemas = new Schemas(Database);
            sqlServerDatabase = new SqlServerDatabase(Database);
        }

        [SetUp]
        public void InitializeTest() {
            Database.Schemas.Refresh();
            var schema = Database.Schemas["schema1"];
            if (schema != null) schema.Drop();

            schema = Database.Schemas["schema2"];
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
    }
}