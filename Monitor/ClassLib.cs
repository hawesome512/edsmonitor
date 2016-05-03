using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Monitor
{
        public class DValues : INotifyPropertyChanged
        {
                public event PropertyChangedEventHandler PropertyChanged;
                public int Addr;
                private string str;
                public string ShowValue
                {
                        get
                        {
                                return str;
                        }
                        set
                        {
                                str = value;
                                if (PropertyChanged != null)
                                {
                                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ShowValue"));
                                }
                        }
                }
                public int Cvt;
                public string Tag;
                public string Unit;
                public string Alias;
        }
        public struct DState
        {
                public Run RunState;
                public Switch SwitchState;
                public ControlMode ControlState;
                public string ErrorMsg
                {
                        get;
                        set;
                }
                public double Ia
                {
                        get;
                        set;
                }
                public double Ib
                {
                        get;
                        set;
                }
                public double Ic
                {
                        get;
                        set;
                }
                public double Ua
                {
                        get;
                        set;
                }
                public double Ub
                {
                        get;
                        set;
                }
                public double Uc
                {
                        get;
                        set;
                }

        }
        public enum Run
        {
                NonSignal,
                Normal,
                Alarm
        }
        public enum Switch
        {
                Unknown,
                Close,
                Open,
                ATS_N,
                ATS_R
        }
        public enum ControlMode
        {
                Unknown,
                Local,
                Remote
        }
        public enum DeviceType
        {
                ACB,
                MCCB,
                ATS
        };
}
