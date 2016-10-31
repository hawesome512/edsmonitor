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
        /// MICRemote.xaml 的交互逻辑
        /// </summary>
        public partial class MICRemote : UserControl
        {
                string command,alias;
                Device device;
                public MICRemote()
                {
                        InitializeComponent();
                }

                public void InitDevice(Device device)
                {
                        this.DataContext = device;
                        this.device = device;
                }

                private void btn_startB_Click(object sender, RoutedEventArgs e)
                {
                        command = "StartB";
                        alias = "起动B";
                        remote();
                }

                private void btn_startA_Click(object sender, RoutedEventArgs e)
                {
                        command = "StartA";
                        alias = "起动A";
                        remote();
                }

                private void btn_reset_Click(object sender, RoutedEventArgs e)
                {
                        command = "Reset";
                        alias = "复位";
                        remote();
                }

                private void btn_stop_Click(object sender, RoutedEventArgs e)
                {
                        command = "Stop";
                        alias = "停止";
                        remote();
                }

                private void btn_cleanQ_Click(object sender, RoutedEventArgs e)
                {
                        command = "CleanQ";
                        alias = "热容快速清除";
                        remote();
                }

                void remote()
                {
                        var result = device.RemoteControl(command);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                                if (result.Length == 1)
                                {
                                        MsgBox.Show(string.Format("{0}  {1}操作完成.", device.Name, alias), "Succeed", MsgBox.Buttons.OK, MsgBox.Icons.Shield);
                                }
                                else
                                {
                                        MsgBox.Show(string.Format("{0} {1}操作失败.", device.Name, alias), "Fail", MsgBox.Buttons.OK, MsgBox.Icons.Error);
                                }
                        }));
                }
        }
}
