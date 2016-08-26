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
using System.Threading.Tasks;
using System.ServiceModel;

namespace Monitor
{
        /// <summary>
        /// MainWindow.xaml 的交互逻辑
        /// </summary>
        public partial class MainWindow : Window
        {
                Common common;
                List<UIElement> mainContainer = null;
                CancellationTokenSource cancelSelToken, cancelOrdToken;
                ServiceHost host;
                public MainWindow()
                {
                        InitializeComponent();
                        init();
                }

                private void init()
                {
                        initOthers();
                        initControls();
                }

                private void initOthers()
                {
                        common = new Common();
                        if (Common.IsServer)
                        {
                                host = new ServiceHost(typeof(EDSService));
                                host.Open();
                        }

                        loadDevices();
                        collectOrdDevicesData();
                }

                private void collectOrdDevicesData()
                {
                        Task.Factory.StartNew(() =>
                        {
                                cancelOrdToken = new CancellationTokenSource();
                                ParallelOptions option = new ParallelOptions();
                                option.CancellationToken = cancelOrdToken.Token;
                                if (!Common.IsServer)
                                {
                                        return;
                                }
                                while (true)
                                {
                                        foreach (var dv in Common.OrdDevices)
                                        {
                                                option.CancellationToken.ThrowIfCancellationRequested();
                                                dv.GetData();
                                        }
                                }
                        });
                }

                private void collectSelDevicesData()
                {
                        Task.Factory.StartNew(() =>
                        {
                                cancelSelToken = new CancellationTokenSource();
                                ParallelOptions option = new ParallelOptions();
                                option.CancellationToken = cancelSelToken.Token;
                                while (true)
                                {
                                        foreach (var dv in Common.SelDevices)
                                        {
                                                option.CancellationToken.ThrowIfCancellationRequested();
                                                dv.GetData();
                                                if (Common.SelectedAddress != 0 && Common.SelectedAddress != dv.Address)
                                                {
                                                        Device dvSelected = (Device)Common.SelDevices.Find(d => d.Address == Common.SelectedAddress);
                                                        dvSelected.GetData();
                                                }
                                        }
                                }
                        });
                }

                private void initControls()
                {
                        Binding bind = new Binding();
                        bind.Source = dvPage;
                        bind.Path = new PropertyPath("IsVisible");

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

                        network.EnterDevice += new EventHandler<EnterDeviceArgs>(diagram_EnterDevice);
                        startPage.Enter += new EventHandler<StartEventArgs>(start_Enter);

                        mainContainer = new List<UIElement>() { network, dvPage, img_link, energy };

                        power.InitPower();
                }

                void start_Enter(object sender, StartEventArgs e)
                {
                        tg_back.IsChecked = true;
                        tg_back.IsEnabled = true;
                        btn_size.IsEnabled = true;
                        cancelOrdToken.Cancel();
                        Common.OrdDevices.First().MyCom.ChangeSelZone(e.ZID);
                        network.InitNetwork(Common.SelDevices);
                        btn_run_Click(btn_run, null);
                        collectOrdDevicesData();
                        collectSelDevicesData();
                        progress.start();
                        Task.Factory.StartNew(new Action(() =>
                        {
                                while (true)
                                {
                                        Thread.Sleep(1000);
                                        if (Common.SelDevices.Last().HasInited)
                                        {
                                                progress.end();
                                                break;
                                        }
                                }
                        }));
                }

                void diagram_EnterDevice(object sender, EnterDeviceArgs e)
                {
                        setMainVisibility(dvPage);
                        dvPage.InitDevice(e.Dv, common);
                        Common.SelDevices.First().MyCom.ChangeSelAddress(dvPage.GetAddr());
                }

                void loadDevices()
                {
                        Common.OrdDevices = new List<Device>();
                        XmlElement xeR = Tool.GetXML(@"Config/DeviceList.xml");
                        for (int z = 0; z < xeR.ChildNodes.Count; z++)
                        {
                                XmlElement zone = (XmlElement)xeR.ChildNodes[z];
                                byte zid = byte.Parse(zone.GetAttribute("ZID"));
                                ComType cType = ComType.WCF;
                                string tag = "";
                                if (Common.IsServer)
                                {
                                        string strCom = zone.GetAttribute("ComType");
                                        cType = (ComType)Enum.Parse(typeof(ComType), strCom);
                                        tag = zone.GetAttribute("Tag");
                                }
                                Com com = new Com(cType, tag);
                                for (int i = 0; i < zone.ChildNodes.Count; i++)
                                {
                                        string type = zone.ChildNodes[i].SelectSingleNode("Type").InnerText;
                                        string name = zone.ChildNodes[i].SelectSingleNode("Name").InnerText;
                                        string alias = zone.ChildNodes[i].SelectSingleNode("Alias").InnerText;
                                        byte addr = byte.Parse(zone.ChildNodes[i].SelectSingleNode("Address").InnerText);
                                        byte parent = byte.Parse(zone.ChildNodes[i].SelectSingleNode("Parent").InnerText);
                                        DeviceType dvType = (DeviceType)Enum.Parse(typeof(DeviceType), type);
                                        Device device = Activator.CreateInstance(Type.GetType("Monitor.Dv" + type)) as Device;
                                        device.InitDevice(zid, addr, dvType, name, alias, parent);
                                        device.PreRemoteModify += (s, o) =>
                                        {
                                                cancelSelToken.Cancel();
                                        };
                                        device.RemoteModified += (s, o) =>
                                        {
                                                collectSelDevicesData();
                                        };
                                        device.MyCom = com;
                                        device.InitAddress();
                                        Common.OrdDevices.Add(device);
                                }
                        }
                        Tool.FindDevicesParents();
                }

                private void btn_close_Click(object sender, System.Windows.RoutedEventArgs e)
                {
                        Environment.Exit(0);
                }

                private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                {
                        this.DragMove();
                }

                private void btn_devices_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        //dvDevices.InitDataGrid(devices, common);
                        //setMainVisibility(dvDevices);
                }

                private void btn_run_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        setMainVisibility(network);
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
                        Login login = new Login();
                        login.LoginSucceeded += (s, o) =>
                        {
                                common.UserLevel = User.ADMIN;
                        };
                        login.ShowDialog();
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
                        Common.OrdDevices.First().MyCom.ChangeSelAddress(0);
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

                private void system_Click(object sender, RoutedEventArgs e)
                {
                        Setting setting = new Setting();
                        setting.InitSetting(common);
                        setting.ShowDialog();
                }

                private void tg_back_Click(object sender, RoutedEventArgs e)
                {
                        cancelOrdToken.Cancel();
                        cancelSelToken.Cancel();
                        network.ClearNetwork();
                        Common.SelDevices.First().MyCom.ChangeSelZone(0);
                        collectOrdDevicesData();
                        setMainVisibility(null);
                        startPage.showStart();
                        tg_back.IsEnabled = false;
                        btn_size.IsEnabled = false;
                        this.WindowState = WindowState.Normal;
                }

        }
}
