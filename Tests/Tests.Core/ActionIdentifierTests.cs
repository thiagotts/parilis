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
            var column = CreateColumnDescription();
            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default1", Column = column, DefaultValue = "0"});
            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "default2", Column = column, DefaultValue = "0"});
            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default1", Column = column, DefaultValue = "0"});
            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "default2", Column = column, DefaultValue = "0"});

            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default2", Column = column, DefaultValue = "0"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is DefaultRemoval);
            Assert.AreEqual("dbo", (action as DefaultRemoval).DefaultDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as DefaultRemoval).DefaultDescription.TableName);
            Assert.AreEqual("default2", (action as DefaultRemoval).DefaultDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAUniqueKeyThatReferenceDatabaseDoesNot_MustReturnAUniqueRemovalAction() {
            var column = CreateColumnDescription();
            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1", Columns =  {column}});
            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2", Columns =  {column}});
            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1", Columns =  {column}});
            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2", Columns =  {column}});

            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique2", Columns =  {column}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is UniqueRemoval);
            Assert.AreEqual("dbo", (action as UniqueRemoval).UniqueDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as UniqueRemoval).UniqueDescription.TableName);
            Assert.AreEqual("unique2", (action as UniqueRemoval).UniqueDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAUniqueKeyWithTheSameNameButDifferentColumnDescription_MustReturnAUniqueRemovalAndAUniqueCreationAction() {
            var column1 = CreateColumnDescription();
            var column2 = CreateColumnDescription(allowsNull: false);
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1", Columns =  { column1 } });
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2", Columns =  { column1 } });
            referenceDatabase.UniqueKeys.Add(new UniqueDescription { Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1", Columns =  { column2 } });
            referenceDatabase.UniqueKeys.Add(new UniqueDescription { Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2", Columns =  { column1 } });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            var action1 = actions.Pop();
            Assert.IsTrue(action1 is UniqueRemoval);
            Assert.AreEqual("dbo", (action1 as UniqueRemoval).UniqueDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action1 as UniqueRemoval).UniqueDescription.TableName);
            Assert.AreEqual("unique1", (action1 as UniqueRemoval).UniqueDescription.Name);

            var action2 = actions.Pop();
            Assert.IsTrue(action2 is UniqueCreation);
            Assert.AreEqual("dbo", (action2 as UniqueCreation).UniqueDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action2 as UniqueCreation).UniqueDescription.TableName);
            Assert.AreEqual("unique1", (action2 as UniqueCreation).UniqueDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAForeignKeyThatReferenceDatabaseDoesNot_MustReturnAForeignKeyRemovalAction() {
            var mockColumn = new ColumnDescription {Schema = "dbo", TableName = "table", Type = "int"};
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});

            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign2", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is ForeignKeyRemoval);
            Assert.AreEqual("dbo", (action as ForeignKeyRemoval).ForeignKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as ForeignKeyRemoval).ForeignKeyDescription.TableName);
            Assert.AreEqual("foreign2", (action as ForeignKeyRemoval).ForeignKeyDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAPrimaryKeyThatReferenceDatabaseDoesNot_MustReturnAPrimaryKeyRemovalAction() {
            var column = CreateColumnDescription();
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1", Columns =  {column}});
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2", Columns =  {column}});
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1", Columns =  {column}});
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2", Columns =  {column}});

            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary2", Columns =  {column}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is PrimaryKeyRemoval);
            Assert.AreEqual("dbo", (action as PrimaryKeyRemoval).PrimaryKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as PrimaryKeyRemoval).PrimaryKeyDescription.TableName);
            Assert.AreEqual("primary2", (action as PrimaryKeyRemoval).PrimaryKeyDescription.Name);
        }

        [Test]
        public void WhenActualDatabaseHasAnIndexThatReferenceDatabaseDoesNot_MustReturnAnIndexRemovalAction() {
            var column = CreateColumnDescription();
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index1", Columns =  {column}});
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "index2", Columns =  {column}});
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index1", Columns =  {column}});
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "index2", Columns =  {column}});

            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE_2", Name = "index1", Columns =  {column}});
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index2", Columns =  {column}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.IsTrue(actions.Queue.All(a => a is IndexRemoval));
            Assert.IsTrue(actions.Queue.Cast<IndexRemoval>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexRemoval>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexRemoval>().Any(a => a.IndexDescription.Name.Equals("index2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexRemoval>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexRemoval>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE_2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexRemoval>().Any(a => a.IndexDescription.Name.Equals("index1", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public void WhenActualDatabaseHasAColumnThatReferenceDatabaseDoesNot_MustReturnAColumnRemovalAction() {
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns = {
                    new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"},
                    new ColumnDescription {Name = "column2", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"}
                }
            });
            actualDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE", Columns =  {new ColumnDescription {Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int"}}});
            referenceDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE", Columns =  {new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"}}});
            referenceDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE", Columns =  {new ColumnDescription {Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int"}}});

            actualDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2", Columns =  {new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE_2", Type = "int"}}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Queue.Count(a => a is ColumnRemoval));
            var columnRemoval = actions.Queue.Single(a => a is ColumnRemoval) as ColumnRemoval;
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
            var action = actions.Pop();
            Assert.IsTrue(action is TableRemoval);
            Assert.AreEqual("TEST_TABLE_2", (action as TableRemoval).TableDescription.Name);
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
            Assert.AreEqual(1, actions.Queue.Count(a => a is TableRemoval));
            Assert.AreEqual(1, actions.Queue.Count(a => a is SchemaRemoval));
            var schemaRemoval = actions.Queue.Single(a => a is SchemaRemoval) as SchemaRemoval;
            Assert.AreEqual("testschema", schemaRemoval.SchemaName);
        }

        [Test]
        public void WhenActualDatabaseHasAColumnWithDifferentDataTypeWhenComparedToReferenceDatabase_MustReturnAColumnModificationAction() {
            var actualColumn = new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "int"};
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns =  {
                    actualColumn,
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var referenceColumn = new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "bigint"};
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns =  {
                    referenceColumn,
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, actions.Queue.Count(a => a is ColumnModification));
            var columnModification = actions.Queue.Single(a => a is ColumnModification) as ColumnModification;
            Assert.IsTrue(columnModification.ColumnDescription.Equals(referenceColumn));
        }

        [Test]
        public void WhenActualDatabaseHasAColumnWithDifferentMaximumSizeWhenComparedToReferenceDatabase_MustReturnAColumnModificationAction() {
            var actualColumn = new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", Length = "100"};
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns =  {
                    actualColumn,
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var referenceColumn = new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", Length = "255"};
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns =  {
                    referenceColumn,
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, actions.Queue.Count(a => a is ColumnModification));
            var columnModification = actions.Queue.Single(a => a is ColumnModification) as ColumnModification;
            Assert.IsTrue(columnModification.ColumnDescription.Equals(referenceColumn));
        }

        [Test]
        public void WhenActualDatabaseHasAColumnAllowingNullAndReferenceDatabaseDoesNot_MustReturnAColumnModificationAction() {
            var actualColumn = new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", Length = "255", AllowsNull = true};
            actualDatabase.Tables.Add(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns =  {
                    actualColumn,
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var referenceColumn = new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column1", Type = "varchar", Length = "255", AllowsNull = false};
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo",
                Name = "TEST_TABLE",
                Columns =  {
                    referenceColumn,
                    new ColumnDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "column2", Type = "int"}
                }
            });

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            Assert.AreEqual(1, actions.Queue.Count(a => a is ColumnModification));
            var columnModification = actions.Queue.Single(a => a is ColumnModification) as ColumnModification;
            Assert.IsTrue(columnModification.ColumnDescription.Equals(referenceColumn));
        }

        [Test]
        public void WhenReferenceDatabaseHasASchemaThatActualDatabaseDoesNot_MustReturnASchemaCreationAction() {
            referenceDatabase.Schemas.Add("dbo");
            referenceDatabase.Schemas.Add("testschema");
            referenceDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});
            referenceDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE"});

            actualDatabase.Schemas.Add("dbo");
            actualDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Queue.Count(a => a is TableCreation));
            Assert.AreEqual(1, actions.Queue.Count(a => a is SchemaCreation));
            var schemaCreation = actions.Queue.Single(a => a is SchemaCreation) as SchemaCreation;
            Assert.AreEqual("testschema", schemaCreation.SchemaName);
        }

        [Test]
        public void WhenReferenceDatabaseHasATableThatActualDatabaseDoesNot_MustReturnATableCreationAction() {
            referenceDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE"});
            referenceDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE_2"});

            actualDatabase.Tables.Add(new TableDescription {Name = "TEST_TABLE"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is TableCreation);
            Assert.AreEqual("TEST_TABLE_2", (action as TableCreation).TableDescription.Name);
        }

        [Test]
        public void WhenReferenceDatabaseHasAColumnThatActualDatabaseDoesNot_MustReturnAColumnCreationAction() {
            referenceDatabase.Tables.Add(new TableDescription {
                Schema = "dbo", Name = "TEST_TABLE", Columns =  {
                    new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"},
                    new ColumnDescription {Name = "column2", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"}
                }
            });
            referenceDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE", Columns =  {new ColumnDescription {Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int"}}});
            actualDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE", Columns =  {new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE", Type = "int"}}});
            actualDatabase.Tables.Add(new TableDescription {Schema = "testschema", Name = "TEST_TABLE", Columns =  {new ColumnDescription {Name = "column2", Schema = "testschema", TableName = "TEST_TABLE", Type = "int"}}});

            referenceDatabase.Tables.Add(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2", Columns =  {new ColumnDescription {Name = "column1", Schema = "dbo", TableName = "TEST_TABLE_2", Type = "int"}}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(1, actions.Queue.Count(a => a is TableCreation));
            Assert.AreEqual(1, actions.Queue.Count(a => a is ColumnCreation));
            var columnCreation = actions.Queue.Single(a => a is ColumnCreation) as ColumnCreation;
            Assert.IsTrue(columnCreation.ColumnDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnCreation.ColumnDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(columnCreation.ColumnDescription.Name.Equals("column2", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WhenReferenceDatabaseHasAnIndexThatActualDatabaseDoesNot_MustReturnAnIndexCreationAction() {
            var column = CreateColumnDescription();
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index1", Columns =  {column}});
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "index2", Columns =  {column}});
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index1", Columns =  {column}});
            actualDatabase.Indexes.Add(new IndexDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "index2", Columns =  {column}});

            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE_2", Name = "index1", Columns =  {column}});
            referenceDatabase.Indexes.Add(new IndexDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "index2", Columns =  {column}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(2, actions.Count);
            Assert.IsTrue(actions.Queue.All(a => a is IndexCreation));
            Assert.IsTrue(actions.Queue.Cast<IndexCreation>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexCreation>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexCreation>().Any(a => a.IndexDescription.Name.Equals("index2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexCreation>().Any(a => a.IndexDescription.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexCreation>().Any(a => a.IndexDescription.TableName.Equals("TEST_TABLE_2", StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(actions.Queue.Cast<IndexCreation>().Any(a => a.IndexDescription.Name.Equals("index1", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public void WhenReferenceDatabaseHasAPrimaryKeyThatActualDatabaseDoesNot_MustReturnAPrimaryKeyCreationAction() {
            var column = CreateColumnDescription();
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1", Columns =  {column}});
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2", Columns =  {column}});
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary1", Columns =  {column}});
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "primary2", Columns =  {column}});

            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "primary2", Columns =  {column}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is PrimaryKeyCreation);
            Assert.AreEqual("dbo", (action as PrimaryKeyCreation).PrimaryKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as PrimaryKeyCreation).PrimaryKeyDescription.TableName);
            Assert.AreEqual("primary2", (action as PrimaryKeyCreation).PrimaryKeyDescription.Name);
        }

        [Test]
        public void WhenReferenceDatabaseHasAForeignKeyThatActualDatabaseDoesNot_MustReturnAForeignKeyCreationAction() {
            var mockColumn = new ColumnDescription {Schema = "dbo", TableName = "table", Type = "int"};
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign1", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "foreign2", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});

            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "foreign2", Columns = new Dictionary<string, ColumnDescription> {{"column", mockColumn}}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is ForeignKeyCreation);
            Assert.AreEqual("dbo", (action as ForeignKeyCreation).ForeignKeyDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as ForeignKeyCreation).ForeignKeyDescription.TableName);
            Assert.AreEqual("foreign2", (action as ForeignKeyCreation).ForeignKeyDescription.Name);
        }

        [Test]
        public void WhenReferenceDatabaseHasAUniqueKeyThatActualDatabaseDoesNot_MustReturnAUniqueCreationAction() {
            var column = CreateColumnDescription();
            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1", Columns =  {column}});
            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2", Columns =  {column}});
            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique1", Columns =  {column}});
            actualDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "unique2", Columns =  {column}});

            referenceDatabase.UniqueKeys.Add(new UniqueDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "unique2", Columns =  {column}});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is UniqueCreation);
            Assert.AreEqual("dbo", (action as UniqueCreation).UniqueDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as UniqueCreation).UniqueDescription.TableName);
            Assert.AreEqual("unique2", (action as UniqueCreation).UniqueDescription.Name);
        }

        [Test]
        public void WhenReferenceDatabaseHasADefaultThatActualDatabaseDoesNot_MustReturnADefaultCreationAction() {
            var column = CreateColumnDescription();
            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default1", Column = column, DefaultValue = "0"});
            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "default2", Column = column, DefaultValue = "0"});
            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default1", Column = column, DefaultValue = "0"});
            actualDatabase.Defaults.Add(new DefaultDescription {Schema = "testschema", TableName = "TEST_TABLE", Name = "default2", Column = column, DefaultValue = "0"});

            referenceDatabase.Defaults.Add(new DefaultDescription {Schema = "dbo", TableName = "TEST_TABLE", Name = "default2", Column = column, DefaultValue = "0"});

            var actions = actionIdentifier.GetActions();

            Assert.IsNotNull(actions);
            Assert.AreEqual(1, actions.Count);
            var action = actions.Pop();
            Assert.IsTrue(action is DefaultCreation);
            Assert.AreEqual("dbo", (action as DefaultCreation).DefaultDescription.Schema);
            Assert.AreEqual("TEST_TABLE", (action as DefaultCreation).DefaultDescription.TableName);
            Assert.AreEqual("default2", (action as DefaultCreation).DefaultDescription.Name);
        }
    }
}