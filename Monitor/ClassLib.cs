using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

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

                public SwitchStatus SwitchState;

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
                                return Math.Round(_Ia, 1);
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
                                return Math.Round(_Ib, 1);
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
                                return Math.Round(_Ic,1);
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
                                return Math.Round(_Ua, 1);
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
                                return Math.Round(_Ub, 1);
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
                                return Math.Round(_Uc, 1);
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
        public enum SwitchStatus
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
                ACB_1,
                ACB_2,
                MCCB_BM,
                MCCB_BMA,
                ATS,
                MIC,
                ACREL
        };

        [DataContract]
        public class Record
        {
                [DataMember]
                public System.DateTime Time
                {
                        get;
                        set;
                }
                [DataMember]
                public int Address
                {
                        get;
                        set;
                }
                [DataMember]
                public Nullable<double> Ia
                {
                        get;
                        set;
                }
                [DataMember]
                public Nullable<double> Ib
                {
                        get;
                        set;
                }
                [DataMember]
                public Nullable<double> Ic
                {
                        get;
                        set;
                }
                [DataMember]
                public Nullable<double> IN
                {
                        get;
                        set;
                }
                [DataMember]
                public Nullable<double> Igf
                {
                        get;
                        set;
                }

                public Record(EDSLot.Record record)
                {
                        Time = record.Time;
                        Address = record.Address;
                        Ia = record.Ia;
                        Ib = record.Ib;
                        Ic = record.Ic;
                        IN = record.IN;
                        Igf = record.Igf;
                }

                public Record(Monitor.ServiceReference1.Record record)
                {
                        Time = record.Time;
                        Address = record.Address;
                        Ia = record.Ia;
                        Ib = record.Ib;
                        Ic = record.Ic;
                        IN = record.IN;
                        Igf = record.Igf;
                }
        }

}
