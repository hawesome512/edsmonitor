using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSLot
{
        public abstract class Record
        {
                public System.DateTime Time
                {
                        get;
                        set;
                }
                public int Address
                {
                        get;
                        set;
                }
                public Nullable<double> Ia
                {
                        get;
                        set;
                }
                public Nullable<double> Ib
                {
                        get;
                        set;
                }
                public Nullable<double> Ic
                {
                        get;
                        set;
                }
                public Nullable<double> IN
                {
                        get;
                        set;
                }
                public Nullable<double> Igf
                {
                        get;
                        set;
                }
        }
}
