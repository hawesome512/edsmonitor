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
        /// ATSRemote.xaml 的交互逻辑
        /// </summary>
        public partial class ATSRemote : UserControl
        {
                string command, cmdAlias;
                Device device;
                public ATSRemote()
                {
                        InitializeComponent();
                }

                public void InitDevice(Device device)
                {
                        this.DataContext = device;
                        this.device = device;
                        img_state.SetBinding(Image.SourceProperty, Tool.addBinding("State", new StateToATSImageSourceConverter()));
                }

                private void btn_ton_Click(object sender, RoutedEventArgs e)
                {
                        command = "N";
                        cmdAlias = "投常";
                        remote();
                }

                private void btn_off_Click(object sender, RoutedEventArgs e)
                {
                        command = "Open";
                        cmdAlias = "双分";
                        remote();
                }

                private void btn_tos_Click(object sender, RoutedEventArgs e)
                {
                        command = "S";
                        cmdAlias = "投备";
                        remote();
                }

                void remote()
                {
                        var result = device.RemoteControl(command);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                                if (result.Length == 1)
                                {
                                        MsgBox.Show(string.Format("{0} 处于 {1}状态.", device.Name, cmdAlias), "成功", MsgBox.Buttons.OK, MsgBox.Icons.Shield);
                                }
                                else
                                {
                                        MsgBox.Show(string.Format("{0} {1}操作失败.", device.Name, command), "失败", MsgBox.Buttons.OK, MsgBox.Icons.Error);
                                }
                        }));

                }
        }
}
