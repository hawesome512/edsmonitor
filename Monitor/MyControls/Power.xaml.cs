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
using System.Windows.Media.Animation;
using System.IO.Ports;
using System.Threading;

namespace Monitor
{
        /// <summary>
        /// Power.xaml 的交互逻辑
        /// </summary>
        public partial class Power : UserControl
        {
                string[] stateArray = new string[] { "未知", "关机", "测试中", "UPS后备式", "UPS故障", "旁路模式", "电池电压低", "市电异常" };
                Timer timer;
                DateTime lastSmsAlarmTime;
                public static string UpsState;
                public Power()
                {
                        InitializeComponent();
                }

                /// <summary>
                /// 初始化，系统没有UPS时不调用
                /// </summary>
                public void InitPower()
                {
                        bool hasUps = false;
                        if (Common.IsServer)
                        {
                                hasUps = Tool.GetConfig("UPS") == "true";
                                if (!hasUps)
                                {
                                        UpsState = "no ups";
                                }
                        }
                        else
                        {
                                UpsState = getUpsState();
                                hasUps = UpsState != "no ups";
                        }
                        if (hasUps)
                        {
                                timer = new Timer((s) =>
                                {
                                        updateUps();
                                }, null, 0, 1000 * 60);
                        }
                        else
                        {
                                this.Visibility = Visibility.Hidden;
                        }
                }

                void updateUps()
                {
                        UpsState = getUpsState();
                        if (string.IsNullOrEmpty(UpsState))
                        {
                                MsgBox.Show("请查看UPS设置是否正确；\r\n若无UPS请修改[系统设置]→[供电系统]，修改后重启生效", "UPS通信失败", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                                timer.Dispose();
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                        this.Visibility = Visibility.Hidden;
                                }));
                                return;
                        }
                        UpsState = UpsState.Substring(1, UpsState.Length - 2);
                        var states = UpsState.Split(' ');
                        int nState = int.Parse(states[7]);
                        string strState = null;
                        for (int i = 0; i < stateArray.Length; i++)
                        {
                                if (Tool.isOne(nState, i))
                                {
                                        strState += stateArray[i] + " ";
                                }
                        }
                        string packUri = null;
                        string strColor = "#009688";
                        if (strState == null)
                        {
                                packUri = "pack://application:,,,/Monitor;component/Images/power_on.png";
                                strState = "正常";
                        }
                        else
                        {
                                packUri = "pack://application:,,,/Monitor;component/Images/power_off.png";
                                strColor = "#f13737";
                                if (lastSmsAlarmTime == null || (DateTime.Now - lastSmsAlarmTime).TotalHours > 1)
                                {
                                        Common.SmsAlarm.SendSms("UPS", strState);
                                        lastSmsAlarmTime = DateTime.Now;
                                }
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                                img_power.Source = (new ImageSourceConverter().ConvertFromString(packUri) as ImageSource);
                                txt_power.Content = string.Format("输入电压:{0}V  输入频率:{1}Hz  输出电压:{2}V  负载:{3}%  温度:{4}℃  UPS状态:{5}", states[0], states[4], states[2], states[3], states[6], strState);
                                txt_power.Background = new BrushConverter().ConvertFromString(strColor) as SolidColorBrush;
                        }));
                }

                string getUpsState()
                {
                        if (Common.IsServer)
                        {
                                using (SerialPort sp = new SerialPort(Tool.GetConfig("UPSCom")))
                                {
                                        sp.BaudRate = 2400;
                                        byte[] snd = new byte[] { 0x51, 0x31, 0x0d };
                                        sp.Open();
                                        sp.Write(snd, 0, snd.Length);
                                        System.Threading.Thread.Sleep(500);
                                        return sp.ReadExisting();
                                }
                        }
                        else
                        {
                                ServiceReference1.EDSServiceClient wcf = new ServiceReference1.EDSServiceClient();
                                string upsState = wcf.GetUpsState();
                                wcf.Close();
                                return upsState;
                        }
                }

                private void img_power_MouseEnter(object sender, MouseEventArgs e)
                {
                        DoubleAnimation da = new DoubleAnimation(32, 600, new Duration(TimeSpan.FromSeconds(0.5)));
                        this.BeginAnimation(UserControl.WidthProperty, da);
                }

                private void img_power_MouseLeave(object sender, MouseEventArgs e)
                {
                        DoubleAnimation da = new DoubleAnimation(600, 32, new Duration(TimeSpan.FromSeconds(0.5)));
                        this.BeginAnimation(UserControl.WidthProperty, da);

                }
        }
}
