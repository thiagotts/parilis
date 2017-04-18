using System;
using System.Collections.Generic;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {

    public delegate void ParilisProgressNotifier(double percentualProgress, string message);

    public class Parilis : IDisposable {
        private ActionIdentifier actionIdentifier;
        private Logger logger;
        public event ParilisProgressNotifier OnProgress;

        public Parilis(DatabaseDescription actualDatabase, DatabaseDescription referenceDatabase, ActionIdentifier actionIdentifier = null) {
            this.actionIdentifier = actionIdentifier ?? new ActionIdentifier(actualDatabase, referenceDatabase);
            logger = new Logger();
        }

        public ParilisResult Run() {
            logger.Info("Parilis has started. Getting list of actions...");
            var actionQueue = actionIdentifier.GetActions();
            logger.Info($"{actionQueue.Count} actions were initially identified.");

            if (actionQueue.Count <= 0) {
                logger.Info("Parilis has finished with no pending actions.");
                return new ParilisResult {FinishedSuccessfully = true};
            }

            var parilisResult = ExecuteActions(actionQueue);

            logger.Info(parilisResult.FinishedSuccessfully ?
                "Parilis has finished successfully with no pending actions." :
                "Parilis has finished with errors.");

            return parilisResult;
        }

        public IList<Action> GetActions() {
            var actionQueue = actionIdentifier.GetActions();
            return actionQueue.Queue.ToList();
        }

        private ParilisResult ExecuteActions(ActionQueue actionQueue) {
            try {
                var actionCount = 0;
                Action action;
                
                while ((action = actionQueue.Pop()) != null) {
                    logger.Info($"Action {++actionCount} of {actionQueue.TotalCount}: {action.Description}");
                    action.Execute();
                        
                    Progress(Math.Round(actionCount/(double)actionQueue.TotalCount*100, 2), action.Description);
                }
            }
            catch (ParilisException ex) {
                logger.Error("Error while executing action.", ex);
                return new ParilisResult {
                    FinishedSuccessfully = false,
                    Exception = ex
                };
            }
            catch (Exception ex) {
                logger.Error("Unexpected error while executing action.", ex);
                return new ParilisResult {
                    FinishedSuccessfully = false,
                    Exception = ex
                };
            }

            return new ParilisResult {FinishedSuccessfully = true};
        }

        public bool AreAlreadyEqual() {
            return actionIdentifier.GetActions().Count == 0;
        }

        protected virtual void Progress(double percentualprogress, string message) {
            OnProgress?.Invoke(percentualprogress, message);
        }

        public void Dispose() {
            OnProgress = null;
            actionIdentifier = null;
            logger = null;
        }
    }
}