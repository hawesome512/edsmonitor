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
                WCF
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
                public byte[] Execute(byte[] snd, int zoneIndex)
                {
                        switch (cType)
                        {
                                case ComType.SP:
                                        return spCom(snd);
                                case ComType.TCP:
                                        return tcpCom(snd);
                                case ComType.WCF:
                                        Thread.Sleep(500);
                                        return wcfCom(snd, zoneIndex);
                                default:
                                        return null;
                        }
                }

                private byte[] wcfCom(byte[] snd, int zoneIndex)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        if (zoneIndex == 3)
                        {
                                return wcf.RemoteControl(snd);
                        }
                        else
                        {
                                return wcf.UpdateDevice(snd[0], zoneIndex);
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

                #region 测试可用连接
                /// <summary>
                /// 测试可用的串口
                /// </summary>
                public bool TestCom(byte[] addrs)
                {
                        switch (cType)
                        {
                                case ComType.SP:
                                        return testSPCom(addrs);
                                case ComType.WCF:
                                        return testWCFCom(addrs);
                                default:
                                        return false;
                        }
                }

                private bool testWCFCom(byte[] addrs)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        foreach (byte addr in addrs)
                        {
                                try
                                {
                                        byte[] tmp = wcf.UpdateDevice(addr, 0);
                                        if (tmp != null)
                                                return true;
                                }
                                catch
                                {
                                }
                        }
                        return false;
                }

                private bool testSPCom(byte[] addrs)
                {
                        if (sp == null || !sp.IsOpen)
                        {
                                comTag = null;
                                List<string> ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
                                string defaultCom = Tool.GetConfig("ComTag");
                                if (ports.Contains(defaultCom))
                                {
                                        ports.Remove(defaultCom);
                                        ports.Insert(0, defaultCom);
                                }
                                foreach (string port in ports)
                                {
                                        sp = new SerialPort(port);
                                        sp.StopBits = StopBits.Two;
                                        sp.ReadTimeout = 500;
                                        sp.WriteTimeout = 500;
                                        sp.RtsEnable = true;
                                        foreach (byte addr in addrs)
                                        {
                                                sp.Open();
                                                byte[] tmp = new byte[] { addr, 3, 0, 0, 0, 1 };
                                                sp.Write(Tool.CRCck(tmp), 0, tmp.Length + 2);
                                                Thread.Sleep(500);
                                                byte[] rcv = new byte[256];
                                                try
                                                {
                                                        sp.Read(rcv, 0, rcv.Length);
                                                        if (rcv[1] == 3)
                                                        {
                                                                comTag = port;
                                                                Tool.SetConfig("ComTag", comTag);
                                                                return true;
                                                        }
                                                }
                                                catch
                                                {

                                                }
                                                finally
                                                {
                                                        sp.Close();//及时释放串口资源，否则容易出错
                                                        sp.Dispose();
                                                }
                                        }
                                }
                        }
                        return false;
                }
                #endregion

                public List<Record> QueryData(byte address, DateTime start, DateTime end)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        var result = wcf.QueryData(address, start, end);
                        List<Record> records = new List<Record>();
                        foreach (var r in result)
                        {
                                records.Add(new Record(r));
                        }
                        return records;
                }

                public List<EDSLot.Trip> QueryTrip(byte address, DateTime start, DateTime end)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        return wcf.QueryTrip(address, start, end).ToList();
                }

                public void ChangeSelAddress(byte address)
                {
                        Common.SelectedAddress = address;
                        if (cType == ComType.WCF)
                        {
                                if (wcf == null)
                                {
                                        wcf = new ServiceReference1.EDSServiceClient();
                                }
                                wcf.ChangeSelAddress(address);
                        }
                }

                public void ChangeSelZone(byte zid)
                {
                        if (zid == 0)
                        {
                                Common.OrdDevices.AddRange(Common.SelDevices);
                                Common.SelDevices.Clear();
                        }
                        else
                        {
                                Common.SelDevices = Common.OrdDevices.FindAll(d => d.ZID == zid);
                                Common.OrdDevices.RemoveAll(d => d.ZID == zid);
                        }
                        if (cType == ComType.WCF)
                        {
                                if (wcf == null)
                                {
                                        wcf = new ServiceReference1.EDSServiceClient();
                                }
                                wcf.ChangeSelZone(zid);
                        }
                }

                public List<EDSLot.Energy> QueryEnergy(int[] addrs, DateTime start, DateTime end)
                {
                        if (wcf == null)
                        {
                                wcf = new ServiceReference1.EDSServiceClient();
                        }
                        return wcf.QueryEnergy(addrs, start, end).ToList();
                }
        }
}
