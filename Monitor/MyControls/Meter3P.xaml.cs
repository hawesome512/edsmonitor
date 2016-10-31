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

namespace Monitor
{
        /// <summary>
        /// EnergyMeter.xaml 的交互逻辑
        /// </summary>
        public partial class Meter3P : UserControl
        {
                string[] items = new string[] { "Ia", "Ib", "Ic", "Ua", "Ub", "Uc", "P", "Q", "FR", "PF", "PE" };
                public Meter3P()
                {
                        InitializeComponent();
                        initCtrls();
                }

                public void InitMeter3P(Device device)
                {
                        List<string> sources = Tool.GetDeviceDependence(device);
                        sources.RemoveAt(0);
                        this.DataContext = device.Dependence;
                        line_1.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_2.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_3.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_4.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_5.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));

                        for(int i=0;i<items.Length;i++)
                        {
                                Label label=Tool.FindChild<Label>(MeterGrid, items[i]);
                                Binding binding = new Binding(string.Format("[0].State.{0}",items[i]));
                                label.SetBinding(Label.ContentProperty, binding);
                                //label.SetBinding(Label.ContentProperty, Tool.addBinding(string.Format("[0].State.{0}",items[i]), new DoubleToInt()));
                        }
                }

                void initCtrls()
                {
                        string[] units = new string[] {"A","A","A","V","V","V","kW","kVar","Hz","","kWh"};
                        Brush[] brushes = new Brush[] { Brushes.Orange, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Pink, Brushes.Brown, Brushes.Red, };
                        for (int i = 0; i < items.Length; i++)
                        {
                                addText(10, items[i], i + 2, Brushes.Black);
                                Label label=addText(50, "0.0", i + 2, brushes[i]);
                                label.Name = items[i];
                                addText(100, units[i], i + 2, Brushes.Black);
                        }

                }

                Label addText(double left, String text, int row, Brush brush)
                {
                        Label label=new Label();
                        label.Foreground=brush;
                        label.Content=text;
                        label.HorizontalAlignment=HorizontalAlignment.Left;
                        label.Margin=new Thickness(left,0,0,0);
                        Grid.SetRow(label, row);
                        MeterGrid.Children.Add(label);
                        return label;
                }
        }
}
