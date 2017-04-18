using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace Core {
    public class Components {
        private readonly IWindsorContainer container;

        public Components() : this(new WindsorContainer(new XmlInterpreter())) {}

        internal Components(IWindsorContainer container) {
            this.container = container;
        }

        private static Components instance;
        public static Components Instance {
            get {
                if (instance != null) return instance;

                lock (typeof (Components)) {
                    instance = new Components();
                }
                return instance;
            }
            internal set { instance = value; }
        }

        public static IKernel Kernel => Instance.container.Kernel;

        public virtual T GetComponent<T>() {
            return Instance.container.Kernel.Resolve<T>();
        }

        public virtual T GetComponent<T>(params object[] args) {
            return Instance.container.Kernel.Resolve<T>(new Arguments(args));
        }

        public static void Dispose() {
            if (instance == null) return;

            instance.container.Dispose();
            instance = null;
        }
    }
}