using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDSLot;

namespace Monitor
{
        public class DvMCCB_BMA : Device
        {
                public override void updateState()
                {
                        DValues dv = RealData["CircuitCheck"];
                        DState state = new DState();
                        int data;
                        if (int.TryParse(dv.ShowValue, out data))
                        {
                                byte data1 = (Byte)(data / 256);
                                if ((data1 >> 6 & 1) == 1)
                                        state.SwitchState = SwitchStatus.Close;
                                else
                                        state.SwitchState = SwitchStatus.Open;
                                //MCCB低位在前/高位在后不符合常规逻辑，转换为高位在前/低位在后
                                data = data % 256 * 256 + data / 256;
                                int nCircuit = data & 0x4CBF;
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
                        if (Common.IsServer)
                        {
                                SaveData();
                                //产生新的脱扣记录
                                if (Tool.isOne(data, 12))
                                {
                                        _tripData = getZoneData(_tripData);
                                        SaveTrip();
                                        SmsAlarm();
                                }
                        }
                }

                double str2int(string name)
                {
                        double data;
                        double.TryParse(RealData[name].ShowValue, out data);
                        return data+new Random().Next(1, 500) / 1000f;
                        //加随机数的原因：使用模拟信号时电流一直保存不变的情况下，NotifyPropertyChanged不会被触发，实际运行时不会出现此状况
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

                        return dic;
                }

                protected override Dictionary<string, DValues> cvtParams()
                {
                        Dictionary<string, DValues> tmps = new Dictionary<string, DValues>();
                        int nIn, nSwH;
                        bool bIn = int.TryParse(BasicData["In"].ShowValue, out nIn);//片区0
                        bool bSw = int.TryParse(_params["ProtectSwitchH"].ShowValue, out nSwH);//片区2
                        if (bIn && bSw)
                        {
                                foreach (KeyValuePair<string, DValues> p in _params)
                                {
                                        DValues d = Tool.CloneDValues(p.Value);
                                        switch (p.Key)
                                        {
                                                case "ProtectSwitchH":
                                                        string[] items = d.Tag.Split('_');
                                                        d.ShowValue = (nSwH % 256 * 256 + nSwH / 256).ToString();
                                                        d.Tag = string.Join("_", items);
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
                        int nSw = int.Parse(dataList[0]);
                        dataList[0] = (nSw % 256 * 256 + nSw / 256).ToString();
                        dataList.Insert(1, "0");

                        byte len = (byte)dataList.Count;
                        byte[] snd = new byte[len * 2];

                        ComConverter cvt = new ComConverter();
                        List<string> keys = new List<string>(_params.Keys);
                        keys.RemoveRange(0, 2);
                        for (int i = 0; i < len; i++)
                        {
                                DValues d = _params[keys[i]];
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

                private string getTag(string tag, int index, int length)
                {
                        List<string> tags = new List<string>(tag.Split('_'));
                        tags = tags.Where((value, id) => id == 0 || (id >= index && id < index + length)).ToList();
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
                                                ZID=this.ZID,
                                                Address = this.Address,
                                                Time = DateTime.Now,
                                                Ia = State.Ia,
                                                Ib = State.Ib,
                                                Ic = State.Ic,
                                                Igf = str2int("Ig"),
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
                                return MyCom.QueryData(ZID,Address, start, end);
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
