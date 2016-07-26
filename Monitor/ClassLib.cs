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
                                return Math.Round(_Ia,0);
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
                                return Math.Round(_Ib, 0);
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
                                return Math.Round(_Ic, 0);
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
                                return Math.Round(_Ua,0);
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
                                return Math.Round(_Ub, 0);
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
                                return Math.Round(_Uc, 0);
                        }
                        set
                        {
                                _Uc = value;
                        }
                }
                double _PE;
                
                public double PE
                {
                        get
                        {
                                return Math.Round(_PE, 3);
                        }
                        set
                        {
                                _PE = value;
                        }
                }
                double _QE;
                
                public double QE
                {
                        get
                        {
                                return Math.Round(_QE, 3);
                        }
                        set
                        {
                                _QE = value;
                        }
                }
                double _P;
                
                public double P
                {
                        get
                        {
                                return Math.Round(_P, 3);
                        }
                        set
                        {
                                _P = value;
                        }
                }
                double _Q;
                
                public double Q
                {
                        get
                        {
                                return Math.Round(_Q, 3);
                        }
                        set
                        {
                                _Q = value;
                        }
                }
                double _FR;
                
                public double FR
                {
                        get
                        {
                                return Math.Round(_FR, 3);
                        }
                        set
                        {
                                _FR = value;
                        }
                }
                double _PF;
                
                public double PF
                {
                        get
                        {
                                return Math.Round(_PF, 3);
                        }
                        set
                        {
                                _PF = value;
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
                
                ATS_N,//ATS
                
                ATS_R,
                
                Run,//MIC
                
                Wait,
                
                Ready
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
                
                MIC,
                
                ACREL
        };
}
