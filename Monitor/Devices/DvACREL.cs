using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Monitor
{
        public class DvACREL : Device
        {

                public override void InitAddress()
                {
                        string path = string.Format("DevicesConfig\\{0}.xml", DvType);
                        XmlElement xeRoot = Tool.GetXML(path);
                        _realData = initDic(0, xeRoot);
                }

                public override void GetData()
                {
                        if (nComFailed < 5)
                        {
                                _realData = getZoneData(_realData);
                                updateState();
                        }
                }

                public override void updateState()
                {
                        DState state = new DState();
                        if (!string.IsNullOrEmpty(RealData["U"].ShowValue))
                        {
                                int data1 = int.Parse(RealData["U"].ShowValue);
                                int data2 = int.Parse(RealData["U_1"].ShowValue);
                                state.Ua = data1 * Math.Pow(10, data2 - 3);

                                data1 = int.Parse(RealData["I"].ShowValue);
                                data2 = int.Parse(RealData["I_1"].ShowValue);
                                state.Ia = data1 * Math.Pow(10, data2 - 3);

                                data1 = int.Parse(RealData["PE"].ShowValue);
                                data2 = int.Parse(RealData["PE_1"].ShowValue);
                                state.Ep = (data1 * 65536 + data2) / 1000f;
                        }
                        State = state;
                }

                protected override byte[] GetValidParams(List<string> dataList)
                {
                        throw new NotImplementedException();
                }

                protected override byte[] GetValidRemote(string p)
                {
                        throw new NotImplementedException();
                }
        }
}
