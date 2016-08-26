using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChangeEDSLot;

namespace ChangeEDSLot
{
        class Program
        {
                static void Main(string[] args)
                {
                        Task.Factory.StartNew(new Action(() =>
                        {
                                using (EDSEntities context = new EDSEntities())
                                {
                                        foreach (var a in context.Trip)
                                        {
                                        }
                                }
                                Console.WriteLine("Done");
                        }));
                        Console.ReadLine();
                }
        }
}
