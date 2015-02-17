using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultRemoval : ConstraintAction {
        private readonly DefaultDescription defaultDescription;

        public DefaultRemoval(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.defaultDescription = defaultDescription;
        }

        internal override void Execute() {
            Constraints.RemoveDefault(defaultDescription);
        }
    }
}