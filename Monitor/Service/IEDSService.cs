using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace Monitor
{
        [ServiceContract]
        public interface IEDSService
        {
                [OperationContract]
                byte[] UpdateDevice(byte address,int zoneIndex);
        }
}
