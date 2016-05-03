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
                SP
        }

        public class Com : IDisposable
        {
                SerialPort sp;
                TcpClient tcp;
                string strIp;
                string strCom;
                ComType cType;
                bool disposed = false;

                public Com(ComType type)
                {
                        cType = type;
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

                                //TCP方式通信每次都有及时回收资源，不做额外处理。

                                disposed = true;
                                GC.SuppressFinalize(this);
                        }
                }

                /// <summary>
                /// 执行通信：读、写
                /// </summary>
                /// <param name="snd"></param>
                /// <returns>Array.Length,0:通信失败；1:写入；2*n:读取</returns>
                public byte[] Execute(byte[] snd,string tag)
                {
                        //待优化
                        strIp = tag;

                        if (cType == ComType.SP)
                                return spCom(snd);
                        else
                                return tcpCom(snd);
                }

                private byte[] tcpCom(byte[] _snd)
                {
                        tcp = new TcpClient();
                        tcp.SendTimeout = 200;
                        tcp.ReceiveTimeout = 300;
                        try
                        {
                                tcp.Connect(strIp, 502);
                        }
                        catch
                        {
                                return new byte[]{};
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
                                return new byte[]{};
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
                                return new byte[]{};
                        }
                }

                private byte[] spCom(byte[] snd)
                {
                        if (strCom == null || strCom == string.Empty)
                                return new byte[]{};
                        if (sp == null || !sp.IsOpen)
                        {
                                sp = new SerialPort(strCom);
                                sp.ReadTimeout = 200;
                                sp.WriteTimeout = 200;
                                sp.StopBits = StopBits.Two;
                                sp.RtsEnable = true;
                                sp.Open();
                        }

                        //频繁通信，排除可能残留未读取数据
                        byte[] rcv = new byte[256];
                        if(sp.BytesToRead > 0)
                        {
                                sp.Read(rcv, 0, rcv.Length);
                                rcv = new byte[256];
                        }
                        sp.Write(Tool.CRCck(snd), 0, snd.Length + 2);
                        Thread.Sleep(500);
                        //Thread.Sleep(snd[1]==16?500:200);//写模式设置长时间，否则容易出错
                        if(sp.BytesToRead>0)
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

                /// <summary>
                /// 测试可用的串口
                /// </summary>
                public string TestCom(byte addr,string defaultCom)
                {
                        if (sp == null || !sp.IsOpen)
                        {
                                strCom = null;
                                List<string> ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
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
                                        sp.Open();
                                        byte[] tmp = new byte[] { addr, 3, 0, 0, 0, 1};
                                        sp.Write(Tool.CRCck(tmp), 0, tmp.Length+2);
                                        Thread.Sleep(500);
                                        byte[] rcv = new byte[256];
                                        try
                                        {
                                                sp.Read(rcv, 0, rcv.Length);
                                                if (rcv[1] == 3)
                                                {
                                                        strCom = port;
                                                        break;
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
                        return strCom;
                }
        }
}
