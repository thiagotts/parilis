using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultRemoval : ConstraintAction {
        internal readonly DefaultDescription DefaultDescription;

        public DefaultRemoval(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.DefaultDescription = defaultDescription;
        }

        internal override void Execute() {
            Constraints.RemoveDefault(DefaultDescription);
        }
    }
}