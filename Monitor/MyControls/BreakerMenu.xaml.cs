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
        /// BreakerMenu.xaml 的交互逻辑
        /// </summary>
        public partial class BreakerMenu : UserControl
        {
                Device myDevice;
                public event EventHandler<EnterDeviceArgs> EnterDevice;
                public BreakerMenu()
                {
                        InitializeComponent();
                }

                public void InitMenu(Device device)
                {
                        myDevice = device;
                        this.DataContext=device;
                        close.SetBinding(RadialMenu.Controls.RadialMenuItem.IsEnabledProperty, Tool.addBinding(".State", new  ControlToBtnEnableConverter()));
                        open.SetBinding(RadialMenu.Controls.RadialMenuItem.IsEnabledProperty, Tool.addBinding(".State", new ControlToBtnEnableConverter()));
                }

                private void RadialMenuCentralItem_Click(object sender, RoutedEventArgs e)
                {
                        menu.IsOpen = false;
                }

                private void menu_MouseLeave(object sender, MouseEventArgs e)
                {
                        menu.IsOpen = false;
                }

                private void open_Click(object sender, RoutedEventArgs e)
                {
                        myDevice.RemoteControl("Open");
                        menu.IsOpen = false;
                }

                private void detail_Click(object sender, RoutedEventArgs e)
                {
                        EnterDevice(this, new EnterDeviceArgs(myDevice));
                        menu.IsOpen = false;
                }

                private void plan_Click(object sender, RoutedEventArgs e)
                {
                        Plan plan = new Plan();
                        plan.InitPlan(myDevice);
                        plan.Show();
                        menu.IsOpen = false;
                }

                private void close_Click(object sender, RoutedEventArgs e)
                {
                        myDevice.RemoteControl("Close");
                        menu.IsOpen = false;
                }
        }
}
