using System.Collections.Generic;
using Core;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core{
    [TestFixture]
    public class DatabaseDescriptionEqualityTests : MockTest{

        private DatabaseDescription actualDatabase;
        private DatabaseDescription referenceDatabase;
        private ActionIdentifier actionIdentifier;

        [TestFixtureSetUp]
        public override void InitializeClass(){
            base.InitializeClass();
            Mock<ITable>();
            Mock<IConstraint>();
            Mock<IIndex>();
            Mock<IColumn>();
            Mock<ISchema>();
        }

        [SetUp]
        public void InitializeTest(){
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
        }

        [Test]
        public void WhenActualDatabaseIsEqualToReferenceDatabase_MustReturnTrue(){
            actualDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });
            referenceDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence);
        }
        
        [Test]
        public void WhenActualDatabaseHaveAnyTableAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });
            actualDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE1" });
            referenceDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveAnyTableAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });
            referenceDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE" });
            referenceDatabase.Tables.Add(new TableDescription { Name = "TEST_TABLE1" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }

        [Test]
        public void WhenActualDatabaseHaveASchemaAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Schemas.Add("dbo");
            actualDatabase.Schemas.Add("any");
            referenceDatabase.Schemas.Add("dbo");

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveASchemaAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Schemas.Add("dbo");
            referenceDatabase.Schemas.Add("dbo");
            referenceDatabase.Schemas.Add("any");

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }

        [Test]
        public void WhenActualDatabaseHaveAnIndexAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Indexes.Add(new IndexDescription{Name = "Index"});
            actualDatabase.Indexes.Add(new IndexDescription{Name = "Index2"});
            referenceDatabase.Indexes.Add(new IndexDescription{Name = "Index"});

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveAnIndexAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Indexes.Add(new IndexDescription{Name = "Index"});
            referenceDatabase.Indexes.Add(new IndexDescription{Name = "Index"});
            referenceDatabase.Indexes.Add(new IndexDescription{Name = "Index1"});

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }

        [Test]
        public void WhenActualDatabaseHaveAPrimaryKeyAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription{Name = "PK1"});
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription{Name = "PK2"});
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription{Name = "PK1"});

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveAPrimaryKeyAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Name = "PK1" });
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Name = "PK1" });
            referenceDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Name = "PK2" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenActualDatabaseHaveAForeignKeyAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription{Name = "KK1"});
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription{Name = "FK2"});
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription{Name = "FK1"});

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveAForeignKeyAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Name = "FK1" });
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription { Name = "FK1" });
            referenceDatabase.ForeignKeys.Add(new ForeignKeyDescription { Name = "FK2" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }

        [Test]
        public void WhenActualDatabaseHaveAUniqueKeyAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.UniqueKeys.Add(new UniqueDescription{Name = "U1"});
            actualDatabase.UniqueKeys.Add(new UniqueDescription{Name = "U2"});
            referenceDatabase.UniqueKeys.Add(new UniqueDescription{Name = "U1"});

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveAUniqueKeyAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Name = "U1" });
            referenceDatabase.UniqueKeys.Add(new UniqueDescription { Name = "U1" });
            referenceDatabase.UniqueKeys.Add(new UniqueDescription { Name = "U2" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenActualDatabaseHaveADefaultAndReferenceDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Defaults.Add(new DefaultDescription{Name = "D1"});
            actualDatabase.Defaults.Add(new DefaultDescription{Name = "D2"});;
            referenceDatabase.Defaults.Add(new DefaultDescription{Name = "D3"});

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }
        
        [Test]
        public void WhenReferenceDatabaseHaveADefaultAndActualDatabaseDontHaveIt_MustReturnFalse(){
            actualDatabase.Defaults.Add(new DefaultDescription { Name = "D1" });
            referenceDatabase.Defaults.Add(new DefaultDescription { Name = "D1" });
            referenceDatabase.Defaults.Add(new DefaultDescription { Name = "D2" });

            var parilis = new Parilis(actualDatabase, referenceDatabase);
            var actualAlreadyEqualToReferecence = parilis.AreAlreadyEqual();

            Assert.That(actualAlreadyEqualToReferecence, Is.False);
        }

    }
}
