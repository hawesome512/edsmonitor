using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor
{
        public class DvACB_philippine : Device
        {
                public override void updateState()
                {
                        DState state = new DState();
                        DValues dv = RealData["CircuitCheck"];
                        int data;
                        if (int.TryParse(dv.ShowValue, out data))
                        {
                                byte data1 = (Byte)(data % 256);
                                if ((data1>> 6 & 1) == 1)
                                        state.SwitchState = Switch.Open;
                                else
                                        state.SwitchState = Switch.Close;
                                if ((data1 & 0x0f) == 1)
                                        state.RunState = Run.Alarm;
                                else
                                        state.RunState = Run.Normal;

                                if (Params["LocalOrRemote"].ShowValue == "Local")
                                        state.ControlState = ControlMode.Local;
                                else
                                        state.ControlState = ControlMode.Remote;
                        }

                        //注1111
                        state.Ia = str2int("Ia");
                        state.Ib = str2int("Ib");
                        state.Ic = str2int("Ic");
                        state.Ua =str2int("Ua");
                        state.Ub = str2int("Ub");
                        state.Uc = str2int("Uc");

                        State = state;
                }

                double str2int(string name)
                {
                        double data;
                        double.TryParse(RealData[name].ShowValue, out data);
                        Random r = new Random();
                        return data+r.Next(1,100)/1000.0;
                }

                protected override Dictionary<string, DValues> cvtBasic()
                {
                        List<string> keys = _basicData.Keys.ToList();
                        keys.Remove("ControllerType");
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
                        int nIn,nIr,nSwH;
                        bool bIn = int.TryParse(BasicData["In"].ShowValue, out nIn);//片区0
                        bool bIr = int.TryParse(_params["Ir"].ShowValue,out nIr);//片区2
                        bool bSw = int.TryParse(_params["ProtectSwitchH"].ShowValue, out nSwH);
                        byte btSwH=(byte)(nSwH%256);
                        if (bIn&&bIr)
                        {
                                string breaker = BasicData["Breaker"].ShowValue;

                                foreach (KeyValuePair<string, DValues> p in _params)
                                {
                                        DValues d = Tool.CloneDValues(p.Value);

                                        if (d.Unit == "*In")
                                        {
                                                double valid = double.Parse(p.Value.ShowValue) / nIn;
                                                d.ShowValue = valid.ToString();
                                        }
                                        else if (d.Unit == "*Ir")
                                        {
                                                double valid = double.Parse(p.Value.ShowValue) / nIr;
                                                d.ShowValue = valid.ToString();
                                        }

                                        switch (p.Key)
                                        {
                                                case "Ir":
                                                        int nIndex = ((nSwH / 256 % 32) & 0x1C)>> 2;
                                                        string[] curves = { "1", "EIT", "HVF", "DT", "SIT", "VIT" };
                                                        string tag = string.Join("_", curves);
                                                        DValues dvCurve = new DValues() { ShowValue = curves[nIndex + 1], Unit = "/", Tag = tag };
                                                        dvCurve.Alias = "曲线";
                                                        tmps.Add("Curve", dvCurve);

                                                        tmps.Add(p.Key, d);
                                                        break;
                                                case "Output12":
                                                        d.Tag = string.Join("_", getTag(d.Tag, 11, 1));
                                                        d.ShowValue = d.ShowValue.Split('_').First();//ShowValue:0A_0B→0A(Output1)0B(Output2)
                                                        d.Alias = "触点1";
                                                        tmps.Add("Output.1", d);
                                                        d = Tool.CloneDValues(p.Value);
                                                        d.Tag = string.Join("_", getTag(d.Tag, 12, 1));
                                                        d.ShowValue = d.ShowValue.Split('_').Last();
                                                        d.Alias = "触点2";
                                                        tmps.Add("Output.2", d);
                                                        break;
                                                case "Output34":
                                                        d.Tag = string.Join("_", getTag(d.Tag, 1, 10));
                                                        d.ShowValue = d.ShowValue.Split('_').First();
                                                        d.Alias = "触点3";
                                                        tmps.Add("Output.3", d);
                                                        d = Tool.CloneDValues(p.Value);
                                                        d.Tag = string.Join("_", getTag(d.Tag, 1, 10));
                                                        d.ShowValue = d.ShowValue.Split('_').Last();
                                                        d.Alias = "触点4";
                                                        tmps.Add("Output.4", d);
                                                        break;
                                                case "Tsd":
                                                        List<string> steps1 = new List<string>() { "0", "0.1", "0.2", "0.3", "0.4" };
                                                        if ((btSwH>>4 & 0x1) == 1)
                                                        {
                                                                steps1.RemoveAt(0);
                                                        }
                                                        d.Tag += "_" + string.Join("_", steps1.ToArray());
                                                        tmps.Add(p.Key, d);
                                                        break;
                                                case "Ig/If":
                                                        if (breaker.Contains("F"))
                                                        {
                                                                double valid = int.Parse(d.ShowValue) / 1000;
                                                                d.ShowValue = valid.ToString();
                                                                string[] steps2 = { "0.5", "1", "2", "3", "5", "7", "10", "20", "30" };
                                                                d.Tag += "_" + string.Join("_", steps2.ToArray());
                                                                d.Unit = "A";
                                                                d.Alias = "If";
                                                                tmps.Add("If", d);
                                                        }
                                                        else
                                                        {
                                                                string[] steps3 = { "0.2", "0.3","0.4", "0.5","0.6", "0.7", "0.8", "1"};
                                                                d.Tag += "_" + string.Join("_", steps3.ToArray());
                                                                d.Alias = "Ig";
                                                                tmps.Add("Ig", d);
                                                        }
                                                        break;
                                                case "Tg/Tf":
                                                        if (breaker.Contains("F"))
                                                        {
                                                                string[] steps4 = { "0", "0.1", "0.2", "0.3", "0.4", "0.6", "0.8", "1" };
                                                                d.Tag += "_" + string.Join("_", steps4.ToArray());
                                                                d.Alias = "Tf";
                                                                tmps.Add("Tf", d);
                                                        }
                                                        else
                                                        {
                                                                List<string> steps5 = new List<string>() { "0", "0.1", "0.2", "0.3", "0.4" };
                                                                if ((btSwH>>5 & 0x1) == 1)
                                                                {
                                                                        steps5.RemoveAt(0);
                                                                }
                                                                d.Tag += "_" + string.Join("_", steps5.ToArray());
                                                                d.Alias = "Tg";
                                                                tmps.Add("Tg", d);
                                                        }
                                                        break;
                                                default:
                                                        tmps.Add(p.Key, d);
                                                        break;
                                        }
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
                        dataList[11] = string.Format("{0}_{1}", dataList[11], dataList[12]);
                        dataList[13] = string.Format("{0}_{1}", dataList[13], dataList[14]);
                        dataList.RemoveAt(14);
                        dataList.RemoveAt(12);

                        string[] curves = { "EIT", "HVF", "DT", "SIT", "VIT" };
                        int nIndex = curves.ToList().IndexOf(dataList[1]);
                        dataList.RemoveAt(1);
                        int tmp = int.Parse(dataList[0]);
                        dataList[0] = (tmp % 1024+1024*nIndex).ToString() ;
                        dataList.Insert(1, (tmp/ 1024).ToString());
                        byte len = (byte)dataList.Count;
                        byte[] snd = new byte[len * 2];

                        double In = int.Parse(BasicData["In"].ShowValue);
                        double Ir = double.Parse(dataList[2])*In;

                        ComConverter cvt = new ComConverter();
                        List<string> keys = new List<string>(_params.Keys);
                        keys.RemoveRange(0, 2);
                        for (int i = 0; i < len; i++)
                        {
                                DValues d=_params[keys[i]];
                                if (d.Unit == "*In")
                                {
                                        double valid = double.Parse(dataList[i]) * In;
                                        dataList[i] = valid.ToString();
                                }
                                else if (d.Unit == "*Ir")
                                {
                                        double valid = double.Parse(dataList[i]) * Ir;
                                        dataList[i] = valid.ToString();
                                }
                                var temp = cvt.CvtWrite(dataList[i], d.Cvt, d.Tag);
                                temp.ToList().CopyTo(0, snd, i * 2, 2);
                        }
                        return snd;
                }

                protected override byte[] GetValidRemote(string p)
                {
                        byte[] snd = { Address, 16, 0x50, 0x00, 00, 01, 2, 0x55, 0x55 };
                        if (p == "Close")
                                snd[7] = snd[8] = 0xAA;
                        return snd;
                }

                private string getTag(string tag, int index,int length)
                {
                        List<string> tags = new List<string>(tag.Split('_'));
                        tags=tags.Where((value, id) =>id==0||(id>= index && id < index + length)).ToList();
                        return string.Join("_", tags.ToArray());
                }
        }
}
