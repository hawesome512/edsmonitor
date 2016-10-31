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
        /// ATS.xaml 的交互逻辑
        /// </summary>
        public partial class ATS : UserControl
        {
                public ATS()
                {
                        InitializeComponent();
                }

                public void InitATS(Device device,bool showImage)
                {
                        if (showImage)
                        {
                                ATSImage.Visibility = Visibility.Visible;
                                ATSRect.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                                ATSImage.Visibility = Visibility.Hidden;
                                ATSRect.Visibility = Visibility.Visible;
                                this.DataContext = device.Dependence;
                                Binding binding = new Binding("[0].State");
                                binding.Converter = new StateToATSConverter();
                                ATS_State.SetBinding(Line.X2Property, binding);
                        }
                }
        }
}
