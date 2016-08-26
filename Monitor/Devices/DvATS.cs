using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor
{
        public class DvATS : Device
        {
                public override void updateState()
                {
                        DState state = new DState();
                        DValues dv = RealData["CircuitCheck"];
                        int data;
                        if (int.TryParse(dv.ShowValue, out data))
                        {
                                byte data1 = (Byte)(data % 256);
                                if (Tool.isOne(data1, 1))
                                        state.SwitchState = SwitchStatus.ATS_N;
                                else if (Tool.isOne(data1, 4))
                                        state.SwitchState = SwitchStatus.ATS_R;
                                else if (!Tool.isOne(data1, 1) && !Tool.isOne(data1, 4))
                                        state.SwitchState = SwitchStatus.Open;
                                else
                                        state.SwitchState = SwitchStatus.Unknown;

                                state.Ia = str2int("Ia");
                                state.Ib = str2int("Ib");
                                state.Ic = str2int("Ic");
                                if (Tool.isOne(data1, 0) && Tool.isOne(data1, 4))
                                {
                                        state.Ua = str2int("Ua2");
                                        state.Ub = str2int("Ub2");
                                        state.Uc = str2int("Uc2");
                                }
                                else
                                {
                                        state.Ua = str2int("Ua1");
                                        state.Ub = str2int("Ub1");
                                        state.Uc = str2int("Uc1");
                                }

                                data = int.Parse(RealData["SysErrType"].ShowValue);
                                if (data> 0)
                                        state.RunState = Run.Alarm;
                                else
                                        state.RunState = Run.Normal;
                                string[] items = RealData["SysErrType"].Tag.Split('_');
                                for (int i = 0; i < items.Length; i++)
                                {
                                        if(Tool.isOne(data,i))
                                        {
                                                state.ErrorMsg += items[i]+" ";
                                        }
                                }

                                if (Params["LocalOrRemote"].ShowValue == "Local")
                                        state.ControlState = ControlMode.Local;
                                else
                                        state.ControlState = ControlMode.Remote;
                        }

                        State = state;

                        //保存数据模块

                        //保护数据模块
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

                protected override byte[] GetValidParams(List<string> dataList)
                {
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
                        byte[] snd = { Address, 16, 0x50, 0x00, 00, 01, 2, 0x55, 0x55 };
                        switch (p)
                        {
                                case "N":
                                        snd[7] = snd[8] = 0x55;
                                        break;
                                case "S":
                                        snd[7] = snd[8] = 0xaa;
                                        break;
                                default:
                                        snd[7] = snd[8] = 0xf0;
                                        break;
                        }
                        return snd;
                }

                private string getTag(string tag, int index, int length)
                {
                        List<string> tags = new List<string>(tag.Split('_'));
                        tags = tags.Where((value, id) => id == 0 || (id>= index && id < index + length)).ToList();
                        return string.Join("_", tags.ToArray());
                }


                //备注：待优化——重复
                protected override string ValidParamTag(string tag)
                {
                        string pattern1 = @"^\d+_\d+\*\d+\*\d+$";
                        string pattern2 = @"^\d+_\d+(\.\d+)?\*\d+\.\d+\*\d+(\.\d+)?$";
                        System.Text.RegularExpressions.Regex regex1 = new System.Text.RegularExpressions.Regex(pattern1);
                        System.Text.RegularExpressions.Regex regex2 = new System.Text.RegularExpressions.Regex(pattern2);
                        if (regex1.IsMatch(tag))
                        {
                                var array1 = tag.Split('_');
                                var array2 = array1[1].Split('*').ToList().ConvertAll(s => int.Parse(s));
                                List<string> array3 = new List<string>();
                                for (int i = array2[0]; i <= array2[2]; i += array2[1])
                                {
                                        array3.Add(i.ToString());
                                }
                                return array1[0] + "_" + string.Join("_", array3.ToArray());
                        }
                        else if (regex2.IsMatch(tag))
                        {
                                var array1 = tag.Split('_');
                                var array2 = array1[1].Split('*').ToList().ConvertAll(s => float.Parse(s)*10);
                                List<string> array3 = new List<string>();
                                for (float i = array2[0]; i <= array2[2]; i += array2[1])
                                {
                                        array3.Add((i/10).ToString("0.0"));
                                }
                                return array1[0] + "_" + string.Join("_", array3.ToArray());
                        }
                        else
                        {
                                return tag;
                        }
                }


        }
}
