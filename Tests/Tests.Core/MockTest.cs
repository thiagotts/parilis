using Castle.MicroKernel.Registration;
using Core;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class MockTest {
        protected ConnectionInfo ConnectionInfo;

        [TestFixtureSetUp]
        public virtual void InitializeClass() {
            Components.Dispose();
            Mock<IDatabase>();
            ConnectionInfo = new ConnectionInfo();
        }

        protected T Mock<T>(params object[] args) where T : class {
            var instante = Substitute.For<T>(args);
            Mock<T, T>(instante);
            return instante;
        }

        private void Mock<T, I>(I instante)
            where I : T
            where T : class {
            var registration = Component.For<T>().Instance(instante).OverridesExistingRegistration();
            Components.Kernel.Register(registration);
        }
    }
}