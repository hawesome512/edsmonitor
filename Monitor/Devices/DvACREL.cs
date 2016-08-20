using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EDSLot;

namespace Monitor
{
        public class DvACREL : Device
        {

                public override void InitAddress()
                {
                        string path = string.Format("Config\\{0}.xml", DvType);
                        XmlElement xeRoot = Tool.GetXML(path);
                        _realData = initDic(0, xeRoot);
                        DataList.Add(null);
                        DataList.Add(new byte[_realData.Count * 2]);
                        DataList.Add(null);
                }

                public override void GetData()
                {
                        if (nComFailed < 5)
                        {
                                _realData = getZoneData(_realData,1);
                                updateState();
                        }
                }

                public override void updateState()
                {
                        DState state = new DState();
                        if (!string.IsNullOrEmpty(RealData["U"].ShowValue))
                        {
                                state.Ua = str2double("U");
                                state.Ia = str2double("I");
                                state.FR = str2double("F");
                                state.PF = str2double("PF");
                                state.P = str2double("P");
                                state.Q = str2double("Q");
                                state.PE = str2doubleE("PE");
                                state.QE = str2doubleE("QE");
                        }
                        State = state;
                        if (Common.IsSaveData)
                        {
                                SaveDate();
                        }
                }

                double str2double(string name)
                {
                        int data1 = int.Parse(RealData[name].ShowValue);
                        int data2 = int.Parse(RealData[name + "_1"].ShowValue);
                        return data1 * Math.Pow(10, data2 - 3);
                }
                double str2doubleE(string name)
                {
                        int data1 = int.Parse(RealData[name].ShowValue);
                        int data2 = int.Parse(RealData[name + "_1"].ShowValue);
                        return (data1 * 65536 + data2) / 1000f;
                }

                protected override byte[] GetValidParams(List<string> dataList)
                {
                        throw new NotImplementedException();
                }

                protected override byte[] GetValidRemote(string p)
                {
                        throw new NotImplementedException();
                }

                protected override void SaveDate()
                {
                        using (EDSLot.EDSEntities context = new EDSLot.EDSEntities())
                        {
                                try
                                {
                                        context.Record_Measure.Add(new EDSLot.Record_Measure()
                                        {
                                                Address = this.Address,
                                                Time = DateTime.Now,
                                                U = State.Ua,
                                                I = State.Ia,
                                                F = State.FR,
                                                PF = State.PF,
                                                P = State.P,
                                                Q = State.Q,
                                                PE = State.PE,
                                                QE = State.QE
                                        });
                                        context.SaveChanges();
                                }
                                catch
                                {
                                }
                        }
                }
        }
}
