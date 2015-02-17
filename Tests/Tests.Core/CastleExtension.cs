using System;
using Castle.MicroKernel.Registration;

namespace Tests.Core {
    internal static class CastleExtension {
        public static ComponentRegistration<T> OverridesExistingRegistration<T>(this ComponentRegistration<T> componentRegistration) where T : class {
            return componentRegistration
                .Named(Guid.NewGuid().ToString())
                .IsDefault();
        }
    }
}