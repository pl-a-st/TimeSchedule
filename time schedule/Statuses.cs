using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace time_schedule {
    public enum MethodResultStatus {
        Ok,
        Fault
    }
    [Serializable]
    public static class Statuses {
         public enum WorkWithProject {
            ProgramStarted,
            LoadProject,
            NewProject
        }
        public enum LoadingStatus {
            Loaded,
            ReadyToLoad,
            NotReady
        }
        public enum FormReadyToBeAddedControl {
            Ready,
            NotReady
        }
        public enum Visibility {
            Visible,
            Invisible
        }
        public enum ProgressBar {
            Use,
            DoNotUse
        }
        public enum FormLoad {
            Load,
            UnLoad
        }
        public enum ProgramWindowState {
            Normal,
            Minimized
        }
    }
}
