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
                        return dv.DataList[zoneIndex];
                }
        }
}
