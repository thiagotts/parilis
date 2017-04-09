using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultRemoval : ConstraintAction {
        internal readonly DefaultDescription DefaultDescription;

        public DefaultRemoval(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.DefaultDescription = defaultDescription;
        }

        public override string Description =>
            $"Removing default value constraint {DefaultDescription.FullName} in table {DefaultDescription.TableName}.";

        internal override void Execute() {
            Constraints.RemoveDefault(DefaultDescription);
        }
    }
}