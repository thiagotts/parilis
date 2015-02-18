using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultRemoval : ConstraintAction {
        internal readonly DefaultDescription DefaultDescription;

        public DefaultRemoval(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.DefaultDescription = defaultDescription;
        }

        public override string Description {
            get {
                return string.Format("Removing default value constraint {0} in table {1}.",
                    DefaultDescription.FullName, DefaultDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.RemoveDefault(DefaultDescription);
        }
    }
}