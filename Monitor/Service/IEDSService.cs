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

                [OperationContract]
                byte[] RemoteControl(byte[] snd);

                [OperationContract]
                Record[] QueryData(byte address,DateTime start, DateTime end);

                [OperationContract]
                EDSLot.Trip[] QueryTrip(byte address,DateTime start, DateTime end);

                [OperationContract]
                void ChangeSelAddress(byte address);

                [OperationContract]
                void ChangeSelZone(byte zid);

                [OperationContract]
                EDSLot.Energy[] QueryEnergy(int[] addrs, DateTime start, DateTime end);

                [OperationContract]
                string GetUpsState();
        }
}
