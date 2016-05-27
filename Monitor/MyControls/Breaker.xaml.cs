using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Expression.Shapes;

namespace Monitor
{
        /// <summary>
        /// breaker.xaml 的交互逻辑
        /// </summary>
        public partial class Breaker : UserControl
        {
                public Breaker()
                {
                        InitializeComponent();
                }

                public void InitBreaker(Device device)
                {
                        this.DataContext = device.Dependence;
                        var lines=initLineArray();
                        List<string> sources = new List<string>();
                        for (int i = 0; i < device.Dependence.Count; i++)
                        {
                                sources.Add(string.Format("[{0}].State", i));
                        }
                        foreach (string name in lines)
                        {
                                Line line=Tool.FindChild<Line>(MyBreaker,name);
                                line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        }
                        Triangle.SetBinding(RegularPolygon.StrokeProperty, Tool.addMulBinding(sources));
                        Switch_open.SetBinding(Line.OpacityProperty, Tool.addBinding("[0].State", new StateToOpenConverter()));
                        Switch_close.SetBinding(Line.OpacityProperty, Tool.addBinding("[0].State", new StateToCloseConverter()));
                        Switch_close.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                }



                private List<string> initLineArray()
                {
                        List<string> lines = new List<string>();
                        for (int i = 0; i <= 13; i++)
                        {
                                lines.Add("line_" + i);
                        }
                        return lines;
                }
        }
}
