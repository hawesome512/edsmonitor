using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Threading;

namespace Monitor
{
        /// <summary>
        /// MainWindow.xaml 的交互逻辑
        /// </summary>
        public partial class MainWindow : Window
        {
                Com com;
                Common common;
                List<Device> devices = null;
                List<UIElement> mainContainer = null;
                Thread threadRun = null;
                public MainWindow()
                {
                        InitializeComponent();

                        intitControls();
                        Thread t = new Thread(() =>
                        {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                        initDiagram();
                                }));
                        });
                        t.Start();
                }

                private void intitControls()
                {
                        if (Tool.GetConfig("ComType") == "SP")
                        {
                                Common.ComType = ComType.SP;
                        }
                        else
                        {
                                Common.ComType = ComType.TCP;
                        }
                        common = new Common();
                        com = new Com(Common.ComType);

                        Binding bind = new Binding();
                        bind.Source = dvPage;
                        bind.Path = new PropertyPath("IsVisible");
                        btn_back.SetBinding(ImageButton.IsEnabledProperty, bind);
                        btn_back.Click += new RoutedEventHandler(btn_back_Click);

                        bind = new Binding();
                        bind.Source = dvPage;
                        bind.Path = new PropertyPath("IsVisible");
                        bind.Converter = new BooleanToVisibilityConverter();
                        border_device.SetBinding(Border.VisibilityProperty, bind);

                        bind = new Binding();
                        bind.Source = common;
                        bind.Path = new PropertyPath("UserLevel");
                        txt_curuser.SetBinding(TextBlock.TextProperty, bind);

                        bind = new Binding();
                        bind.Source = common;
                        bind.Path = new PropertyPath("UserLevel");
                        bind.Converter = new UserToAccountText();
                        txt_account.SetBinding(TextBlock.TextProperty, bind);

                        dvDevices.ReloadDevices += new EventHandler<EventArgs>(dvDevices_ReloadDevices);

                        loginPage.LoginSucceeded += loginPage_LoginSucceeded;

                        mainContainer = new List<UIElement>() { diagram, dvPage, dvDevices, img_link, loginPage,energy };
                }

                void loginPage_LoginSucceeded(object sender, EventArgs e)
                {
                        common.UserLevel = User.ADMIN;
                        btn_run_Click(btn_run, null);
                }

                void dvDevices_ReloadDevices(object sender, EventArgs e)
                {
                        com.Dispose();
                        com = new Com(Common.ComType);
                        initDiagram();
                        btn_run_Click(btn_run, new RoutedEventArgs());
                }

                private void initDiagram()
                {
                        if (threadRun != null)
                                threadRun.Suspend();
                        loadDevices();
                        if (Common.ComType == ComType.SP)
                        {
                                testComs();
                        }
                        diagram.InitDiagram(devices);
                        diagram.EnterDevice += new EventHandler<EnterDeviceArgs>(diagram_EnterDevice);
                        threadRun = new Thread(() =>
                        {
                                while (true)
                                {
                                        foreach (var dv in devices)
                                        {
                                                dv.GetData();
                                                if (Common.SelectedAddress != -1 && Common.SelectedAddress != dv.Address)
                                                {
                                                        Device dvSelected= (Device)devices.Find(d => d.Address == Common.SelectedAddress);
                                                        dvSelected.GetData();
                                                }
                                        }
                                }
                        });
                        threadRun.IsBackground = true;
                        threadRun.Start();
                }

                void btn_back_Click(object sender, RoutedEventArgs e)
                {
                        setMainVisibility(diagram);
                }

                void diagram_EnterDevice(object sender, EnterDeviceArgs e)
                {
                        setMainVisibility(dvPage);
                        dvPage.InitDevice(e.Dv, common);
                        Common.SelectedAddress = dvPage.GetAddr();
                }


                void loadDevices()
                {
                        devices = new List<Device>();
                        XmlElement xeR = Tool.GetXML(@"DevicesConfig/DeviceList.xml");
                        for (int i = 0; i < xeR.ChildNodes.Count;i++ )
                        {
                                string type = xeR.SelectNodes("//Type")[i].InnerText;
                                string name = xeR.SelectNodes("//Name")[i].InnerText;
                                byte addr = byte.Parse(xeR.SelectNodes("//Address")[i].InnerText);
                                string tag = xeR.SelectNodes("//Tag")[i].InnerText;
                                byte parent = byte.Parse(xeR.SelectNodes("//Parent")[i].InnerText);
                                DeviceType dvType = (DeviceType)Enum.Parse(typeof(DeviceType), type);
                                Device device = Activator.CreateInstance(Type.GetType("Monitor.Dv" + type)) as Device;
                                device.InitDevice(addr, dvType, name, tag, parent);
                                device.MyCom = com;
                                device.PreRemoteModify += (s, o) =>
                                {
                                        threadRun.Suspend();
                                };
                                device.RemoteModified += (s, o) =>
                                {
                                        threadRun.Resume();
                                };
                                device.InitAddress();
                                devices.Add(device);
                        }
                        foreach (Device dv in devices)
                        {
                                dv.Dependence = Tool.FindParents(devices, dv.Address);
                        }
                }

                void testComs()
                {
                        foreach (var d in devices)
                        {
                                string resultCom = com.TestCom(d.Address, Tool.GetConfig("Com"));
                                if (resultCom != null)
                                {
                                        Tool.SetConfig("Com", resultCom);
                                        return;
                                }
                        }
                        MsgBox.Show("请检查串口连接和设备通信地址是否正确.", "无信号", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                }

                private void btn_close_Click(object sender, System.Windows.RoutedEventArgs e)
                {
                        // 在此处添加事件处理程序实现。
                        this.Close();
                }

                private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                {
                        this.DragMove();
                }

                private void btn_devices_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        dvDevices.InitDataGrid(devices, common);
                        setMainVisibility(dvDevices);
                }

                private void btn_run_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        setMainVisibility(diagram);
                }

                private void btn_link_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        setMainVisibility(img_link);
                }

                private void btn_energy_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        setMainVisibility(energy);
                }

                private void btn_account_Click(object sender, RoutedEventArgs e)
                {
                        if (common.UserLevel == User.ADMIN)
                        {
                                common.UserLevel = User.UNKNOWN;
                                return;
                        }
                        setHerePosition(sender);
                        setMainVisibility(loginPage);
                }

                void setHerePosition(Object sender)
                {
                        img_here.Margin = (sender as ImageButton).Margin;
                        img_here.HorizontalAlignment = (sender as ImageButton).HorizontalAlignment;
                }

                void setMainVisibility(UIElement ui)
                {
                        foreach (UIElement u in mainContainer)
                        {
                                u.Visibility = (u == ui) ? Visibility.Visible : Visibility.Hidden;
                        }
                        Common.SelectedAddress = -1;
                }

                private void btn_min_Click(object sender, RoutedEventArgs e)
                {
                        this.WindowState = WindowState.Minimized;
                }

                private void btn_size_Click(object sender, RoutedEventArgs e)
                {
                        if (this.WindowState == WindowState.Normal)
                        {
                                this.WindowState = WindowState.Maximized;
                        }
                        else
                        {
                                this.WindowState = WindowState.Normal;
                        }
                }

                private void btn_set_Click(object sender, RoutedEventArgs e)
                {
                        this.menu.IsOpen = true;
                }

                private void help_Click(object sender, RoutedEventArgs e)
                {
                        System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + "MonitorHelp.pdf");
                }

                private void psw_Click(object sender, RoutedEventArgs e)
                {
                        Account account = new Account();
                        account.Height = 400;
                        account.Width = 360;
                        account.ShowDialog();
                }

                private void feedback_Click(object sender, RoutedEventArgs e)
                {
                        MsgBox.Show("Tel：0592-610-0660-322(分机号)\r\nEmail：haisheng.xu@xseec.cn", "联系方式", MsgBox.Buttons.OK, MsgBox.Icon.Info, MsgBox.AnimateStyle.FadeIn);
                }

                private void about_Click(object sender, RoutedEventArgs e)
                {
                        System.Diagnostics.Process.Start("http://www.xseec.cn/cn/index.asp");
                }
        }
}
