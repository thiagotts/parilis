﻿using Core.Interfaces;

namespace Core.Actions {
    internal abstract class ConstraintAction : Action {
        internal readonly IConstraint Constraints;

        protected ConstraintAction(ConnectionInfo connectionInfo) : base(connectionInfo) {
            Constraints = Components.Instance.GetComponent<IConstraint>(connectionInfo);
        }
    }
}