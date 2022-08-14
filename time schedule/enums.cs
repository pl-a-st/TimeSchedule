using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule { 
    public enum BtnClick {
        ok,
        aplly,
        cancel,
        noClick
    }
    public enum IsChecked {
        yes,
        no,
        undefined
    }
    public enum BtnUpDowm {
        up,
        down
    }
}