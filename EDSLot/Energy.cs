//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDSLot
{
        using System;
        using System.Collections.Generic;
        using System.ServiceModel;
        using System.Runtime.Serialization;
        
        [DataContract]
        public partial class Energy
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
                public Nullable<double> PE
                {
                        get;
                        set;
                }
        }
}
