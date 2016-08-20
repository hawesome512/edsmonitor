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
                        Device dv = Common.Devices.Find(d => d.Address == address);
                        if (dv == null)
                                return null;
                        return dv.DataList[zoneIndex];
                }

                public byte[] RemoteControl(byte[] snd)
                {
                        Device dv = Common.Devices.Find(d => d.Address == snd[0]);
                        if (dv == null)
                                return null;
                        return dv.Remote(snd);
                }

                public Record[] QueryData(byte address, DateTime start, DateTime end)
                {
                        Device dv = Common.Devices.Find(d => d.Address == address);
                        if (dv == null)
                                return null;
                        return dv.QueryData(start, end).ToArray();
                }

                public EDSLot.Trip[] QueryTrip(byte address, DateTime start, DateTime end)
                {
                        Device dv = Common.Devices.Find(d => d.Address == address);
                        if (dv == null)
                                return null;
                        return dv.QueryTrip(start, end).ToArray();
                }


                public void ChangeSelectedAddress(byte address)
                {
                        Common.SelectedAddress = address;
                }


                public EDSLot.Energy[] QueryEnergy(int[] addrs, DateTime start, DateTime end)
                {
                        return DataLib.QueryEnergy(addrs, start, end).ToArray();
                }
        }
}
