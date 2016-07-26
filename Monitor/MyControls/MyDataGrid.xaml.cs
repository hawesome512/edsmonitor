using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Xml;

namespace Monitor
{
        /// <summary>
        /// Interaction logic for MyDataGrid.xaml
        /// </summary>
        public partial class MyDataGrid : UserControl
        {
                List<DeviceInf> deviceInfList;
                public event EventHandler<EventArgs> ReloadDevices;
                public MyDataGrid()
                {
                        InitializeComponent();
                }

                public void InitDataGrid(List<Device> members, Common common)
                {
                        if (Common.CType == ComType.SP)
                        {
                                rb_sp.IsChecked = true;
                        }
                        else
                        {
                                rb_tcp.IsChecked = true;
                        }
                        deviceInfList = members.ConvertAll<DeviceInf>(m => new DeviceInf(m));
                        dg_devices.DataContext = deviceInfList;
                        List<string> ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
                        cbox_coms.ItemsSource = ports;
                        cbox_coms.SelectedIndex = ports.IndexOf(Tool.GetConfig("ComTag"));

                        Binding bind = new Binding();
                        bind.Source = common;
                        bind.Path = new PropertyPath("UserLevel");
                        bind.Converter = new UserToBoolConverter();
                        btn_save.SetBinding(ImageButton.IsEnabledProperty, bind);
                }

                private void btn_save_Click(object sender, RoutedEventArgs e)
                {
                        deviceInfList.Sort();
                        XmlDocument doc = new XmlDocument();
                        XmlNode node = doc.CreateXmlDeclaration("1.0", "utf-8", "");
                        doc.AppendChild(node);
                        XmlNode root = doc.CreateElement("Devices");
                        doc.AppendChild(root);
                        foreach (DeviceInf di in deviceInfList)
                        {
                                XmlNode node1 = doc.CreateElement("Device");
                                createNode(doc, node1, "Type", di.DvType.ToString());
                                createNode(doc, node1, "Name", di.Name);
                                createNode(doc, node1, "Address", di.Address.ToString());
                                createNode(doc, node1, "IP", di.IP.ToString());
                                byte parentAddr = 0;
                                byte.TryParse(di.ParentAddr,out parentAddr);
                                createNode(doc, node1, "Parent", di.ParentAddr);
                                root.AppendChild(node1);
                        }
                        doc.Save(@"Config/DeviceList.xml");

                        if (cbox_coms.SelectedIndex == -1)
                                cbox_coms.SelectedIndex = 0;
                        Tool.SetConfig("Com", cbox_coms.SelectedValue.ToString());
                        bool isSp = (bool)rb_sp.IsChecked;
                        Tool.SetConfig("ComType", isSp ? "SP" : "TCP");
                        Common.CType = isSp ? ComType.SP : ComType.TCP;

                        var result = MsgBox.Show("是否立即刷新配电网?", "保存成功", MsgBox.Buttons.YesNo, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                                ReloadDevices(this, new EventArgs());
                        }
                }

                private static void createNode(XmlDocument doc, XmlNode node, string name, string value)
                {
                        XmlElement xe = doc.CreateElement(name);
                        xe.InnerText = value;
                        node.AppendChild(xe);
                }
        }
        public class DeviceInf : IComparable
        {
                public string Name
                {
                        get;
                        set;
                }
                public string Alias
                {
                        get;
                        set;
                }
                public DeviceType DvType
                {
                        get;
                        set;
                }
                public byte Address
                {
                        get;
                        set;
                }
                public string ParentAddr
                {
                        get;
                        set;
                }
                public string IP
                {
                        get;
                        set;
                }
                public DeviceInf()
                {
                }
                public DeviceInf(Device device)
                {
                        Name = device.Name;
                        Address = device.Address;
                        DvType = device.DvType;
                        IP = device.IP;
                        Alias = device.Alias;
                        ParentAddr = device.ParentAddr==0?"---":device.ParentAddr.ToString();
                }

                public int CompareTo(object obj)
                {
                        DeviceInf di = obj as DeviceInf;
                        if (DvType < di.DvType)
                                return -1;
                        else if (DvType > di.DvType)
                                return 1;
                        else
                        {
                                if (Address < di.Address)
                                        return -1;
                                else if (Address > di.Address)
                                        return 1;
                                else
                                {
                                        return Name.CompareTo(di.Name);
                                }
                        }
                }
        }
}
