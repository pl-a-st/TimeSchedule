using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_schedule {
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
    }
}
