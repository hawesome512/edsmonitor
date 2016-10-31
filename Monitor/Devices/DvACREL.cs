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
                        NeedParamsNum = 0;//电能表无参数
                }

                public override void GetData()
                {
                        if (nComFailed < 5)
                        {
                                if (MyCom.GetComType() == ComType.SL)
                                {
                                        simulateState();
                                        return;
                                }
                                _realData = getZoneData(_realData,1);
                                updateState();
                        }
                }

                protected override void simulateState(string command = null)
                {
                        DState state = new DState();
                        Device parent = Common.ZoneDevices[ZID].Find(d => d.Address == ParentAddr);
                        Random random = new Random();
                        state.Ia = parent.State.Ia;
                        state.Ib = parent.State.Ib;
                        state.Ic = parent.State.Ic;
                        state.Ua = 220 + random.Next(-3, 3);
                        state.Ub = 220 + random.Next(-3, 3);
                        state.Uc = 220 + random.Next(-3, 3);
                        state.PF = random.Next(90, 95) / 100f;
                        state.P = state.Ia * state.Ua * state.PF / 1000;
                        state.Q = state.Ia * state.Ua * (1-state.PF) / 1000;
                        state.FR = 50;
                        state.PE +=State.PE+ random.Next(50,150)/1000f;
                        State = state;
                }

                public override void updateState()
                {
                        DState state = new DState();
                        state.ControlState = ControlMode.Local;
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
                                state.RunState = Run.Normal;
                        }
                        else
                        {
                                state.RunState = Run.NonSignal;
                        }
                        State = state;
                        if (Common.IsServer)
                        {
                                SaveData();
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

                protected override void SaveData()
                {
                        using (EDSLot.EDSEntities context = new EDSLot.EDSEntities())
                        {
                                try
                                {
                                        context.Record_Measure.Add(new EDSLot.Record_Measure()
                                        {
                                                ZID=this.ZID,
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
