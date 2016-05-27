using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;

namespace Monitor
{
        public abstract class Device : INotifyPropertyChanged
        {
                public Com MyCom;
                public byte Address
                {
                        get;
                        set;
                }
                public byte ParentAddr
                {
                        get;
                        set;
                }
                public DeviceType DvType
                {
                        get;
                        set;
                }
                public string Name
                {
                        get;
                        set;
                }
                public string Tag
                {
                        get;
                        set;
                }
                public List<Device> Dependence
                {
                        get;
                        set;
                }
                ComConverter comcvt = new ComConverter();
                protected int nComFailed;
                public event EventHandler<EventArgs> PreRemoteModify, RemoteModified;

                public void InitDevice(byte addr, DeviceType type, string name,string tag,byte parent)
                {
                        Address = addr;
                        DvType = type;
                        Name = name;
                        Tag = tag;
                        ParentAddr = parent;
                }

                #region About Binding
                public event PropertyChangedEventHandler PropertyChanged;//当属性发生改变时触发事件通知绑定对象

                protected Dictionary<string, DValues> _basicData;
                public Dictionary<string, DValues> BasicData
                {
                        get
                        {
                                return cvtBasic();
                        }
                }
                protected virtual Dictionary<string, DValues> cvtBasic()
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

                protected Dictionary<string, DValues> _realData;
                public Dictionary<string, DValues> RealData
                {
                        get
                        {
                                return _realData;
                        }
                }

                protected Dictionary<string, DValues> _params;
                public Dictionary<string, DValues> Params
                {
                        get
                        {
                                return cvtParams();
                        }
                }
                protected virtual Dictionary<string, DValues> cvtParams()
                {
                        return _params;
                }

                protected DState _state = new DState();
                public DState State
                {
                        get
                        {
                                return _state;
                        }
                        set
                        {
                                _state = value;
                                if (PropertyChanged != null)
                                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("State"));
                        }
                }
                public virtual void updateState()
                {
                }
                #endregion

                #region GetData
                /// <summary>
                /// 获取设备数据(0-2片区)
                /// </summary>
                public virtual void GetData()
                {
                        if (nComFailed < 5)
                        {
                                //第一片区：只读一次
                                //第二片区：循环读取
                                //第三片区：进入设备页时循环读取
                                if (_basicData["Device"].ShowValue == null)
                                {
                                        _basicData = getZoneData(_basicData);
                                }
                                _realData = getZoneData(_realData);
                                if (Address == Common.SelectedAddress || _params["LocalOrRemote"].ShowValue == null)
                                {
                                        _params = getZoneData(_params);
                                }
                                updateState();
                        }
                }

                protected Dictionary<string, DValues> getZoneData(Dictionary<string, DValues> dic)
                {
                        byte[] snd = { Address, 3, 0, 0, 0, 1 };
                        int addr = dic.First().Value.Addr;
                        int length = dic.Count;
                        snd[2] = (byte)(addr / 256);
                        snd[3] = (byte)(addr % 256);
                        snd[5] = (byte)length;
                        byte[] rcv = MyCom.Execute(snd,Tag);
                        nComFailed = rcv.Length == 0 ? nComFailed + 1 : 0;
                        List<string> keys = new List<string>(dic.Keys);
                        for (int i = 0; i < keys.Count; i++)
                        {
                                DValues dv = dic[keys[i]];
                                if (rcv.Length == dic.Count * 2)
                                {
                                        byte[] source = new byte[] { rcv[2 * i], rcv[2 * i + 1] };
                                        dv.ShowValue = comcvt.CvtRead(source, dv.Cvt, dv.Tag);
                                }
                                else
                                {
                                        dv.ShowValue = null;
                                }
                                dic[keys[i]] = dv;
                        }
                        return dic;
                }
                #endregion

                #region Remote Control
                public byte[] SetParams(List<string> dataList)
                {
                        PreRemoteModify(null, new EventArgs());
                        System.Threading.Thread.Sleep(500);
                        byte[] snd = GetValidParams(dataList);
                        var result = MyCom.Execute(snd,Tag);
                        RemoteModified(null, new EventArgs());
                        return result;
                }

                protected abstract byte[] GetValidParams(List<string> dataList);

                public byte[] RemoteControl(string p)
                {
                        PreRemoteModify(null, new EventArgs());
                        System.Threading.Thread.Sleep(500);
                        byte[] snd = GetValidRemote(p);
                        var result = MyCom.Execute(snd,Tag);
                        RemoteModified(null, new EventArgs());
                        return result;
                }

                protected abstract byte[] GetValidRemote(string p);
                #endregion

                #region Init
                public virtual void InitAddress()
                {
                        string path = string.Format("DevicesConfig\\{0}.xml", DvType);
                        XmlElement xeRoot = Tool.GetXML(path);
                        _basicData = initDic(0, xeRoot);
                        _realData = initDic(1, xeRoot);
                        _params = initDic(2, xeRoot);
                }

                protected Dictionary<string, DValues> initDic(int zone, XmlElement xeRoot)
                {
                        Dictionary<string, DValues> dic = new Dictionary<string, DValues>();
                        foreach (XmlNode node in xeRoot.ChildNodes[zone].ChildNodes)
                        {
                                string name = node["Name"].InnerText;
                                DValues dv = new DValues()
                                {
                                        Addr = Convert.ToInt32(node["Address"].InnerText, 16),
                                        Cvt = int.Parse(node["ShowType"].InnerText),
                                        Unit = node["Unit"].InnerText,
                                        Tag = ValidParamTag(node["Extra"].InnerText),
                                        Alias = node["Alias"].InnerText
                                };
                                dic.Add(name, dv);
                        }
                        return dic;
                }

                protected virtual string ValidParamTag(string tag)
                {
                        return tag;
                }
                #endregion
        }
}
