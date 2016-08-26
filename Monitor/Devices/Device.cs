using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;
using EDSLot;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Monitor
{
        [DataContract]
        public abstract class Device : INotifyPropertyChanged
        {
                [DataMember]
                public List<byte[]> DataList = new List<byte[]>();

                public Com MyCom;
                public byte ZID
                {
                        get;
                        set;
                }
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

                public string Alias
                {
                        get;
                        set;
                }
                public List<Device> Dependence
                {
                        get;
                        set;
                }
                protected ComConverter comcvt = new ComConverter();
                protected int nComFailed;
                public event EventHandler<EventArgs> PreRemoteModify, RemoteModified;
                public bool HasInited = false;

                public void InitDevice(byte zid,byte addr, DeviceType type, string name, string alias, byte parent)
                {
                        ZID = zid;
                        Address = addr;
                        DvType = type;
                        Name = name;
                        Alias = alias;
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
                        set
                        {
                                if (PropertyChanged != null)
                                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Params"));
                        }
                }
                protected virtual Dictionary<string, DValues> cvtParams()
                {
                        return _params;
                }

                protected Dictionary<string, DValues> _tripData;
                public Dictionary<string, DValues> TripData
                {
                        get
                        {
                                return _tripData;
                        }
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
                                        _basicData = getZoneData(_basicData, 0);
                                }
                                _realData = getZoneData(_realData, 1);
                                if (Address == Common.SelectedAddress || _params["LocalOrRemote"].ShowValue == null)
                                {
                                        _params = getZoneData(_params, 2);
                                }
                                updateState();
                        }
                        HasInited = true;
                }

                protected Dictionary<string, DValues> getZoneData(Dictionary<string, DValues> dic, int zoneIndex = -1)
                {
                        byte[] snd = { Address, 3, 0, 0, 0, 1 };
                        int addr = dic.First().Value.Addr;
                        int length = dic.Count;
                        snd[2] = (byte)(addr / 256);
                        snd[3] = (byte)(addr % 256);
                        snd[5] = (byte)length;
                        byte[] rcv = MyCom.Execute(snd, zoneIndex);
                        //只有0000/1000/2000片区数据需要通过WCF共享zoneIndex=0/1/2
                        if (zoneIndex != -1)
                        {
                                DataList[zoneIndex] = rcv;
                        }
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
                public bool SetParams(List<string> dataList)
                {
                        PreRemoteModify(this, null);
                        System.Threading.Thread.Sleep(2000);
                        int tag = Common.IsServer ?-1:3;

                        byte[] snd = GetValidParams(dataList);
                        int len = snd.Length / 4 * 2;//参数成对出现，必须为偶数
                        byte[] snd1 = snd.Take(len).ToArray();
                        byte[] snd2 = snd.Skip(len).ToArray();
                        byte[] baseSnd = new byte[7];
                        baseSnd[0] = Address;
                        baseSnd[1] = 16;
                        baseSnd[2] = 0x20;

                        baseSnd[3] = 0x02;
                        baseSnd[5] = (byte)(snd1.Length / 2);
                        baseSnd[6] = (byte)snd1.Length;
                        byte[] rcv1 = MyCom.Execute(baseSnd.Concat(snd1).ToArray(),tag);
                        baseSnd[3] = (byte)(2 + snd1.Length);
                        baseSnd[5] = (byte)(snd2.Length / 2);
                        baseSnd[6] = (byte)snd2.Length;
                        byte[] rcv2 = MyCom.Execute(baseSnd.Concat(snd2).ToArray(),tag);

                        RemoteModified(this, null);
                        if (rcv1.Length == 1 && rcv2.Length == 1)
                        {
                                return true;
                        }
                        else
                        {
                                return false;
                        }
                }


                protected abstract byte[] GetValidParams(List<string> dataList);

                public byte[] RemoteControl(string p)
                {
                        byte[] snd = GetValidRemote(p);
                        return Remote(snd);
                }

                protected abstract byte[] GetValidRemote(string p);

                public byte[] Remote(byte[] snd)
                {
                        PreRemoteModify(this, null);
                        System.Threading.Thread.Sleep(2000);
                        int tag = Common.IsServer ? -1:3;
                        var result = MyCom.Execute(snd, tag);
                        RemoteModified(this, null);
                        return result;
                }
                #endregion

                #region Init
                public virtual void InitAddress()
                {
                        string path = string.Format("Config\\{0}.xml", DvType);
                        XmlElement xeRoot = Tool.GetXML(path);
                        _basicData = initDic(0, xeRoot);
                        DataList.Add(new byte[_basicData.Count * 2]);
                        _realData = initDic(1, xeRoot);
                        DataList.Add(new byte[_realData.Count * 2]);
                        _params = initDic(2, xeRoot);
                        DataList.Add(new byte[_params.Count * 2]);
                        if (xeRoot.ChildNodes.Count > 3)
                        {
                                //默认设定配置地址表XML文件时忽略3/4/5000片区，6000片区为脱扣记录，以后可能需要修改此设定
                                _tripData = initDic(3, xeRoot);
                        }
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

                protected virtual void SaveData()
                {
                }
                protected virtual void SaveTrip()
                {
                        using (EDSEntities context = new EDSEntities())
                        {
                                context.Trip.Add(new Trip()
                                {
                                        ZID=this.ZID,
                                        Address = this.Address,
                                        Time = DateTime.Now,
                                        Phase = TripData["Phase"].ShowValue,
                                        Type = TripData["Type"].ShowValue,
                                        Ia = tripStr2int("Ia"),
                                        Ib = tripStr2int("Ib"),
                                        Ic = tripStr2int("Ic"),
                                        IN = tripStr2int("IN"),
                                        It = tripStr2int("It"),
                                        Tt = tripStr2int("Tt"),
                                        Ip = tripStr2int("Ip"),
                                        Tp = tripStr2int("Tp"),
                                        Ir = getIr()
                                });
                                try
                                {
                                        context.SaveChanges();
                                }
                                catch
                                {
                                }
                        }

                }
                protected virtual void SmsAlarm()
                {
                        string info = string.Format("{0}-{1}", TripData["Phase"].ShowValue, TripData["Type"].ShowValue);
                        Common.SmsAlarm.SendSms(Address.ToString(), info);
                }

                private double getIr()
                {
                        return double.Parse(BasicData["In"].ShowValue) * double.Parse(Params["Ir"].ShowValue);
                }

                double tripStr2int(string name)
                {
                        double data;
                        if (!TripData.Keys.Contains(name))
                        {
                                return 0;
                        }
                        else
                        {
                                double.TryParse(TripData[name].ShowValue, out data);
                                return data;
                        }
                }

                public virtual List<Record> QueryData(DateTime start, DateTime end)
                {
                        return null;
                }
                public virtual List<Trip> QueryTrip(DateTime start, DateTime end)
                {
                        if (!Common.IsServer)
                                return MyCom.QueryTrip(Address, start, end);

                        using (EDSEntities context = new EDSEntities())
                        {
                                var result = from m in context.Trip
                                             where m.ZID==this.ZID&& m.Address == this.Address && (m.Time >= start && m.Time <= end)
                                             orderby m.Time descending
                                             select m;
                                return result.ToList();
                        }
                }
        }
}
