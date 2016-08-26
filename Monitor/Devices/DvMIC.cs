using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor
{
        public class DvMIC : Device
        {
                public override void updateState()
                {
                        DState state = new DState();
                        DValues dv = RealData["Abnormal"];
                        int data;
                        if (int.TryParse(dv.ShowValue, out data))
                        {
                                if (data > 0)
                                        state.RunState = Run.Alarm;
                                else
                                        state.RunState = Run.Normal;
                                string[] items = dv.Tag.Split('_');
                                for (int i = 0; i < items.Length; i++)
                                {
                                        if (Tool.isOne(data, i))
                                        {
                                                state.ErrorMsg += items[i] + " ";
                                        }
                                }

                                if (Params["LocalOrRemote"].ShowValue == "Local")
                                        state.ControlState = ControlMode.Local;
                                else
                                        state.ControlState = ControlMode.Remote;
                        }

                        if (int.TryParse(RealData["WorkingStatusH"].ShowValue, out data))
                        {
                                switch (data & 3)
                                {
                                        case 0:
                                                state.SwitchState = SwitchStatus.Wait;
                                                break;
                                        case 1:
                                                state.SwitchState = SwitchStatus.Ready;
                                                break;
                                        case 2:
                                                state.SwitchState = SwitchStatus.Run;
                                                break;
                                }
                        }

                        state.Ia = str2int("Ia");
                        state.Ib = str2int("Ib");
                        state.Ic = str2int("Ic");
                        state.Ua = str2int("Uab");
                        state.Ub = str2int("Ubc");
                        state.Uc = str2int("Uca");
                        state.FR = str2int("Freq");
                        state.PE = str2int("WP_kwhH") * Math.Pow(2, 16) + str2int("WP_kwhL") + str2int("WP_wh") / 1000f;
                        state.P = str2int("PH") * Math.Pow(2, 16) + str2int("PL");
                        state.Q = str2int("QH") * Math.Pow(2, 16) + str2int("QL");
                        state.PF = str2int("Factor");

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
                        int nIn = 0;
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
                                        case 7:
                                                dic.Add(keys[i], tmp);
                                                nIn = (int)double.Parse(tmp.ShowValue);//Ir1可调范围由In决定
                                                switch (nIn)
                                                {
                                                        case 2:
                                                                _params["Ir"].Tag = "100_" + string.Join("_", initList(0.5, 2, 0.05));
                                                                break;
                                                        case 6://In=6.3取整
                                                                _params["Ir"].Tag = "100_" + string.Join("_", initList(1.6, 6.3, 0.1));
                                                                break;
                                                        case 25:
                                                                var itmes1 = initList(6.3, 24.8, 0.5);
                                                                var items2 = initList(6.5, 25, 0.5);
                                                                _params["Ir"].Tag = "100_" + string.Join("_", itmes1.Concat(items2));
                                                                break;
                                                        case 100:
                                                                _params["Ir"].Tag = "10_" + string.Join("_", initList(25, 100, 1));
                                                                break;
                                                        case 200:
                                                                _params["Ir"].Tag = "10_" + string.Join("_", initList(100, 200, 5));
                                                                break;
                                                        case 400:
                                                                _params["Ir"].Tag = "10_" + string.Join("_", initList(200, 400, 10));
                                                                break;
                                                        case 800:
                                                                _params["Ir"].Tag = "10_" + string.Join("_", initList(400, 800, 10));
                                                                break;
                                                }
                                                break;
                                        case 9:
                                                dic.Add(keys[i], tmp);
                                                int nSwitch = int.Parse(tmp.ShowValue);
                                                switch (nSwitch & 3)
                                                {
                                                        case 2://漏电
                                                                List<double> items = new List<double>() { 0.3, 0.5, 1, 3, 10, 30 };
                                                                _params["Igf"].Tag = "100_OFF_" + string.Join("_", items.Where(n => n < nIn));
                                                                _params["Tgf"].Tag = "10_0_0.1_0.5_1";
                                                                _params["ProType_Igf"].Alias = "漏电保护";
                                                                _params["Igf"].Alias = "漏电动作电流(If)";
                                                                _params["Igf"].Unit = "(A)";
                                                                _params["Tgf"].Alias = "漏电动作时间(Tf)";
                                                                break;
                                                        case 1://接地
                                                                _params["Igf"].Tag = "100_OFF_" + string.Join("_", initList(0.2, 1, 0.05));
                                                                _params["Tgf"].Tag = "10_0.1_0.2_0.3_0.4";
                                                                _params["ProType_Igf"].Alias = "接地保护";
                                                                _params["Igf"].Alias = "接地动作电流(Ig)";
                                                                _params["Igf"].Unit = "*Ie";
                                                                _params["Tgf"].Alias = "接地动作时间(Tg)";
                                                                break;
                                                        case 0://都关闭
                                                                break;
                                                }
                                                break;
                                        default:
                                                dic.Add(keys[i], tmp);
                                                break;
                                }

                        }
                        return dic;
                }

                List<double> initList(double min, double max, double step)
                {
                        List<double> tmps = new List<double>();
                        for (double i = min; i <= max; i += step)
                        {
                                tmps.Add(Math.Round(i,3));
                        }
                        return tmps;
                }

                protected override Dictionary<string, DValues> cvtParams()
                {
                        Dictionary<string, DValues> tmps = new Dictionary<string, DValues>();
                        foreach (KeyValuePair<string, DValues> p in _params)
                        {
                                DValues d = Tool.CloneDValues(p.Value);
                                if (d.Cvt == 2)
                                {
                                        var items = d.Tag.Split('_').Where((i, index) => index %2 == 1);
                                        d.Tag ="0_"+ string.Join("_", items);
                                }
                                if (p.Key == "StartType")
                                {
                                        string[] types = new string[] { "星三角起动","电阻降压启动","自耦变压器启动"};
                                        if (types.Contains(d.ShowValue))
                                        {
                                                _params["SwitchTime"].Tag = "10_" + string.Join("_", initList(1, 60, 1));
                                        }
                                        else
                                        {
                                                _params["SwitchTime"].Tag = "10_0";
                                        }
                                }
                                tmps.Add(p.Key, d);
                        }
                        return tmps;
                }

                protected override byte[] GetValidParams(List<string> dataList)
                {
                        dataList.RemoveAt(0);//MIC没有开关量
                        byte len = (byte)dataList.Count;
                        byte[] snd = new byte[len * 2];

                        List<string> keys = new List<string>(Params.Keys);
                        keys.RemoveRange(0, 2);
                        for (int i = 0; i < len; i++)
                        {
                                DValues d = _params[keys[i]];
                                var temp = comcvt.CvtWrite(dataList[i], d.Cvt, d.Tag);
                                temp.ToList().CopyTo(0, snd, i * 2, 2);
                        }
                        return snd;
                }

                protected override byte[] GetValidRemote(string p)
                {
                        byte[] snd = { Address, 16, 0x50, 0x00, 00, 01, 2, 0x55, 0x55 };
                        switch (p)
                        {
                                case "Stop":
                                        snd[7] = snd[8] = 0x55;
                                        break;
                                case "StartA":
                                        snd[7] = snd[8] = 0xAA;
                                        break;
                                case "StartB":
                                        snd[7] = snd[8] = 0xBB;
                                        break;
                                case "Reset":
                                        snd[7] = snd[8] = 0x66;
                                        break;
                                case "CleanQ":
                                        snd[7] = snd[8] = 0x77;
                                        break;
                        }
                        return snd;
                }

                private string getTag(string tag, int index, int length)
                {
                        List<string> tags = new List<string>(tag.Split('_'));
                        tags = tags.Where((value, id) => id == 0 || (id >= index && id < index + length)).ToList();
                        return string.Join("_", tags.ToArray());
                }

                protected override string ValidParamTag(string tag)
                {
                        string pattern = @"^\d+(\.\d+)?_(OFF_)?\d+(\.\d+)?\*\d+(\.\d+)?\*\d+(\.\d+)?$";
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
                        if (regex.IsMatch(tag))
                        {
                                var array1 = tag.Split('_');
                                var array2 = array1.Last().Split('*').ToList().ConvertAll(s => double.Parse(s));
                                var array3 = initList(array2[0], array2[2], array2[1]);
                                string tmp = tag.Contains("OFF") ? "OFF_" : string.Empty;
                                return array1[0] + "_"+tmp + string.Join("_", array3.ToArray());
                        }
                        else
                        {
                                return tag;
                        }
                }
        }
}
