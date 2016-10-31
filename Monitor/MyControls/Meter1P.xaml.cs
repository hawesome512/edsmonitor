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
        /// Messure.xaml 的交互逻辑
        /// </summary>
        public partial class Meter1P : UserControl
        {
                public Meter1P()
                {
                        InitializeComponent();
                }

                public void InitMeter1P(Device device, bool showImage)
                {
                        this.DataContext = device.Dependence;
                        List<string> sources = Tool.GetDeviceDependence(device);
                        sources.RemoveAt(0);
                        for (int i = 0; i < 4; i++)
                        {
                                Shape shape = MeterRect.Children[i] as Shape;
                                shape.SetBinding(Shape.StrokeProperty, Tool.addMulBinding(sources));
                        }
                        if (showImage)
                        {
                                MeterImage.Visibility = Visibility.Visible;
                                MeterShow.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                                MeterImage.Visibility = Visibility.Hidden;
                                MeterShow.Visibility = Visibility.Visible;
                                I.SetBinding(Label.ContentProperty, new Binding("[0].State.Ia"));
                                U.SetBinding(Label.ContentProperty, new Binding("[0].State.Ua"));
                                PE.SetBinding(Label.ContentProperty, new Binding("[0].State.PE"));
                                FR.SetBinding(Label.ContentProperty, new Binding("[0].State.FR"));
                                PF.SetBinding(Label.ContentProperty, new Binding("[0].State.PF"));
                                P.SetBinding(Label.ContentProperty, new Binding("[0].State.P"));
                                Q.SetBinding(Label.ContentProperty, new Binding("[0].State.Q"));
                                QE.SetBinding(Label.ContentProperty, new Binding("[0].State.QE"));
                        }
                }
        }
}
