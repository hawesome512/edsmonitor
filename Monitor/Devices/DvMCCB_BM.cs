using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDSLot;

namespace Monitor
{
        public class DvMCCB_BM:Device
        {
                public override void updateState()
                {
                        DValues dv = RealData["CircuitCheck"];
                        DState state = new DState();
                        int data;
                        if (int.TryParse(dv.ShowValue, out data))
                        {
                                byte data1 = (Byte)(data % 256);
                                if ((data1>> 6 & 1) == 1)
                                        state.SwitchState = SwitchStatus.Close;
                                else
                                        state.SwitchState = SwitchStatus.Open;
                                int nCircuit = data & 0x0087;
                                if (nCircuit > 0)
                                        state.RunState = Run.Alarm;
                                else
                                        state.RunState = Run.Normal;
                                string[] items = dv.Tag.Split('_');
                                for (int i = 0; i < items.Length; i++)
                                {
                                        if (Tool.isOne(nCircuit, i))
                                        {
                                                state.ErrorMsg += items[i] + " ";
                                        }
                                }

                                if (Params["LocalOrRemote"].ShowValue == "Local")
                                        state.ControlState = ControlMode.Local;
                                else
                                        state.ControlState = ControlMode.Remote;
                        }

                        //注1111
                        state.Ia = str2int("Ia");
                        state.Ib = str2int("Ib");
                        state.Ic = str2int("Ic");

                        State = state;

                        //BM通信地址表描述太模糊，暂不处理数据存储和报警
                        //if (Common.IsServer)
                        //{
                        //        SaveData();
                        //        //产生新的脱扣记录
                        //        if (Tool.isOne(data, 12))
                        //        {
                        //                _tripData = getZoneData(_tripData);
                        //                SaveTrip();
                        //                SmsAlarm();
                        //        }
                        //}
                }

                double str2int(string name)
                {
                        double data;
                        double.TryParse(RealData[name].ShowValue, out data);
                        Random r = new Random();
                        return data + r.Next(1, 100) / 1000.0;
                }

                protected override Dictionary<string, DValues> cvtBasic()
                {
                        List<string> keys = _basicData.Keys.ToList();
                        Dictionary<string, DValues> dic = new Dictionary<string, DValues>();
                        for (int i = 0; i < keys.Count; i++)
                        {
                                var tmp = Tool.CloneDValues(_basicData[keys[i]]);
                                switch (i)
                                {
                                        case 2:
                                                string value1 = _basicData[keys[i + 1]].ShowValue;
                                                tmp.ShowValue = string.Format("{0} {1}", tmp.ShowValue, value1);
                                                dic.Add("SN", tmp);
                                                i++;
                                                break;
                                        case 4:
                                                string value2 = _basicData[keys[i + 1]].ShowValue;
                                                tmp.ShowValue = string.Format("{0} {1}", tmp.ShowValue, value2);
                                                dic.Add("PD", tmp);
                                                i++;
                                                break;
                                        default:
                                                dic.Add(keys[i], tmp);
                                                break;
                                }

                        }

                        //******临时方案：框架等级=额定电流.**********//
                        //******地址0007不能反应正确的框架等级******//
                        //dic["Inm"] = dic["In"];

                        return dic;
                }

                protected override Dictionary<string, DValues> cvtParams()
                {
                        Dictionary<string, DValues> tmps = new Dictionary<string, DValues>();
                        int nIn,nSwH;
                        bool bIn = int.TryParse(BasicData["In"].ShowValue, out nIn);//片区0
                        bool bSw = int.TryParse(_params["ProtectSwitchH"].ShowValue, out nSwH);//片区2
                        if(bIn&&bSw)
                        {
                                foreach (KeyValuePair<string, DValues> p in _params)
                                {
                                        DValues d = Tool.CloneDValues(p.Value);
                                        //******临时方案：框架等级250以下没有短路功能.**********//
                                        switch (p.Key)
                                        {
                                                case "ProtectSwitchH":
                                                        string[] items = d.Tag.Split('_');
                                                        if (nIn <= 250)
                                                        {
                                                                nSwH = Tool.BitSet(nSwH, 0, 0);
                                                                items[0] = "短路/Off/Off";
                                                        }
                                                        items[1] = "瞬动/On/On";
                                                        nSwH = Tool.BitSet(nSwH, 1, 1);//瞬动不能关闭，但上位机可以修改铁电
                                                        d.ShowValue = (nSwH).ToString();
                                                        d.Tag = string.Join("_", items);
                                                        break;
                                                case "Isd":
                                                case "Tsd":
                                                        if (nIn <= 250)
                                                        {
                                                                d.ShowValue = "---";
                                                                d.Tag = "1_---";
                                                        }
                                                        break;
                                                default:
                                                        break;
                                        }
                                        tmps.Add(p.Key, d);
                                }
                        }
                        else
                        {
                                tmps = _params;
                        }
                        return tmps;
                }

                protected override byte[] GetValidParams(List<string> dataList)
                {
                        dataList.Insert(1, "0");

                        //备注：待优化——框架等级250以下没有短路功能
                        int nIn;
                        bool bIn =int.TryParse(BasicData["In"].ShowValue, out nIn);//片区0
                        if (bIn && nIn <= 250)
                        {
                                dataList[4] = dataList[5] = "0";
                        }

                        byte len = (byte)dataList.Count;
                        byte[] snd = new byte[len * 2];

                        ComConverter cvt = new ComConverter();
                        List<string> keys = new List<string>(_params.Keys);
                        keys.RemoveRange(0, 2);
                        for (int i = 0; i < len; i++)
                        {
                                DValues d=_params[keys[i]];
                                var temp = cvt.CvtWrite(dataList[i], d.Cvt, d.Tag);
                                temp.ToList().CopyTo(0, snd, i * 2, 2);
                        }
                        return snd;
                }

                protected override byte[] GetValidRemote(string p)
                {
                        byte[] snd = { Address, 16, 0x50, 0x00, 00, 01, 2, 0x00, 0x00 };
                        if (p == "Close")
                                snd[8] = 0x40;
                        return snd;
                }

                private string getTag(string tag, int index,int length)
                {
                        List<string> tags = new List<string>(tag.Split('_'));
                        tags=tags.Where((value, id) =>id==0||(id>= index && id < index + length)).ToList();
                        return string.Join("_", tags.ToArray());
                }


                #region Data
                protected override void SaveData()
                {
                        using (EDSLot.EDSEntities context = new EDSLot.EDSEntities())
                        {
                                try
                                {
                                        context.Record_MCCB.Add(new EDSLot.Record_MCCB()
                                        {
                                                ZID = this.ZID,
                                                Address = this.Address,
                                                Time = DateTime.Now,
                                                Ia = State.Ia,
                                                Ib = State.Ib,
                                                Ic = State.Ic,
                                                IN = str2int("IN")
                                        });
                                        context.SaveChanges();
                                }
                                catch
                                {
                                }
                        }
                }

                public override List<Record> QueryData(DateTime start, DateTime end)
                {
                        if (!Common.IsServer)
                                return MyCom.QueryData(Address, start, end);
                        using (EDSEntities context = new EDSEntities())
                        {
                                var result = from m in context.Record_MCCB
                                             where m.ZID==this.ZID&& m.Address == this.Address && (m.Time >= start && m.Time <= end)
                                             select m;
                                List<Record> records = new List<Record>();
                                foreach (var r in result)
                                {
                                        records.Add(new Record(r));
                                }
                                return records;
                        }
                }
                #endregion
        }
}
