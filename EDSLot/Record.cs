using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace EDSLot
{
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
        }
}
