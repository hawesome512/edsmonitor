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
                byte[] UpdateDevice(byte zid,byte address,int zoneIndex);

                [OperationContract]
                byte[] RemoteControl(byte zid,byte[] snd);

                [OperationContract]
                Record[] QueryData(byte zid,byte address,DateTime start, DateTime end);

                [OperationContract]
                EDSLot.Trip[] QueryTrip(byte zid,byte address,DateTime start, DateTime end);

                [OperationContract]
                void ChangeDeviceLiveness(byte zid, byte address, int liveness);

                [OperationContract]
                EDSLot.Energy[] QueryEnergy(int[] addrs, DateTime start, DateTime end);

                [OperationContract]
                string GetUpsState();
        }
}
