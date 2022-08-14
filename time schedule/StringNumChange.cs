using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_schedule {
    public class StringNumChange {
        public StringNumChange(string str, int num) {
            Str = str;
            Num = num;
        }
        public string Str {
            get;
            private set;
        } = string.Empty;
        public int Num {
            get;
            private set;
        }
        public bool HasChange {
            get;
            private set;
        } = false;

        public void SetStr(string str) {
            Str = str;
        }
    }
}
