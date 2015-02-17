using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultCreation : ConstraintAction {
        private readonly DefaultDescription defaultDescription;

        public DefaultCreation(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.defaultDescription = defaultDescription;
        }

        internal override void Execute() {
            Constraints.CreateDefault(defaultDescription);
        }
    }
}