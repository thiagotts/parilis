using System;
using System.Collections.Generic;
using Core.Descriptions;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {
    public class Parilis {
        private DatabaseDescription actualDatabase;
        private DatabaseDescription referenceDatabase;

        public Parilis(DatabaseDescription actualDatabase, DatabaseDescription referenceDatabase) {
            this.actualDatabase = actualDatabase;
            this.referenceDatabase = referenceDatabase;
        }

        public bool Run() {
            var actionIdentifier = new ActionIdentifier(actualDatabase, referenceDatabase);
            IList<Action> actions = actionIdentifier.GetActions();

            try {
                foreach (var action in actions) {
                    action.Execute();
                }
            }
            catch (ParilisException) {
                return false;
            }
            catch (Exception) {
                return false;
            }

            actualDatabase = new DatabaseDescription(actualDatabase.ConnectionInfo);
            referenceDatabase = new DatabaseDescription(referenceDatabase.ConnectionInfo);
            
            actions = actionIdentifier.GetActions();
            return actions.Count == 0;
        }
    }
}