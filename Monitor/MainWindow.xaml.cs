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
                List<Device> devices=null;
                List<UIElement> mainContainer=null;
                Thread threadRun=null;
                public MainWindow()
                {
                        InitializeComponent();
                        com = new Com(ComType.SP);
                        //com = new Com(ComType.TCP, "172.16.66.124");
                        intitControls();
                        Thread t = new Thread(() =>
                        {
                                this.Dispatcher.Invoke(new Action(() => {
                                        initDiagram();
                                }));
                        });
                        t.Start();
                }

                private void intitControls()
                {
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

                        dvDevices.ReloadDevices += new EventHandler<EventArgs>(dvDevices_ReloadDevices);

                        mainContainer = new List<UIElement>() { diagram, dvPage, dvDevices, img_link };
                }

                void dvDevices_ReloadDevices(object sender, EventArgs e)
                {
                        initDiagram();
                        btn_run_Click(btn_run, new RoutedEventArgs());
                }

                private void initDiagram()
                {
                        if(threadRun!=null)
                                threadRun.Suspend();
                        loadDevices();
                        testComs();
                        diagram.InitDevices(devices);
                        diagram.EnterDevice += new EventHandler<EnterDeviceArgs>(diagram_EnterDevice);
                        threadRun = new Thread(() =>
                        {
                                while (true)
                                {
                                        foreach (var dv in devices)
                                        {
                                                dv.GetData();
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
                        dvPage.InitDevice(e.Dv);
                }
                

                void loadDevices()
                {
                        devices = new List<Device>();
                        XmlElement xeR = Tool.GetXML(@"Devices/DeviceList.xml");
                        foreach (XmlElement xe in xeR.ChildNodes)
                        {
                                string type = xe.ChildNodes[0].InnerText;
                                string name = xe.ChildNodes[1].InnerText;
                                byte addr = byte.Parse(xe.ChildNodes[2].InnerText);
                                Device device=null;
                                switch (type)
                                {
                                        case "ACB":
                                                device=new DvACB(addr, DeviceType.ACB, name);
                                                break;
                                        case "MCCB":
                                                device = new DvMCCB(addr, DeviceType.MCCB, name);
                                                break;
                                }
                                device.MyCom = com;
                                device.PreRemoteModify += (s, o) => { threadRun.Suspend(); };
                                device.RemoteModified += (s, o) => { threadRun.Resume(); };
                                device.InitAddress();
                                devices.Add(device);
                        }
                }

                void testComs()
                {
                        foreach (var d in devices)
                        {
                                string resultCom = com.TestCom(d.Address, Tool.GetCom());
                                if (resultCom != null)
                                {
                                        Tool.SetCom(resultCom);
                                        return;
                                }
                        }
                        MsgBox.Show("Please check serial ports and device's addresses.", "No Signal", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
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
                        img_here.Margin = (sender as ImageButton).Margin;
                        dvDevices.InitDataGrid(devices);
                        setMainVisibility(dvDevices);
                }

                private void btn_run_Click(object sender, RoutedEventArgs e)
                {
                        img_here.Margin = (sender as ImageButton).Margin;
                        setMainVisibility(diagram);
                }

                private void btn_link_Click(object sender, RoutedEventArgs e)
                {
                        img_here.Margin = (sender as ImageButton).Margin;
                        setMainVisibility(img_link);
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
        }
}
