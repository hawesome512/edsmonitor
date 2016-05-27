﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor
{
        class ComConverter
        {
                byte[] source;
                string extra;
                int nInmFactor = 1;//ACB框架等级
                //待优化——太多case
                public string CvtRead(byte[] _source, int showType, string _extra)
                {
                        source = _source;
                        extra = _extra;
                        switch (showType)
                        {
                                case 0:
                                        return cvtRZero();
                                case 1:
                                        return cvtROne();
                                case 2:
                                        return cvtRTwo();
                                case 4:
                                        return cvtRFour();
                                case 5:
                                        return cvtRFive();
                                case 10:
                                        return cvtRTen();
                                case 11:
                                        return cvtREleven();
                                case 12:
                                        return cvtRTwelve();
                                case 13:
                                        return cvtRThirteen();
                                case 14:
                                        return cvtRFourteen();
                                case 21:
                                        return cvtRTwentyOne();
                                case 22:
                                        return cvtRTwentyTwo();
                                case 23:
                                        return cvtRTwentyThree();
                                case 30:
                                        return cvtRThirty();
                                case 31:
                                        return cvtRThirtyOne();
                                case 32:
                                        return cvtRThirtyTwo();
                                case 33:
                                        return cvtRThirtyThree();
                                default:
                                        return cvtRDefault();
                        }
                }

                public byte[] CvtWrite(string value, int showType, string _extra)
                {
                        switch (showType)
                        {
                                case 1:
                                        return cvtWOne(value, _extra);
                                case 11:
                                        return cvtWEleven(value, _extra);
                                case 12:
                                        return cvtWTwelve(value, _extra);
                                case 32:
                                        return cvtWThirtyTwo(value, _extra);
                                case 33:
                                        return cvtWThirtyThree(value,_extra);
                                default:
                                        return cvtWDefault(value, _extra);
                        }
                }

                /// <summary>
                /// 开关量
                /// </summary>
                string cvtRDefault()
                {
                        return (source[0] * 256 + source[1]).ToString();
                }
                byte[] cvtWDefault(string value, string _extra)
                {
                        byte[] bts=new byte[2];
                        int data=int.Parse(value);
                        bts[0] = (byte)(data / 256);
                        bts[1] = (byte)(data % 256);
                        return bts;
                }


                /// <summary>
                /// 十六进制（默认）
                /// </summary>
                string cvtRZero()
                {
                        return string.Format("{0:X2} {1:X2}", source[0], source[1]);
                }

                /// <summary>
                /// 十进制
                /// </summary>
                string cvtROne()
                {
                        int value = source[0] * 256 + source[1];
                        double factor1 = double.Parse(extra.Split('_')[0]);
                        return (value / factor1).ToString();
                }
                byte[] cvtWOne(string value, string _extra)
                {
                        double factor1 = double.Parse(_extra.Split('_')[0]);
                        double data1=double.Parse(value);
                        int data = Convert.ToInt32(data1 * factor1);
                        byte[] bts = new byte[2];
                        bts[0] = (byte)(data / 256);
                        bts[1] = (byte)(data % 256);
                        return bts;
                }

                /// <summary>
                /// 固定值
                /// </summary>
                string cvtRTwo()
                {
                        List<string> listExt = extra.Split('_').ToList();
                        string value = (source[0] * 256 + source[1]).ToString();
                        int index =listExt.Contains(value)? listExt.IndexOf(value):0;
                        return listExt[index + 1];
                }

                /// <summary>
                /// 控制器
                /// </summary>
                string cvtRFour()
                {
                        List<string> listExt = extra.Split('_').ToList();
                        string value = source[0].ToString();
                        int index = listExt.Contains(value) ? listExt.IndexOf(value) : 0;
                        return string.Format("{0} {1}P", listExt[index + 1], source[1]);
                } 
                
                /// <summary>
                /// 软件版本
                /// </summary>
                string cvtRFive()
                {
                        return string.Format("V{0:x}.{1:x}", source[0], source[1]);
                }

                #region ACB
                /// <summary>
                /// ACB框架等级
                /// </summary>
                /// <returns></returns>
                private string cvtRTen()
                {
                        int value = source[0] * 256 + source[1];
                        double factor1 = double.Parse(extra.Split('_')[0]);
                        value = (int)(value / factor1);
                        nInmFactor = value == 6300 ? 2 : 1;
                        return value.ToString();
                }

                /// <summary>
                /// ACB框架等级不同，对应的转换系数也不同
                /// 框①②*1,框③*2
                /// </summary>
                /// <returns></returns>
                private string cvtREleven()
                {
                        int value = source[0] * 256 + source[1];
                        double factor1 = double.Parse(extra.Split('_')[0]);
                        return (value / factor1 / nInmFactor).ToString();
                }
                private byte[] cvtWEleven(string value, string _extra)
                {
                        double factor1 = double.Parse(_extra.Split('_')[0]);
                        double data1 = double.Parse(value);
                        int data = Convert.ToInt32(data1 * factor1 * nInmFactor);
                        byte[] bts = new byte[2];
                        bts[0] = (byte)(data / 256);
                        bts[1] = (byte)(data % 256);
                        return bts;
                }

                /// <summary>
                /// ACB触点开关
                /// </summary>
                private string cvtRTwelve()
                {
                        List<string> listExt = extra.Split('_').ToList();
                        listExt.RemoveAt(0);
                        return string.Format("{0}_{1}", listExt[source[1]], listExt[source[0]]);
                }
                byte[] cvtWTwelve(string value, string _extra)
                {
                        List<string> listExt = _extra.Split('_').ToList();
                        listExt.RemoveAt(0);
                        var values = value.Split('_');
                        byte[] bts = new byte[2];
                        bts[0] = (byte)listExt.IndexOf(values[1]);
                        bts[1] = (byte)listExt.IndexOf(values[0]);
                        return bts;
                }

                /// <summary>
                /// ACB日期
                /// </summary>
                string cvtRThirteen()
                {
                        return string.Format("{0:D2} {1:D2}", source[0], source[1]);
                }

                /// <summary>
                /// ACB断路器型号
                /// </summary>
                string cvtRFourteen()
                {
                        List<string> listExt = extra.Split('_').ToList();
                        string value = source[0].ToString();
                        int index = listExt.Contains(value) ? listExt.IndexOf(value) : 0;
                        return string.Format("{0} {1}P",listExt[index + 1],source[1]);
                }

                #endregion ACB

                #region MCCB
                /// <summary>
                /// MCCB断路器型号_old
                /// </summary>
                /// <returns></returns>
                private string cvtRTwentyOne()
                {
                        return string.Format("{0} {1}P", Char.ConvertFromUtf32(source[0]), source[1]);
                }

                /// <summary>
                /// MCCB断路器型号
                /// </summary>
                /// <returns></returns>
                private string cvtRTwentyTwo()
                {
                        string breaker=null;
                        string[] array = new string[] { "N", "E", "H", "R" };
                        breaker += array[source[0] & 0x3];
                        int p = (source[0]>> 2) & 0x3;
                        breaker += " " + (p==2?"4P" : "3P");
                        int z = (source[0]>> 4) & 0x3;
                        if (z == 3)
                        {
                                breaker += " " + "ZSI";
                        }
                        int g = (source[0]>> 6) & 0x3;
                        if (g == 3)
                        {
                                breaker += " " + "G";
                        }
                        int p1 = source[1] & 0x3;
                        if (z == 3)
                        {
                                breaker += " " + "P";
                        }

                        return breaker;
                }

                private string cvtRTwentyThree()
                {
                        return "V1.0";
                }
                #endregion MCCB

                #region ATS
                string cvtRThirty()
                {
                        return string.Format("{0} {1}", (char)source[0], (char)source[1]);
                }

                string cvtRThirtyOne()
                {
                        return Tool.isOne(source[1], 6) ? "Remote" : "Local";
                }

                string cvtRThirtyTwo()
                {
                        return cvtROne();
                }
                byte[] cvtWThirtyTwo(string value, string _extra)
                {
                        return cvtWOne(value, _extra);
                }

                string cvtRThirtyThree()
                {
                        int value = source[0] * 256 + source[1];
                        double factor1 = double.Parse(extra.Split('_')[0]);
                        return (value / factor1).ToString("0.0");
                }
                byte[] cvtWThirtyThree(string value, string _extra)
                {
                        return cvtWOne(value, _extra);
                }
                #endregion
        }
}
