using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDSLot;
using System.Net;
using System.IO;

namespace GenerateEDSData
{
        class Program
        {
                static EDSEntities context;
                static void Main(string[] args)
                {
                        context = new EDSEntities();
                        //DateTime start=DateTime.Today;
                        //DateTime end = start.AddDays(1);
                        //var result=(from e in context.Energy where e.Address==1&&e.Time>=start&&e.Time<end select e).ToList();
                        //var r = result.GroupBy(x => x.Time.Hour).Select(g => g.Sum(x => x.PE)).ToList();
                        Task.Factory.StartNew(new Action(() =>
                        {
                                DateTime start = new DateTime(2015, 1, 1);
                                DateTime end = new DateTime(2017, 1, 1);
                                for (DateTime t = start; t < end; t = t.AddDays(1))
                                {
                                        setDayData(t);
                                        Console.WriteLine(t.ToShortDateString());
                                }
                        }));
                        Console.ReadLine();
                }


                static void setDayData(DateTime time)
                {
                        context = new EDSEntities();
                        int hol = request(time);
                        double dHol = hol == 2 ? 0.2 : (hol == 1 ? 0.33 : 1);
                        Random rnd = new Random();
                        for (DateTime t = time; t < time.AddDays(1); t = t.AddMinutes(15))
                        {
                                double sum = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                        double peSum = 0;
                                        for (int j = 0; j < 4; j++)
                                        {
                                                double max = j == 2 ? dHol * 6.25 : dHol * 3.75;
                                                double max1 = max * daytime(time, t);
                                                double pe = Math.Round(rnd.Next(60, 100) / 100.0 * max1, 2);
                                                peSum += pe;
                                                context.Energy.Add(new Energy()
                                                {
                                                        Address = i * 5 + j + 2,
                                                        Time = t,
                                                        PE = pe
                                                });
                                        }
                                        double pe1 = Math.Round(peSum, 2);
                                        sum += pe1;
                                        context.Energy.Add(new Energy()
                                        {
                                                Address = i * 5 + 1,
                                                Time = t,
                                                PE = pe1
                                        });
                                }
                                context.Energy.Add(new Energy()
                                {
                                        Address = 0,
                                        Time = t,
                                        PE = sum
                                });
                        }
                        try
                        {
                                context.SaveChanges();
                        }
                        catch
                        {

                        }
                }

                static double daytime(DateTime date, DateTime time)
                {
                        if (time <= date.AddHours(7.5) || time >= date.AddHours(17.5))
                        {
                                return 0.3;
                        }
                        else if ((time >= date.AddHours(8.5) && time <= date.AddHours(11.5)) || (time >= date.AddHours(13.5) && time <= date.AddHours(16.5)))
                        {
                                return 1;
                        }
                        else
                        {
                                return 0.8;
                        }
                }

                /// <summary>
                /// 发送HTTP请求
                /// </summary>
                /// <param name="url">请求的URL</param>
                /// <param name="param">请求的参数</param>
                /// <returns>请求结果</returns>
                public static int request(DateTime date)
                {
                        string strURL = "http://apis.baidu.com/xiaogg/holiday/holiday?d=" + date.ToString("yyyyMMdd");
                        System.Net.HttpWebRequest request;
                        request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
                        request.Method = "GET";
                        // 添加header
                        request.Headers.Add("apikey", "1c7b90c856a0a68c505591a57e4bdaea");
                        System.Net.HttpWebResponse response;
                        response = (System.Net.HttpWebResponse)request.GetResponse();
                        System.IO.Stream s;
                        s = response.GetResponseStream();
                        string StrDate = "";
                        string strValue = "";
                        StreamReader Reader = new StreamReader(s, Encoding.UTF8);
                        while ((StrDate = Reader.ReadLine()) != null)
                        {
                                strValue += StrDate + "\r\n";
                        }
                        return int.Parse(strValue.Substring(0, 1));
                }
        }
}
