using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule {
    public class Tree {
        public Tree() {
            FmProjectTree = new fmProjectTree();
        }
        public fmProjectTree FmProjectTree {
            get; private set;
        }

    }
}
