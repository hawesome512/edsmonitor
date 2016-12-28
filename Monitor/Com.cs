using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;


namespace Monitor
{
        /// <summary>
        /// 通信方式：TCP,SP(串口)
        /// </summary>
        public enum ComType
        {
                TCP,
                SP,
                WCF,
                SL
        }

        public class Com : IDisposable
        {
                SerialPort sp;
                TcpClient tcp;
                ServiceReference1.EDSServiceClient wcf;
                string comTag;
                ComType cType;
                bool disposed = false;
                public Com(ComType type,string tag)
                {
                        cType = type;
                        comTag=tag;
                }

                ~Com()
                {
                        Dispose();
                }

                public ComType GetComType()
                {
                        return cType;
                }

                public void Dispose()
                {
                        if (!disposed)
                        {
                                if (sp != null)
                                        sp.Dispose();
                                if (wcf != null)
                                {
                                        wcf.Close();
                                        wcf = null;
                                }
                                disposed = true;
                                GC.SuppressFinalize(this);
                        }
                }
                /// <summary>
                /// 执行通信：读、写
                /// </summary>
                /// <param name="snd"></param>
                /// <returns>Array.Length,0:通信失败；1:写入；2*n:读取</returns>
                public byte[] Execute(byte zid,byte[] snd, int zoneIndex)
                {
                        switch (cType)
                        {
                                case ComType.SP:
                                        return spCom(snd);
                                case ComType.TCP:
                                        return tcpCom(snd);
                                case ComType.WCF:
                                        Thread.Sleep(200);//500→200
                                        return wcfCom(zid,snd, zoneIndex);
                                default:
                                        return null;
                        }
                }

                private byte[] wcfCom(byte zid,byte[] snd, int zoneIndex)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        try
                        {
                                if (zoneIndex == 3)
                                {
                                        return wcf.RemoteControl(zid, snd);
                                }
                                else
                                {
                                        return wcf.UpdateDevice(zid, snd[0], zoneIndex);
                                }
                        }
                        catch
                        {
                                return null;
                        }
                }

                private byte[] tcpCom(byte[] _snd)
                {
                        tcp = new TcpClient();
                        tcp.SendTimeout = 200;
                        tcp.ReceiveTimeout = 300;
                        try
                        {
                                tcp.Connect(comTag, 502);
                        }
                        catch
                        {
                                return new byte[] { };
                        }

                        byte[] snd = new byte[6 + _snd.Length];
                        snd[5] = (byte)_snd.Length;
                        _snd.CopyTo(snd, 6);
                        NetworkStream stream = tcp.GetStream();
                        stream.Write(snd, 0, snd.Length);
                        byte[] rcv = new byte[256];
                        try
                        {
                                stream.Read(rcv, 0, rcv.Length);
                        }
                        catch
                        {
                                return new byte[] { };
                        }
                        finally
                        {
                                stream.Dispose();
                                tcp.Close();
                        }
                        if (rcv[7] == 3 && (rcv[5] - rcv[8] == 3))
                        {
                                byte[] rcv1 = new byte[snd[11] * 2];
                                rcv.ToList().CopyTo(9, rcv1, 0, rcv1.Length);
                                return rcv1;
                        }
                        else if (rcv[7] == 16)
                        {
                                return new byte[] { 1 };
                        }
                        else
                        {
                                return new byte[] { };
                        }
                }

                private byte[] spCom(byte[] snd)
                {
                        if (comTag == null || comTag == string.Empty)
                                return new byte[] { };
                        if (sp == null || !sp.IsOpen)
                        {
                                sp = new SerialPort(comTag);
                                sp.ReadTimeout = 200;
                                sp.WriteTimeout = 200;
                                sp.StopBits = StopBits.Two;
                                sp.RtsEnable = true;
                                sp.Open();
                        }

                        //频繁通信，排除可能残留未读取数据
                        byte[] rcv = new byte[256];
                        if (sp.BytesToRead > 0)
                        {
                                sp.Read(rcv, 0, rcv.Length);
                                rcv = new byte[256];
                        }
                        sp.Write(Tool.CRCck(snd), 0, snd.Length + 2);
                        Thread.Sleep(500);
                        if (sp.BytesToRead > 0)
                                sp.Read(rcv, 0, rcv.Length);
                        if (rcv[1] == 3)
                        {
                                byte[] rtn = new byte[snd[5] * 2];
                                rcv.ToList().CopyTo(3, rtn, 0, rtn.Length);
                                return rtn;
                        }
                        else if (rcv[1] == 16)
                        {
                                return new byte[] { 1 };
                        }
                        else
                        {
                                return new byte[] { };
                        }
                }

                public List<Record> QueryData(byte zid,byte address, DateTime start, DateTime end)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        var result = wcf.QueryData(zid,address, start, end);
                        List<Record> records = new List<Record>();
                        foreach (var r in result)
                        {
                                records.Add(new Record(r));
                        }
                        return records;
                }

                public List<EDSLot.Trip> QueryTrip(byte zid,byte address, DateTime start, DateTime end)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        return wcf.QueryTrip(zid,address, start, end).ToList();
                }

                public void ChangeDeviceLiveness(byte zid, byte address, int liveness)
                {
                        Device device = Common.ZoneDevices[zid].Find(d => d.Address == address);
                        if (device != null)
                        {
                                //device.NeedParamsNum += liveness;
                                device.NeedParamsNum = liveness;
                        }
                        if (cType == ComType.WCF)
                        {
                                wcf.ChangeDeviceLiveness(zid, address, liveness/3);
                        }
                }

                public List<EDSLot.Energy> QueryEnergy(byte[] addrs, DateTime start, DateTime end)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }

                        //临时试验 12/19
                        var tmp = addrs.ToList().ConvertAll(a => Convert.ToInt32(a)).ToArray();
                        return wcf.QueryEnergy(tmp, start, end).ToList();
                }
        }
}
