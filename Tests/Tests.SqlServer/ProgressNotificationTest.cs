using Castle.MicroKernel.Registration;
using Core;
using Core.Actions;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.SqlServer {
    public class ProgressNotificationTest {

        public class Receiver {
            public virtual void Receive(double progress, string message) {
                
            }
        }

        private T Mock<T>() where T : class{
            var mock = Substitute.For<T>();
            Components.Kernel.Register(Component.For<T>()
                                                .Instance(mock)
                                                .Named(typeof(T).FullName)
                                                .LifestyleTransient()
                                                .IsDefault());
            return mock;
        }

        [Test]
        public void ShouldNotifyProgressWhenActionsAreExecuted() {
            Mock<IDatabase>();
            Mock<IColumn>();
            Mock<IConstraint>();
            
            var connectionInfo1 = new ConnectionInfo();
            var connectionInfo2 = new ConnectionInfo();

            var actualDescription = Substitute.For<DatabaseDescription>(connectionInfo1);
            var referenceDescription = Substitute.For<DatabaseDescription>(connectionInfo2);
            var actionIdentifier = Substitute.For<ActionIdentifier>(actualDescription, referenceDescription);

            var actionQueue = new ActionQueue();

            var columnCreation1 = new ColumnCreation(connectionInfo1, new ColumnDescription { TableName = "Table A", Schema = "schema A", Name = "Col A" });
            var columnCreation2 = new ColumnCreation(connectionInfo2, new ColumnDescription { TableName = "Table B", Schema = "schema B", Name = "Col B" });

            actionQueue.Push(columnCreation1);
            actionQueue.Push(columnCreation2);

            actionIdentifier.GetActions().Returns(actionQueue);

            var receiver = Substitute.For<Receiver>();

            var parilis = new Parilis(actualDescription, referenceDescription, actionIdentifier);
            parilis.OnProgress += (progress, msg) => {
                receiver.Receive(progress, message:msg);
            };
            parilis.Run();

            receiver.Received(2).Receive(Arg.Any<double>(), Arg.Any<string>());

        }
    }
}