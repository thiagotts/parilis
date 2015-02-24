using Core;
using Core.Actions;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class ActionQueueTests : MockTest {
        private ActionQueue actionQueue;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<IColumn>();
        }        

        [SetUp]
        public void InitializeTest() {
            actionQueue = Components.Instance.GetComponent<ActionQueue>();
            actionQueue.Clear();
        }

        [Test]
        public void WhenActionIsValid_PushMustAddActionToQueue() {
            Action action = new ColumnRemoval(new ConnectionInfo(), new ColumnDescription());

            actionQueue.Push(action);

            Assert.AreEqual(1, actionQueue.Count);
        }

        [Test]
        public void WhenActionIsNotValid_PushMustThrowException() {
            Assert.Throws<InvalidActionException>(() => actionQueue.Push(null));
        }

        [Test]
        public void WhenQueueHasElements_PopMustRetunTheFirstElementIn() {
            Action action1 = new ColumnRemoval(new ConnectionInfo(), new ColumnDescription());
            Action action2 = new ColumnRemoval(new ConnectionInfo(), new ColumnDescription());
            actionQueue.Push(action1);
            actionQueue.Push(action2);

            Action action = actionQueue.Pop();

            Assert.IsTrue(ReferenceEquals(action1, action));
        }

        [Test]
        public void WhenQueueHasNoElements_PopMustRetunNull() {
            Action action = actionQueue.Pop();

            Assert.IsNull(action);
        }
    }
}