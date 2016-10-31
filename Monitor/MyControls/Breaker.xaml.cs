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

                public void InitBreaker(Device device,bool showImg)
                {
                        breakerMenu.InitMenu(device);
                        this.DataContext = device.Dependence;
                        var lines = initLineArray();
                        List<string> sources = Tool.GetDeviceDependence(device);
                        Triangle.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        if (showImg)
                        {
                                Binding binding = new Binding("[0].State");
                                binding.Converter = new StateToBreakerImageSource();
                                binding.ConverterParameter = device.Name;
                                breakerImg.SetBinding(Image.SourceProperty,binding);
                                breakerImg.Visibility = Visibility.Visible;
                                breakerShow.Visibility = Visibility.Hidden;
                                for (int i = 0; i < 2; i++)
                                {
                                        Line line = Tool.FindChild<Line>(MyBreaker, lines[i]);
                                        line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                                }
                        }
                        else
                        {
                                breakerShow.Visibility = Visibility.Visible;
                                breakerImg.Visibility = Visibility.Hidden;
                                foreach (string name in lines)
                                {
                                        Line line = Tool.FindChild<Line>(MyBreaker, name);
                                        line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                                }
                                Switch_open.SetBinding(Line.OpacityProperty, Tool.addBinding("[0].State", new StateToOpenConverter()));
                                Switch_open.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                                Switch_close.SetBinding(Line.OpacityProperty, Tool.addBinding("[0].State", new StateToCloseConverter()));
                                Switch_close.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                                Ia.SetBinding(Label.ContentProperty, Tool.addBinding("[0].State.Ia", new DoubleToInt()));
                                Ib.SetBinding(Label.ContentProperty, Tool.addBinding("[0].State.Ib", new DoubleToInt()));
                                Ic.SetBinding(Label.ContentProperty, Tool.addBinding("[0].State.Ic", new DoubleToInt()));

                                Binding binding = new Binding()
                                {
                                        Source = Switch_close,
                                        Path = new PropertyPath("Stroke"),
                                        Converter = new StrokeToOpacityConverter()
                                };
                                grid_current.SetBinding(Line.OpacityProperty, binding);
                        }
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

                private void grid_current_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                {
                        breakerMenu.menu.IsOpen = true;
                }
        }
}
