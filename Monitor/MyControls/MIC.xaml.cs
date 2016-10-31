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
        /// MIC.xaml 的交互逻辑
        /// </summary>
        public partial class MIC : UserControl
        {
                public MIC()
                {
                        InitializeComponent();
                }

                public void InitMIC(Device device,bool showImage)
                {
                        breakerMenu.InitMenu(device);
                        List<string> sources = Tool.GetDeviceDependence(device);
                        this.DataContext = device.Dependence;
                        line_1.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_2.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_3.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_4.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        line_2.SetBinding(Line.OpacityProperty, Tool.addBinding("[0].State", new StateToCloseConverter()));
                        if (showImage)
                        {
                                micImage.Visibility = Visibility.Visible;
                                micRect.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                                micImage.Visibility = Visibility.Hidden;
                                micRect.Visibility = Visibility.Visible;
                        }
                }

                private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                {
                        breakerMenu.menu.IsOpen = true;
                }
        }
}
