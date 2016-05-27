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
                double _Ia;
                public double Ia
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Ia.ToString()), 1);
                        }
                        set
                        {
                                _Ia = value;
                        }
                }
                double _Ib;
                public double Ib
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Ib.ToString()), 1);
                        }
                        set
                        {
                                _Ib = value;
                        }
                }
                double _Ic;
                public double Ic
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Ic.ToString()), 1);
                        }
                        set
                        {
                                _Ic = value;
                        }
                }
                double _Ua;
                public double Ua
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Ua.ToString()), 1);
                        }
                        set
                        {
                                _Ua = value;
                        }
                }
                double _Ub;
                public double Ub
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Ub.ToString()), 1);
                        }
                        set
                        {
                                _Ub = value;
                        }
                }
                double _Uc;
                public double Uc
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Uc.ToString()), 1);
                        }
                        set
                        {
                                _Uc = value;
                        }
                }
                double _Ep;
                public double Ep
                {
                        get
                        {
                                return (double)decimal.Round(decimal.Parse(_Ep.ToString()),3);
                        }
                        set
                        {
                                _Ep = value;
                        }
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
                ATS,
                ACREL
        };
}
