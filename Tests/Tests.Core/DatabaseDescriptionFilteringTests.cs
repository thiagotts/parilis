using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class DatabaseDescriptionFilteringTests : MockTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<IDatabase>();
        }

        [SetUp]
        public void InitializeTest() {}

        [Test]
        public void FilterBySchemaMustReturnADatabaseDescriptioOnlyWithElementsWithinTheSpecifiedSchema() {
            var description = new DatabaseDescription(Substitute.For<ConnectionInfo>()) {
                Schemas = new List<string> {"schema1", "schema2"},
                Tables = new List<TableDescription> {
                    new TableDescription {Schema = "schema1", Name = "Table1"},
                    new TableDescription {Schema = "schema2", Name = "Table2"},
                },
                Indexes = new List<IndexDescription> {
                    new IndexDescription {Schema = "schema1", Name = "Index1"},
                    new IndexDescription {Schema = "schema2", Name = "Index2"}
                },
                PrimaryKeys = new List<PrimaryKeyDescription> {
                    new PrimaryKeyDescription {Schema = "schema1", Name = "PK1"},
                    new PrimaryKeyDescription {Schema = "schema2", Name = "PK2"}
                },
                ForeignKeys = new List<ForeignKeyDescription> {
                    new ForeignKeyDescription {Schema = "schema1", Name = "FK1"},
                    new ForeignKeyDescription {Schema = "schema2", Name = "FK2"}
                },
                UniqueKeys = new List<UniqueDescription> {
                    new UniqueDescription {Schema = "schema1", Name = "UK1"},
                    new UniqueDescription {Schema = "schema2", Name = "UK2"}
                },
                Defaults = new List<DefaultDescription> {
                    new DefaultDescription {Schema = "schema1", Name = "Default1"},
                    new DefaultDescription {Schema = "schema2", Name = "Default2"}
                }
            };

            var result = description.FilterBySchema(new[] {"schema1"});

            Assert.IsTrue(result.Schemas.Count == 1 && result.Schemas.Single().Equals("schema1"));
            Assert.IsTrue(result.Tables.Count == 1 && result.Tables.Single().Schema.Equals("schema1"));
            Assert.IsTrue(result.Indexes.Count == 1 && result.Indexes.Single().Schema.Equals("schema1"));
            Assert.IsTrue(result.PrimaryKeys.Count == 1 && result.PrimaryKeys.Single().Schema.Equals("schema1"));
            Assert.IsTrue(result.ForeignKeys.Count == 1 && result.ForeignKeys.Single().Schema.Equals("schema1"));
            Assert.IsTrue(result.UniqueKeys.Count == 1 && result.UniqueKeys.Single().Schema.Equals("schema1"));
            Assert.IsTrue(result.Defaults.Count == 1 && result.Defaults.Single().Schema.Equals("schema1"));
        }

        [Test]
        public void EmptyOrNullSchemasShouldBeIgnored() {
            var description = new DatabaseDescription(Substitute.For<ConnectionInfo>()) {
                Schemas = new List<string> { "schema1", "schema2" },
                Tables = new List<TableDescription> {
                    new TableDescription {Schema = "schema1", Name = "Table1"},
                    new TableDescription {Schema = "schema2", Name = "Table2"},
                }
            };

            var result = description.FilterBySchema(new[] {"schema1", string.Empty, "  ", null});

            Assert.IsTrue(result.Schemas.Count == 1 && result.Schemas.Single().Equals("schema1"));
            Assert.IsTrue(result.Tables.Count == 1 && result.Tables.Single().Schema.Equals("schema1"));
        }

        [Test]
        public void NonExistingSchemasShouldBeIgnored() {
            var description = new DatabaseDescription(Substitute.For<ConnectionInfo>()) {
                Schemas = new List<string> { "schema1", "schema2" },
                Tables = new List<TableDescription> {
                    new TableDescription {Schema = "schema1", Name = "Table1"},
                    new TableDescription {Schema = "schema2", Name = "Table2"},
                }
            };

            var result = description.FilterBySchema(new[] {"schema1", "schema3", "schema4"});

            Assert.IsTrue(result.Schemas.Count == 1 && result.Schemas.Single().Equals("schema1"));
            Assert.IsTrue(result.Tables.Count == 1 && result.Tables.Single().Schema.Equals("schema1"));
        }
      
      
    }
}