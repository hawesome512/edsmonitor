using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDSLot;
using System.Xml;

namespace Monitor
{
        public class DataLib
        {
                public static List<EDSLot.Energy> QueryEnergy(int[] addrs, DateTime start, DateTime end)
                {
                        if (!Common.IsServer)
                        {
                                return Common.ZoneDevices.First().Value.First().MyCom.QueryEnergy(addrs, start, end);
                        }
                        else
                        {
                                using (EDSEntities context = new EDSEntities())
                                {
                                        List<EDSLot.Energy> dataList;
                                        bool shortTime = (end - start).TotalHours <= 24;
                                        int selAddr = addrs[0];
                                        var result = (from x in context.Energy
                                                      where x.Address == selAddr && x.Time >= start && x.Time < end
                                                      select x).ToList();
                                        if (shortTime)
                                        {
                                                dataList = result.GroupBy(y => (y.Time.Hour)).Select(g => new EDSLot.Energy()
                                                {
                                                        Address = selAddr,
                                                        Time = g.First().Time,
                                                        PE = g.Sum(z => z.PE)
                                                }).ToList();
                                        }
                                        else
                                        {
                                                dataList = result.GroupBy(y => (y.Time.Date)).Select(g => new EDSLot.Energy()
                                                {
                                                        Address = selAddr,
                                                        Time = g.First().Time,
                                                        PE = g.Sum(z => z.PE)
                                                }).ToList();
                                        }

                                        DateTime start1 = start.AddMonths(-1);
                                        DateTime end1 = end.AddMonths(-1);
                                        addLastData(context, dataList, selAddr, start1, end1);
                                        start1 = start.AddYears(-1);
                                        end1 = end.AddYears(-1);
                                        addLastData(context, dataList, selAddr, start1, end1);
                                        addChildrenData(context, dataList, addrs, start, end);
                                        return dataList;
                                }
                        }
                }

                private static void addLastData(EDSEntities context, List<EDSLot.Energy> dataList, int selAddr, DateTime start, DateTime end)
                {
                        double? lastMonth = (from x in context.Energy
                                             where x.Address == selAddr && x.Time >= start && x.Time < end
                                             select x).Sum(n => n.PE);
                        dataList.Add(new EDSLot.Energy()
                        {
                                Address = selAddr,
                                Time = start,
                                PE = lastMonth
                        });
                }

                private static void addChildrenData(EDSLot.EDSEntities context, List<EDSLot.Energy> dataList,int[] addrs,DateTime start,DateTime end)
                {
                        List<string> children = new List<string>();
                        List<double?> childValues = new List<double?>();
                        for(int i=1;i<addrs.Length;i++)
                        {
                                int addr = addrs[i];
                                var result = (from x in context.Energy
                                              where x.Address == addr && x.Time >= start && x.Time < end
                                              select x);
                                dataList.Add(new EDSLot.Energy()
                                {
                                        Address = addr,
                                        Time = start,
                                        PE = result.Sum(r => r.PE)
                                });
                        }    
                }
        }
}
