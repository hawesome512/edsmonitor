using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor
{
        //待更改：防止恶意注入
        public class EDSService:IEDSService
        {
                public byte[] UpdateDevice(byte zid,byte address,int zoneIndex)
                {
                        Device device = Common.ZoneDevices[zid].Find(d => d.Address == address);
                        if (device == null)
                                return null;
                        return device.DataList[zoneIndex];
                }

                public byte[] RemoteControl(byte zid,byte[] snd)
                {
                        Device device = Common.ZoneDevices[zid].Find(d => d.Address == snd[0]);
                        if (device == null)
                                return null;
                        return device.Remote(snd);
                }

                public byte[] RemoteControlJson(byte zid, string strSnd)
                {
                        byte[] snd= strSnd.Split('_').ToList().ConvertAll(s => Convert.ToByte(s, 16)).ToArray();
                        return RemoteControl(zid, snd);
                }

                public Record[] QueryData(byte zid,byte address, DateTime start, DateTime end)
                {
                        Device device = Common.ZoneDevices[zid].Find(d => d.Address == address);
                        if (device == null)
                                return null;
                        return device.QueryData(start, end).ToArray();
                }

                public Record[] QueryDataJson(byte zid, byte address, long start, long end)
                {
                        DateTime dtStart = new DateTime(start);
                        DateTime dtEnd = new DateTime(end);
                        return QueryData(zid,address, dtStart, dtEnd);
                }

                public EDSLot.Trip[] QueryTrip(byte zid,byte address, DateTime start, DateTime end)
                {
                        Device device = Common.ZoneDevices[zid].Find(d => d.Address == address);
                        if (device == null)
                                return null;
                        return device.QueryTrip(start, end).ToArray();
                }

                public EDSLot.Trip[] QueryTripJson(byte zid, byte address, long start, long end)
                {
                        DateTime dtStart = new DateTime(start);
                        DateTime dtEnd = new DateTime(end);
                        return QueryTrip(zid, address, dtStart, dtEnd);
                }

                public void ChangeDeviceLiveness(byte zid, byte address, int liveness)
                {
                        Device device= Common.ZoneDevices[zid].Find(d => d.Address == address);
                        if (device != null)
                        {
                                //        device.NeedParamsNum += liveness;
                                device.NeedParamsNum = liveness;
                        }
                }

                public EDSLot.Energy[] QueryEnergy(byte[] addrs,DateTime start, DateTime end)
                {
                        return DataLib.QueryEnergy(addrs, start, end).ToArray();
                }

                public string GetUpsState()
                {
                        return Power.UpsState;
                }
        }
}
