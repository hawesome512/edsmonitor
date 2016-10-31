using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Net;

namespace Monitor
{
        /// <summary>
        /// MainWindow.xaml 的交互逻辑
        /// </summary>
        public partial class MainWindow : Window
        {
                Common common;
                List<UIElement> mainContainer = null;
                ServiceHost host;
                public MainWindow()
                {
                        InitializeComponent();
                        initEDS();
                }

                private void initEDS()
                {
                        initOthers();
                        initControls();
                }

                private void initOthers()
                {
                        common = new Common();
                        loadDevices();

                        if (Common.IsServer)
                        {
                                try
                                {
                                        host = new ServiceHost(typeof(EDSService));
                                        host.Open();
                                        startDataTask();
                                }
                                catch
                                {
                                        MsgBox.Show("请检查服务器是否已启动或端口被占用！", "启动失败", MsgBox.Buttons.OK, MsgBox.Icons.Error);
                                }
                        }
                        else
                        {
                                startDataTask(true);
                        }
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
                        if (!Common.IsServer)
                        {
                                this.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/EDS;component/Images/BG_client.png")));
                        }
                        mainContainer = new List<UIElement>() { network, dvPage, img_link, energy, deviceList };
                        this.Closed += MainWindow_Closed;

                        power.InitPower();
                        checkUpdate();
                }

                private void startDataTask(bool once = false)
                {
                        foreach (byte zid in Common.ZoneDevices.Keys)
                        {
                                startDataTaskByZID(zid, once);
                        }
                }

                private void startDataTaskByZID(byte zid, bool once = false)
                {
                        Task.Factory.StartNew(new Action(() =>
                        {
                                Common.CancelTokens[zid] = new CancellationTokenSource();
                                //Common.CancelTokens.Add(zid, cancel);
                                ParallelOptions option = new ParallelOptions();
                                option.CancellationToken = Common.CancelTokens[zid].Token;
                                while (true)
                                {
                                        foreach (var device in Common.ZoneDevices[zid])
                                        {
                                                option.CancellationToken.ThrowIfCancellationRequested();
                                                device.GetData();
                                                var npDevices = Common.ZoneDevices[zid].FindAll(d => d.NeedParamsNum > 0);
                                                foreach (var d in npDevices)
                                                {
                                                        d.GetData();
                                                }
                                        }
                                        if (once)
                                        {
                                                break;
                                        }
                                }
                        }));
                }

                private void stopDataTaskByZID(byte zid)
                {
                        Common.CancelTokens[zid].Cancel();
                        //Common.CancelTokens.Remove(zid);
                }

                void checkUpdate()
                {
                        Task.Factory.StartNew(new Action(() =>
                        {
                                Version version = getVersion(Tool.GetConfig("LanUpdate") + @"/EDS最新版本号.txt");
                                bool isLan = true;
                                if (version == null)
                                {
                                        version = getVersion(Tool.GetConfig("WanUpdate") + @"/EDS最新版本号.txt");
                                        isLan = false;
                                }
                                Version now = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                                this.Dispatcher.Invoke(() =>
                                {
                                        if (version != null && version > now)
                                        {
                                                var result = MsgBox.Show(string.Format("发现可用更新，新版本为:{0}\r\n是否现在更新？", version.ToString()), "更新", MsgBox.Buttons.YesNo, MsgBox.Icons.Question);
                                                if (result == System.Windows.Forms.DialogResult.Yes)
                                                {
                                                        update(isLan, version.ToString());
                                                }
                                        }
                                });
                        }));
                }

                void update(bool isLan, string version)
                {
                        string address = isLan ? Tool.GetConfig("LanUpdate") : Tool.GetConfig("WanUpdate");
                        string dir = AppDomain.CurrentDomain.BaseDirectory;
                        if (dir.Contains(" "))
                        {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                        MsgBox.Show(string.Format("软件地址:{0}中存在空格，请删除空格后重试！", dir), "更新失败", MsgBox.Buttons.OK, MsgBox.Icons.Error);
                                }));
                        }
                        else
                        {
                                address = string.Format(@"{0}/EDS_{1}.rar", address, version);
                                string args = string.Format("{0} {1}", address, AppDomain.CurrentDomain.BaseDirectory);
                                System.Diagnostics.Process.Start(@"Update\Update.exe", args);
                        }
                        this.Close();
                }

                private static Version getVersion(string url)
                {
                        WebClient client = new WebClient();
                        try
                        {
                                string v = Encoding.ASCII.GetString(client.DownloadData(url));
                                return new Version(v);
                        }
                        catch
                        {
                                return null;
                        }
                }

                void start_Enter(object sender, StartEventArgs e)
                {
                        tg_back.IsChecked = true;
                        tg_back.IsEnabled = true;
                        btn_size.IsEnabled = true;
                        if (!Common.IsServer)
                        {
                                startDataTaskByZID(e.ZID);
                        }
                        network.InitNetwork(Common.ZoneDevices[e.ZID]);
                        txt_zone.Text = Common.ZoneDevices[e.ZID].First().ZoneName;
                        btn_run_Click(btn_run, null);
                        progress.start();
                        Task.Factory.StartNew(new Action(() =>
                        {
                                while (true)
                                {
                                        Thread.Sleep(1000);
                                        if (Common.ZoneDevices[e.ZID].Last().NeedParamsNum == 0)
                                        {
                                                progress.end();
                                                break;
                                        }
                                }
                        }));
                }

                void diagram_EnterDevice(object sender, EnterDeviceArgs e)
                {
                        dvPage.InitDevice(e.Dv, common);
                        Device device = e.Dv;
                        device.MyCom.ChangeDeviceLiveness(device.ZID, device.Address, 6);
                        txt_device.Text = device.Name;
                        setMainVisibility(dvPage);
                }

                void loadDevices()
                {
                        Common.ZoneDevices = new Dictionary<byte, List<Device>>();
                        Common.CancelTokens = new Dictionary<byte, CancellationTokenSource>();
                        XmlElement xeR = Tool.GetXML(@"Config/DeviceList.xml");
                        for (int z = 0; z < xeR.ChildNodes.Count; z++)
                        {
                                List<Device> devices = new List<Device>();

                                XmlElement zone = (XmlElement)xeR.ChildNodes[z];
                                byte zid = byte.Parse(zone.GetAttribute("ZID"));
                                string zoneName = zone.GetAttribute("ZoneName");
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
                                        device.InitDevice(zid, zoneName, addr, dvType, name, alias, parent);
                                        device.PreRemoteModify += (s, o) =>
                                        {
                                                stopDataTaskByZID(device.ZID);
                                        };
                                        device.RemoteModified += (s, o) =>
                                        {
                                                startDataTaskByZID(device.ZID);
                                        };
                                        device.MyCom = com;
                                        device.InitAddress();
                                        devices.Add(device);
                                }
                                Tool.FindDevicesParents(devices);
                                Common.ZoneDevices.Add(zid, devices);
                                Common.CancelTokens.Add(zid, new CancellationTokenSource());
                        }
                }

                void MainWindow_Closed(object sender, EventArgs e)
                {
                        if (dvPage.Visibility == Visibility.Visible)
                        {
                                dvPage.Visibility = Visibility.Hidden;
                        }
                        //Environment.Exit(0);
                }

                private void btn_close_Click(object sender, System.Windows.RoutedEventArgs e)
                {
                        this.Close();
                }

                private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                {
                        this.DragMove();
                }

                private void btn_devices_Click(object sender, RoutedEventArgs e)
                {
                        setHerePosition(sender);
                        deviceList.InitList();
                        setMainVisibility(deviceList);
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
                        System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + "EDSHelp.pdf");
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
                        MsgBox.Show("Tel：0592-610-0660-322(分机号)\r\nEmail：haisheng.xu@xseec.cn", "联系方式", MsgBox.Buttons.OK, MsgBox.Icons.Info);
                }

                private void about_Click(object sender, RoutedEventArgs e)
                {
                        System.Diagnostics.Process.Start("http://www.xseec.cn/cn/index.asp");
                }

                private void system_Click(object sender, RoutedEventArgs e)
                {
                        if (common.UserLevel == User.UNKNOWN)
                        {
                                btn_account_Click(this, null);
                                if (common.UserLevel == User.UNKNOWN)
                                        return;
                        }
                        Setting setting = new Setting();
                        setting.InitSetting(common);
                        setting.ShowDialog();
                }

                private void tg_back_Click(object sender, RoutedEventArgs e)
                {
                        if (dvPage.Visibility == Visibility.Visible)
                        {
                                btn_run_Click(btn_run, null);
                                tg_back.IsChecked = true;
                        }
                        else
                        {
                                network.ClearNetwork();
                                if (!Common.IsServer)
                                {
                                        stopDataTaskByZID(Common.CancelTokens.Keys.First());
                                }
                                setMainVisibility(null);
                                startPage.showStart();
                                tg_back.IsEnabled = false;
                                btn_size.IsEnabled = false;
                                this.WindowState = WindowState.Normal;
                        }
                }
        }
}
