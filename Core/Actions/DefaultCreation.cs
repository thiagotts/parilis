using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultCreation : ConstraintAction {
        internal readonly DefaultDescription DefaultDescription;

        public DefaultCreation(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.DefaultDescription = defaultDescription;
        }

        internal override void Execute() {
            Constraints.CreateDefault(DefaultDescription);
        }
    }
}