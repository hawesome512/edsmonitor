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
using System.Windows.Shapes;
using System.Configuration;
using System.Xml;

namespace Monitor
{
        /// <summary>
        /// Setting.xaml 的交互逻辑
        /// </summary>
        public partial class Setting : Window
        {
                public Setting()
                {
                        InitializeComponent();
                }

                public void InitSetting(Common common)
                {
                        switch (Common.CType)
                        {
                                case ComType.SP:
                                        rb_sp.IsChecked = true;
                                        break;
                                case ComType.TCP:
                                        rb_tcp.IsChecked = true;
                                        break;
                                default:
                                        rb_wcf.IsChecked = true;
                                        break;
                        }
                        var ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
                        cbox_upsCom.ItemsSource = ports;
                        cbox_upsCom.SelectedIndex = ports.IndexOf(Tool.GetConfig("UPSCom"));
                        bool hasUps = Convert.ToBoolean(Tool.GetConfig("UPS"));
                        if (hasUps)
                        {
                                rb_ups.IsChecked = true;
                        }
                        else
                        {
                                rb_noUps.IsChecked = true;
                        }
                        txt_sql.Text = ConfigurationManager.ConnectionStrings[1].ConnectionString;
                        txt_tel.Text = Common.SmsAlarm.RecNum;
                        Binding bind = new Binding();
                        bind.Source = common;
                        bind.Path = new PropertyPath("UserLevel");
                        bind.Converter = new UserToBoolConverter();
                        btn_save.SetBinding(ImageButton.IsEnabledProperty, bind);
                }

                private void btn_close_Click(object sender, RoutedEventArgs e)
                {
                        this.Close();
                }

                private void btn_save_Click(object sender, RoutedEventArgs e)
                {
                        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        string comType = (bool)rb_sp.IsChecked ? "SP" : ((bool)rb_tcp.IsChecked ? "TCP" : "WCF");
                        cfa.AppSettings.Settings["ComType"].Value = comType;
                        cfa.AppSettings.Settings["Telephones"].Value = txt_tel.Text;
                        cfa.AppSettings.Settings["UPS"].Value = (bool)rb_ups.IsChecked ? "true" : "false";
                        cfa.AppSettings.Settings["UPSCom"].Value = (bool)rb_ups.IsChecked ? cbox_upsCom.SelectedValue.ToString() : string.Empty;
                        cfa.ConnectionStrings.ConnectionStrings[1].ConnectionString = txt_sql.Text;
                        cfa.Save();
                        this.Close();
                }
        }
}
