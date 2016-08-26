using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor
{
        public class EDSService:IEDSService
        {
                public byte[] UpdateDevice(byte address,int zoneIndex)
                {
                        Device dv = Common.SelDevices.Find(d => d.Address == address);
                        if (dv == null)
                                return null;
                        return dv.DataList[zoneIndex];
                }

                public byte[] RemoteControl(byte[] snd)
                {
                        Device dv = Common.SelDevices.Find(d => d.Address == snd[0]);
                        if (dv == null)
                                return null;
                        return dv.Remote(snd);
                }

                public Record[] QueryData(byte address, DateTime start, DateTime end)
                {
                        Device dv = Common.SelDevices.Find(d => d.Address == address);
                        if (dv == null)
                                return null;
                        return dv.QueryData(start, end).ToArray();
                }

                public EDSLot.Trip[] QueryTrip(byte address, DateTime start, DateTime end)
                {
                        Device dv = Common.SelDevices.Find(d => d.Address == address);
                        if (dv == null)
                                return null;
                        return dv.QueryTrip(start, end).ToArray();
                }


                public void ChangeSelAddress(byte address)
                {
                        Common.SelectedAddress = address;
                }

                public void ChangeSelZone(byte zid)
                {
                        if (zid == 0)
                        {
                                Common.OrdDevices.AddRange(Common.SelDevices);
                                Common.SelDevices.Clear();
                        }
                        else
                        {
                                Common.SelDevices = Common.OrdDevices.FindAll(d => d.ZID == zid);
                                Common.OrdDevices.RemoveAll(d => d.ZID == zid);
                        }
                }

                public EDSLot.Energy[] QueryEnergy(int[] addrs, DateTime start, DateTime end)
                {
                        return DataLib.QueryEnergy(addrs, start, end).ToArray();
                }

                public string GetUpsState()
                {
                        return Power.UpsState;
                }
        }
}
