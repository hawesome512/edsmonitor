﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Configuration;
using System.Security;
using System.Security.Cryptography;

namespace Monitor
{
        class Tool
        {
                /// <summary>
                /// 在byte数组中添加CRC校验码
                /// </summary>
                /// <param name="data">需要校验的字节数组</param>
                /// <returns>拥有CRC校验的字节数组</returns>
                public static byte[] CRCck(byte[] data)
                {
                        byte CRC_L = 0xFF;
                        byte CRC_H = 0xFF;   //CRC寄存器 
                        byte SH;
                        byte SL;
                        int j;

                        for (int i = 0; i < data.Length; i++)
                        {
                                CRC_L = (byte)(CRC_L ^ data[i]); //每一个数据与CRC寄存器进行异或 
                                for (j = 0; j < 8; j++)
                                {
                                        SH = CRC_H;
                                        SL = CRC_L;
                                        CRC_H = (byte)(CRC_H >> 1);      //高位右移一位
                                        //CRC_H = (byte)(CRC_H & 0x7F);
                                        CRC_L = (byte)(CRC_L >> 1);      //低位右移一位 
                                        //CRC_L = (byte)(CRC_L & 0x7F);
                                        if ((SH & 0x01) == 0x01) //如果高位字节最后一位为1 
                                        {
                                                CRC_L = (byte)(CRC_L | 0x80);   //则低位字节右移后前面补1 
                                        }             //否则自动补0 
                                        if ((SL & 0x01) == 0x01) //如果低位为1，则与多项式码进行异或 
                                        {
                                                CRC_H = (byte)(CRC_H ^ 0xA0);
                                                CRC_L = (byte)(CRC_L ^ 0x01);
                                        }
                                }
                        }

                        byte[] rtn = new byte[data.Length + 2];
                        data.CopyTo(rtn, 0);
                        rtn[data.Length] = CRC_L;
                        rtn[data.Length + 1] = CRC_H;
                        return rtn;
                        //return CRC_L * 256 + CRC_H;
                }

                public static XmlElement GetXML(string url)
                {
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreComments = true;
                        XmlReader reader = XmlReader.Create(url, settings);
                        XmlDocument doc = new XmlDocument();
                        doc.Load(reader);
                        return doc.DocumentElement;
                }


                public static Binding addBinding(string path, IValueConverter converter)
                {
                        Binding binding = new Binding(path);
                        binding.Converter = converter;
                        return binding;
                }

                public static MultiBinding addMulBinding(List<string> sources)
                {
                        MultiBinding mulBinding = new MultiBinding();
                        mulBinding.Converter = new MulStatesToLineStrokeConverter();
                        foreach (string s in sources)
                        {
                                mulBinding.Bindings.Add(new Binding(s));
                        }
                        return mulBinding;
                }

                public static DValues CloneDValues(DValues d)
                {
                        DValues tmp = new DValues()
                        {
                                Addr = d.Addr,
                                Cvt = d.Cvt,
                                ShowValue = d.ShowValue,
                                Tag = d.Tag,
                                Unit = d.Unit,
                                Alias = d.Alias
                        };
                        return tmp;
                }

                public static string GetValid(string input)
                {
                        return System.Text.RegularExpressions.Regex.Replace(input, "[^0-9a-zA-Z\u4e00-\u9fa5]", "");
                }

                /// <summary>
                /// Finds a Child of a given item in the visual tree. 
                /// </summary>
                /// <param name="parent">A direct parent of the queried item.</param>
                /// <typeparam name="T">The type of the queried item.</typeparam>
                /// <param name="childName">x:Name or Name of child. </param>
                /// <returns>The first parent item that matches the submitted type parameter. 
                /// If not matching item can be found, 
                /// http://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
                /// a null parent is being returned.</returns>
                /// 该方法来源于网上的解决方案——Hawesome
                /// 用此方法能在grid中根据控件的Name找到相应的TextBox控件
                public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
                {
                        // Confirm parent and childName are valid. 
                        if (parent == null)
                                return null;

                        T foundChild = null;

                        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                        for (int i = 0; i < childrenCount; i++)
                        {
                                var child = VisualTreeHelper.GetChild(parent, i);
                                // If the child is not of the request child type child
                                T childType = child as T;
                                if (childType == null)
                                {
                                        // recursively drill down the tree
                                        foundChild = FindChild<T>(child, childName);

                                        // If the child is found, break so we do not overwrite the found child. 
                                        if (foundChild != null)
                                                break;
                                }
                                else if (!string.IsNullOrEmpty(childName))
                                {
                                        var frameworkElement = child as FrameworkElement;
                                        // If the child's name is set for search
                                        if (frameworkElement != null && frameworkElement.Name == childName)
                                        {
                                                // if the child's name is of the request name
                                                foundChild = (T)child;
                                                break;
                                        }
                                }
                                else
                                {
                                        // child element found.
                                        foundChild = (T)child;
                                        break;
                                }
                        }

                        return foundChild;
                }

                public static bool isOne(int raw, int index)
                {
                        raw = (int)(raw >> index & 1);
                        return raw == 1 ? true : false;
                }

                public static int BitSet(int bt, int index, int para)
                {
                        return para == 1 ? (bt | (0x1 << index)) : (bt & ~(0x1 << index));
                }

                public static bool CheckSubset<T>(T[] complete, T[] subset)
                {
                        bool isSub = false;
                        int nLen1 = complete.Length;
                        int nLen2 = subset.Length;
                        T[] tmp = new T[nLen2];
                        for (int i = 0; i <= nLen1 - nLen2; i++)
                        {
                                Array.Copy(complete, i, tmp, 0, nLen2);
                                if (tmp.SequenceEqual(subset))
                                {
                                        isSub = true;
                                        break;
                                }
                        }
                        return isSub;
                }

                public static string GetConfig(string key)
                {
                        //return ConfigurationSettings.AppSettings[key];
                        return ConfigurationManager.AppSettings[key];
                }
                public static void SetConfig(string key, string value)
                {
                        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        cfa.AppSettings.Settings[key].Value = value;
                        cfa.Save();
                }

                public static string MD5Cryp(string source)
                {
                        MD5 md5 = new MD5CryptoServiceProvider();
                        byte[] palindata = Encoding.Default.GetBytes(source);
                        byte[] encryptdata = md5.ComputeHash(palindata);
                        //return Convert.ToBase64String(encryptdata);
                        return BitConverter.ToString(encryptdata).Replace("-", "");
                }

                public static int MatchItems(System.Windows.Controls.ItemCollection items, string value)
                {
                        double dValue;
                        if (double.TryParse(value, out dValue))
                        {
                                var t = items.Cast<object>().Select(s => s).ToList();
                                var array = items.Cast<object>().Select(s => s).ToList().ConvertAll(s => cvt2Double(s));
                                array = array.ConvertAll(s => Math.Abs(s - dValue));
                                return array.FindIndex(s => s == array.Min());
                        }
                        else
                        {
                                return 0;
                        }
                }

                static double cvt2Double(object obj)
                {
                        double d = 0;
                        double.TryParse(obj.ToString(), out d);
                        return d;
                }

                public static void FindDevicesParents(List<Device> devices)
                {
                        foreach (Device dv in devices)
                        {
                                List<Device> parents = new List<Device>();
                                //待优化：父级不应该包括本身
                                parents.Add(dv);
                                Device dvP=dv;
                                while (dvP.ParentAddr != 0)
                                {
                                        dvP = devices.Find(d => d.ZID == dvP.ZID && d.Address == dvP.ParentAddr);
                                        parents.Add(dvP);
                                }
                                dv.Dependence = parents;
                        }
                }

                public static List<string> GetDeviceDependence(Device device)
                {
                        List<string> sources = new List<string>();
                        for (int i = 0; i < device.Dependence.Count; i++)
                        {
                                sources.Add(string.Format("[{0}].State", i));
                        }
                        return sources;
                }

                public static double Ndb2db(double? source)
                {
                        return source == null ? 0 : (double)source;
                }

                /// <summary>
                /// 发送HTTP请求
                /// </summary>
                /// <param name="url">请求的URL</param>
                /// <param name="param">请求的参数</param>
                /// <returns>请求结果</returns>
                public static int CheckDateType(DateTime date)
                {
                        string strURL = "http://apis.baidu.com/xiaogg/holiday/holiday?d=" + date.ToString("yyyyMMdd");
                        System.Net.HttpWebRequest request;
                        request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(strURL);
                        request.Method = "GET";
                        // 添加header
                        request.Headers.Add("apikey", "1c7b90c856a0a68c505591a57e4bdaea");
                        System.Net.HttpWebResponse response;
                        response = (System.Net.HttpWebResponse)request.GetResponse();
                        System.IO.Stream s;
                        s = response.GetResponseStream();
                        string StrDate = "";
                        string strValue = "";
                        System.IO.StreamReader Reader = new System.IO.StreamReader(s, Encoding.UTF8);
                        while ((StrDate = Reader.ReadLine()) != null)
                        {
                                strValue += StrDate + "\r\n";
                        }
                        return int.Parse(strValue.Substring(0, 1));
                }
        }
}
