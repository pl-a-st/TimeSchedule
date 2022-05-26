using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule { 
    public enum FormBtnClick {
        Ok,
        Cancel,
        NoClick
    }
    public enum IsChecked {
        Yes,
        No,
        Undefined
    }
}