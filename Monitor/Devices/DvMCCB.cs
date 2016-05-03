﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor
{
        public class DvMCCB:Device
        {
                public DvMCCB(byte addr, DeviceType type, string name,string tag)
                        : base(addr, type, name,tag)
                {

                }
                public override void updateState()
                {
                        DValues dv = RealData["CircuitCheck"];
                        DState state = new DState();
                        int data;
                        if (int.TryParse(dv.ShowValue, out data))
                        {
                                byte data1 = (Byte)(data / 256);
                                if ((data1 >> 6 & 1) == 1)
                                        state.SwitchState = Switch.Close;
                                else
                                        state.SwitchState = Switch.Open;
                                data = data % 256 * 256 + data / 256;
                                int nCircuit = data & 0x4CBF;
                                if (nCircuit> 0)
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
                                        switch (p.Key)
                                        {
                                                case "ProtectSwitchH":
                                                        string[] items = d.Tag.Split('_');
                                                        d.ShowValue = (nSwH%256*256+nSwH/256).ToString();
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
                        byte[] snd = new byte[7 + len * 2];
                        snd[0] = Address;
                        snd[1] = 16;
                        snd[2]=0x20;
                        snd[3]=0x02;
                        snd[5]=len;
                        snd[6] = (byte)(2 * len);

                        ComConverter cvt = new ComConverter();
                        List<string> keys = new List<string>(_params.Keys);
                        keys.RemoveRange(0, 2);
                        for (int i = 0; i < len; i++)
                        {
                                DValues d=_params[keys[i]];
                                var temp = cvt.CvtWrite(dataList[i], d.Cvt, d.Tag);
                                temp.ToList().CopyTo(0, snd, i * 2 + 7, 2);
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
                        tags=tags.Where((value, id) =>id==0||(id >= index && id < index + length)).ToList();
                        return string.Join("_", tags.ToArray());
                }
        }
}