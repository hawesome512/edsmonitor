using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Monitor
{
        [ServiceContract]
        public interface IEDSService
        {
                [OperationContract]
                [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateDevice?zid={zid}&address={address}&zoneIndex={zoneIndex}", BodyStyle = WebMessageBodyStyle.Bare)]
                byte[] UpdateDevice(byte zid,byte address,int zoneIndex);

                [OperationContract]
                [WebGet(BodyStyle = WebMessageBodyStyle.Wrapped)]
                byte[] RemoteControl(byte zid,byte[] snd);

                [OperationContract]
                [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "RemoteControlJson?zid={zid}&strSnd={strSnd}", BodyStyle = WebMessageBodyStyle.Bare)]
                byte[] RemoteControlJson(byte zid, string strSnd);

                [OperationContract]
                [WebGet(BodyStyle = WebMessageBodyStyle.Wrapped)]
                Record[] QueryData(byte zid,byte address,DateTime start, DateTime end);

                [OperationContract]
                [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "QueryDataJson?zid={zid}&address={address}&start={start}&end={end}", BodyStyle = WebMessageBodyStyle.Bare)]
                Record[] QueryDataJson(byte zid, byte address, long start, long end);

                [OperationContract]
                [WebGet(BodyStyle = WebMessageBodyStyle.Wrapped)]
                EDSLot.Trip[] QueryTrip(byte zid, byte address, DateTime start, DateTime end);

                [OperationContract]
                [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "QueryTripJson?zid={zid}&address={address}&start={start}&end={end}", BodyStyle = WebMessageBodyStyle.Bare)]
                EDSLot.Trip[] QueryTripJson(byte zid, byte address, long start, long end);

                [OperationContract]
                [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ChangeDeviceLiveness?zid={zid}&address={address}&liveness={liveness}", BodyStyle = WebMessageBodyStyle.Bare)]
                void ChangeDeviceLiveness(byte zid, byte address, int liveness);

                [OperationContract]
                [WebGet(BodyStyle = WebMessageBodyStyle.Wrapped)]
                EDSLot.Energy[] QueryEnergy(byte[] addrs,DateTime start, DateTime end);

                [OperationContract]
                [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetUpsState", BodyStyle = WebMessageBodyStyle.Bare)]
                string GetUpsState();
        }
}
