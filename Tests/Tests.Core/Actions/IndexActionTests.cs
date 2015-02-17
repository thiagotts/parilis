using Core.Actions;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core.Actions {
    [TestFixture]
    public class IndexActionTests : MockTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<IIndex>();
        }

        [Test]
        public void IndexCreationMustCallIndexesCreateMethod() {
            var indexDescription = new IndexDescription { Name = "test_name" };
            var defaultCreation = new IndexCreation(ConnectionInfo, indexDescription);

            defaultCreation.Execute();

            defaultCreation.Indexes.Received(1).Create(Arg.Is<IndexDescription>(
                d => d.Name.Equals(indexDescription.Name)));
        }

        [Test]
        public void IndexRemovalMustCallIndexesRemoveMethod() {
            var indexDescription = new IndexDescription { Name = "test_name" };
            var defaultCreation = new IndexRemoval(ConnectionInfo, indexDescription);

            defaultCreation.Execute();

            defaultCreation.Indexes.Received(1).Remove(Arg.Is<IndexDescription>(
                d => d.Name.Equals(indexDescription.Name)));
        }
    }
}