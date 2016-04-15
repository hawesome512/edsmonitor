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

                public void InitDataGrid(List<Device> members)
                {
                        deviceInfList=members.ConvertAll<DeviceInf>(m => new DeviceInf(m));
                        dg_devices.DataContext = deviceInfList;
                        List<string> ports = System.IO.Ports.SerialPort.GetPortNames().ToList();
                        cbox_coms.ItemsSource = ports;
                        cbox_coms.SelectedIndex = ports.IndexOf(Tool.GetCom());
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
                                root.AppendChild(node1);
                        }
                        doc.Save(@"Devices/DeviceList.xml");

                        if (cbox_coms.SelectedIndex == -1)
                                cbox_coms.SelectedIndex = 0;
                        Tool.SetCom(cbox_coms.SelectedValue.ToString());

                        var result=MsgBox.Show("Do you want to update the grid now?", "Saved", MsgBox.Buttons.YesNo, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                                ReloadDevices(this, new EventArgs());
                        }
                }

                private static void createNode(XmlDocument doc, XmlNode node,string name, string value)
                {
                        XmlElement xe = doc.CreateElement(name);
                        xe.InnerText = value;
                        node.AppendChild(xe);
                }
        }
        public class DeviceInf:IComparable
        {
                public string Name{get;set;}
                public DeviceType DvType{get;set;}
                public byte Address{get;set;}
                public DeviceInf() { }
                public DeviceInf(Device device)
                {
                        Name = device.Name;
                        Address = device.Address;
                        DvType = device.DvType;
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
                                else if(Address>di.Address)
                                        return 1;
                                else
                                {
                                        return Name.CompareTo(di.Name);
                                }
                        }
                }
        }
}
