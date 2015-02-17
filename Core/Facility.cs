using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;

namespace Core {
    public class Facility : AbstractFacility {
        protected override void Init() {
            Kernel.Register(Types.FromThisAssembly()
                .Pick()
                .If(Component.IsCastleComponent));
        }
    }
}