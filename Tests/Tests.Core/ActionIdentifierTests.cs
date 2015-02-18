using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Actions;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class ActionIdentifierTests : MockTest {
        private DatabaseDescription actualDatabase;
        private DatabaseDescription referenceDatabase;
        private ActionIdentifier actionIdentifier;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<ITable>();
            Mock<IConstraint>();
            Mock<IIndex>();
            Mock<IColumn>();
            Mock<ISchema>();
        }

        [SetUp]
        public void InitializeTest() {
            var database = Components.Instance.GetComponent<IDatabase>(new ConnectionInfo());
            database.GetSchemas().Returns(new List<string>(), new List<string>());
            database.GetTables().Returns(new List<TableDescription>(), new List<TableDescription>());
            database.GetIndexes().Returns(new List<IndexDescription>(), new List<IndexDescription>());
            database.GetPrimaryKeys().Returns(new List<PrimaryKeyDescription>(), new List<PrimaryKeyDescription>());
            database.GetForeignKeys().Returns(new List<ForeignKeyDescription>(), new List<ForeignKeyDescription>());
            database.GetUniqueKeys().Returns(new List<UniqueDescription>(), new List<UniqueDescription>());
            database.GetDefaults().Returns(new List<DefaultDescription>(), new List<DefaultDescription>());

            actualDatabase = new DatabaseDescription(new ConnectionInfo());
            referenceDatabase = new DatabaseDescription(new ConnectionInfo());
            actionIdentifier = new ActionIdentifier(actualDatabase, referenceDatabase);
        }

        [Test]
        public void WhenActualDatabaseIsEqualToReferenceDatabase_MustReturnAnEmptyList() {
            actualDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE"});
            referenceDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(0, actions.Count);
        }

        [Test]
        public void WhenActualDatabaseHasADefaultThatReferenceDatabaseDoesNot_MustReturnADefaultRemovalAction() {
            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default1"});
            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "default2"});
            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default1"});
            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "default2"});

            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default2"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is DefaultRemoval);
            Assert.AreEqual("dbo", (actions.Single() as DefaultRemoval).DefaultDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (actions.Single() as DefaultRemoval).DefaultDescription.TableName);
            Assert.AreEqual("default2", (actions.Single() as DefaultRemoval).DefaultDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAUniqueKeyThatReferenceDatabaseDoesNot_MustReturnAUniqueRemovalAction() {
            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1"});
            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2"});
            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1"});
            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2"});

            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique2"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is UniqueRemoval);
            Assert.AreEqual("dbo", (actions.Single() as UniqueRemoval).UniqueDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (actions.Single() as UniqueRemoval).UniqueDescription.TableName);
            Assert.AreEqual("unique2", (actions.Single() as UniqueRemoval).UniqueDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAForeignKeyThatReferenceDatabaseDoesNot_MustReturnAForeignKeyRemovalAction() {
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1"});
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2"});
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1"});
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2"});

            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign2"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is ForeignKeyRemoval);
            Assert.AreEqual("dbo", (actions.Single() as ForeignKeyRemoval).ForeignKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (actions.Single() as ForeignKeyRemoval).ForeignKeyDescription.TableName);
            Assert.AreEqual("foreign2", (actions.Single() as ForeignKeyRemoval).ForeignKeyDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAPrimaryKeyThatReferenceDatabaseDoesNot_MustReturnAPrimaryKeyRemovalAction() {
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1"});
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2"});
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1"});
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2"});

            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary2"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is PrimaryKeyRemoval);
            Assert.AreEqual("dbo", (actions.Single() as PrimaryKeyRemoval).PrimaryKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (actions.Single() as PrimaryKeyRemoval).PrimaryKeyDescription.TableName);
            Assert.AreEqual("primary2", (actions.Single() as PrimaryKeyRemoval).PrimaryKeyDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAnIndexThatReferenceDatabaseDoesNot_MustReturnAnIndexRemovalAction() {
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index1"});
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "index2"});
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index1"});
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "index2"});

            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE_2", Name = "index1"});
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index2"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.IsTrue(actions.All(a => a is IndexRemoval));
            Assert.IsTrue(actions.Cast<IndexRemoval>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexRemoval>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexRemoval>().Any(a => a.IndexDescription.Name.Equals("index2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexRemoval>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexRemoval>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE_2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexRemoval>().Any(a => a.IndexDescription.Name.Equals("index1", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public void WhenActualDatabaseHasAColumnThatReferenceDatabaseDoesNot_MustReturnAColumnRemovalAction() {
            actualDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {
                new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"},
                new ColumnDescription {Name = "column2", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"}
            }});
            actualDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {new ColumnDescription {Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int"}}});
            referenceDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"}}});
            referenceDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {new ColumnDescription {Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int"}}});

            actualDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2", Columns = new List<ColumnDescription> {new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE_2", Type = "int"}}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is ColumnRemoval));
            var columnRemoval = actions.Single(a => a is ColumnRemoval) as ColumnRemoval;
            Assert.IsTrue(columnRemoval.ColumnDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnRemoval.ColumnDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnRemoval.ColumnDescription.Name.Equals("column2", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WhenActualDatabaseHasATableThatReferenceDatabaseDoesNot_MustReturnATableRemovalAction() {
            actualDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE"});
            actualDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE_2"});
            referenceDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is TableRemoval);
            Assert.AreEqual("TEST_TABLE_2", (actions.Single() as TableRemoval).TableDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasASchemaThatReferenceDatabaseDoesNot_MustReturnASchemaRemovalAction() {
            actualDatabase.Schemas.Add("dbo");
            actualDatabase.Schemas.Add("testschema");
            actualDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});
            actualDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE"});
            referenceDatabase.Schemas.Add("dbo");
            referenceDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is TableRemoval));
            Assert.AreEqual(1, actions.Count(a => a is SchemaRemoval));
            var schemaRemoval = actions.Single(a => a is SchemaRemoval) as SchemaRemoval;
            Assert.AreEqual("testschema", schemaRemoval.SchemaName);
        }

        [Test]
        public void WhenActualDatabaseHasAColumnWithDifferentDataTypeWhenComparedToReferenceDatabase_MustReturnAColumnModificationAction() {
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "int"},
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "bigint"},
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is ColumnModification));
            var columnModification = actions.Single(a => a is ColumnModification) as ColumnModification;
            Assert.IsTrue(columnModification.ColumnDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnModification.ColumnDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnModification.ColumnDescription.Name.Equals("column1", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WhenActualDatabaseHasAColumnWithDifferentMaximumSizeWhenComparedToReferenceDatabase_MustReturnAColumnModificationAction() {
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", MaximumSize = "100"},
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", MaximumSize = "255"},
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is ColumnModification));
            var columnModification = actions.Single(a => a is ColumnModification) as ColumnModification;
            Assert.IsTrue(columnModification.ColumnDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnModification.ColumnDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnModification.ColumnDescription.Name.Equals("column1", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WhenActualDatabaseHasAColumnAllowingNullAndReferenceDatabaseDoesNot_MustReturnAColumnModificationAction() {
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns = new List<ColumnDescription> {
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", MaximumSize = "255", AllowsNull = true},
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns = new List<ColumnDescription> {
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", MaximumSize = "255", AllowsNull = false},
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is ColumnModification));
            var columnModification = actions.Single(a => a is ColumnModification) as ColumnModification;
            Assert.IsTrue(columnModification.ColumnDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnModification.ColumnDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnModification.ColumnDescription.Name.Equals("column1", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WhenReferenceDatabaseHasASchemaThatActualDatabaseDoesNot_MustReturnASchemaCreationAction() {
            referenceDatabase.Schemas.Add("dbo");
            referenceDatabase.Schemas.Add("testschema");
            referenceDatabase.Tables.Add(new TableDescription { Schema = "dbo", Name = "TEST_TABLE" });
            referenceDatabase.Tables.Add(new TableDescription { Schema = "testschema", Name = "TEST_TABLE" });

            actualDatabase.Schemas.Add("dbo");
            actualDatabase.Tables.Add(new TableDescription { Schema = "dbo", Name = "TEST_TABLE" });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is TableCreation));
            Assert.AreEqual(1, actions.Count(a => a is SchemaCreation));
            var schemaCreation = actions.Single(a => a is SchemaCreation) as SchemaCreation;
            Assert.AreEqual("testschema", schemaCreation.SchemaName);
        }

        [Test]
        public void WhenReferenceDatabaseHasATableThatActualDatabaseDoesNot_MustReturnATableCreationAction() {
            referenceDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });
            referenceDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE_2" });

            actualDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is TableCreation);
            Assert.AreEqual("TEST_TABLE_2", (actions.Single() as TableCreation).TableDescription.Name);
        }

        [Test]
        public void WhenReferenceDatabaseHasAColumnThatActualDatabaseDoesNot_MustReturnAColumnCreationAction() {
            referenceDatabase.Tables.Add(new TableDescription { Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> {
                new ColumnDescription { Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int" },
                new ColumnDescription { Name = "column2", Schema = "dbo", TableName = "TEST_TABLE", Type = "int" }
            } });
            referenceDatabase.Tables.Add(new TableDescription { Schema = "testschema", Name = "TEST_TABLE", Columns = new List<ColumnDescription> { new ColumnDescription { Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int" } } });
            actualDatabase.Tables.Add(new TableDescription { Schema = "dbo", Name = "TEST_TABLE", Columns = new List<ColumnDescription> { new ColumnDescription { Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int" } } });
            actualDatabase.Tables.Add(new TableDescription { Schema = "testschema", Name = "TEST_TABLE", Columns = new List<ColumnDescription> { new ColumnDescription { Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int" } } });

            referenceDatabase.Tables.Add(new TableDescription { Schema = "dbo", Name = "TEST_TABLE_2", Columns = new List<ColumnDescription> { new ColumnDescription { Name = "column1", Schema = "dbo", TableName = "TEST_TABLE_2", Type = "int" } } });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Count(a => a is TableCreation));
            Assert.AreEqual(1, actions.Count(a => a is ColumnCreation));
            var columnCreation = actions.Single(a => a is ColumnCreation) as ColumnCreation;
            Assert.IsTrue(columnCreation.ColumnDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnCreation.ColumnDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnCreation.ColumnDescription.Name.Equals("column2", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WhenReferenceDatabaseHasAnIndexThatActualDatabaseDoesNot_MustReturnAnIndexCreationAction() {
            referenceDatabase.Indexes.Add(new IndexDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "index1" });
            referenceDatabase.Indexes.Add(new IndexDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "index2" });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "index1" });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "index2" });

            referenceDatabase.Indexes.Add(new IndexDescription { Schema = "dbo", TableName = "TEST_TABLE_2", Name = "index1" });
            referenceDatabase.Indexes.Add(new IndexDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "index2" });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.IsTrue(actions.All(a => a is IndexCreation));
            Assert.IsTrue(actions.Cast<IndexCreation>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexCreation>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexCreation>().Any(a => a.IndexDescription.Name.Equals("index2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexCreation>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexCreation>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE_2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Cast<IndexCreation>().Any(a => a.IndexDescription.Name.Equals("index1", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public void WhenReferenceDatabaseHasAPrimaryKeyThatActualDatabaseDoesNot_MustReturnAPrimaryKeyCreationAction() {
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1" });
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2" });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1" });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2" });

            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "primary2" });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is PrimaryKeyCreation);
            Assert.AreEqual("dbo", (actions.Single() as PrimaryKeyCreation).PrimaryKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (actions.Single() as PrimaryKeyCreation).PrimaryKeyDescription.TableName);
            Assert.AreEqual("primary2", (actions.Single() as PrimaryKeyCreation).PrimaryKeyDescription.Name);
        }

        [Test]
        public void WhenReferenceDatabaseHasAForeignKeyThatActualDatabaseDoesNot_MustReturnAForeignKeyCreationAction() {
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1" });
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2" });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1" });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2" });

            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign2" });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.IsTrue(actions.Single() is ForeignKeyCreation);
            Assert.AreEqual("dbo", (actions.Single() as ForeignKeyCreation).ForeignKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (actions.Single() as ForeignKeyCreation).ForeignKeyDescription.TableName);
            Assert.AreEqual("foreign2", (actions.Single() as ForeignKeyCreation).ForeignKeyDescription.Name);
        }
    }
}