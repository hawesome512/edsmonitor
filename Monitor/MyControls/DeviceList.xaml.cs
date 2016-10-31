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
        /// DeviceList.xaml 的交互逻辑
        /// </summary>
        public partial class DeviceList : UserControl
        {
                List<DeviceInf> deviceInfList;
                public DeviceList()
                {
                        InitializeComponent();
                }

                public void InitList()
                {
                        deviceInfList = new List<DeviceInf>();
                        foreach (var devices in Common.ZoneDevices.Values)
                        {
                                foreach (var device in devices)
                                {
                                        deviceInfList.Add(new DeviceInf(device));
                                }
                        }
                        ListCollectionView collection = new ListCollectionView(deviceInfList);
                        collection.GroupDescriptions.Add(new PropertyGroupDescription("ZoneName"));
                        dg_devices.ItemsSource = collection;
                }
        }


        public class DeviceInf
        {
                public string Name
                {
                        get;
                        set;
                }
                public string DeviceType
                {
                        get;
                        set;
                }
                public string ZoneName
                {
                        get;
                        set;
                }
                public byte Address
                {
                        get;
                        set;
                }
                public string ControlState
                {
                        get;
                        set;
                }
                public string Error
                {
                        get;
                        set;
                }
                public string RunState
                {
                        get;
                        set;
                }
                public string SwitchState
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
                        DeviceType = device.DvType.ToString();
                        ZoneName = device.ZoneName;
                        Error = device.State.ErrorMsg;
                        switch (device.State.ControlState)
                        {
                                case ControlMode.Local:
                                        ControlState = "本地";
                                        break;
                                case ControlMode.Remote:
                                        ControlState = "远程";
                                        break;
                                default:
                                        ControlState = "未知";
                                        break;
                        }
                        switch (device.State.RunState)
                        {
                                case Run.Alarm:
                                        RunState = "异常";
                                        break;
                                case Run.Normal:
                                        RunState = "正常";
                                        break;
                                default:
                                        RunState = "无信号";
                                        break;
                        }
                        switch (device.State.SwitchState)
                        {
                                case SwitchStatus.Open:
                                        SwitchState = "分闸";
                                        break;
                                case SwitchStatus.Close:
                                        SwitchState = "合闸";
                                        break;
                                default:
                                        SwitchState = "其他";
                                        break;
                        }
                }
        }
}
