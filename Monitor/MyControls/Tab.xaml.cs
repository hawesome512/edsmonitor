using System;
using System.Collections.Generic;
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
using System.Timers;

namespace Monitor
{
	/// <summary>
	/// Tab.xaml 的交互逻辑
	/// </summary>
	public partial class Tab : UserControl
	{
                Device device;
                double hei = 30;
                double width = 678;
                double wid = 678/ 6;
                List<string> switchList,cboxList;
		public Tab()
		{
			this.InitializeComponent();
		}

                public void InitTab(Device _device)
                {
                        removeOldControls();
                        myTab.SelectedIndex = 0;

                        this.DataContext = _device;
                        device=_device;
                        initMonitor();
                        initParams();
                        initRemote();
                }

                private void removeOldControls()
                {
                        monitor.Children.Clear();
                        param.Children.Clear();
                        remote.Children.Clear();
                }

                private void initParams()
                {
                        switchList = new List<string>();
                        cboxList = new List<string>();
                        hei=(device.DvType == DeviceType.MCCB)?40:30;

                        //布局：每行设置高度30,3列
                        int index = 0;
                        addTitle("Protect Switch",ref index);
                        addSwitches(ref index, "ProtectSwitchH");
                        addSwitches(ref index, "ProtectSwitchL");
                        index = (index / 3 + 1) * 3;//另起一行
                        addTitle("Params",ref index);
                        List<string> keys = new List<string>(device.Params.Keys);
                        keys.RemoveRange(0, 4);
                        addParams(ref index,keys);
                        addSetBtn(index);
                }

                private void setDeviceParams()
                {
                        setSwitches();
                        setParams();
                }

                private void setParams()
                {
                        foreach(string key in cboxList)
                        {
                                string name = Tool.GetValid(key);
                                ComboBox cbox = Tool.FindChild<ComboBox>(param, name);
                                string selection=device.Params[key].ShowValue;
                                if (cbox.Items.Contains(selection))
                                        cbox.SelectedValue = device.Params[key].ShowValue;
                                else
                                        cbox.SelectedIndex = 0;
                        }
                }

                private void setSwitches()
                {
                        int data1;
                        if (int.TryParse(device.Params["ProtectSwitchH"].ShowValue, out data1))
                        {
                                int data2 = int.Parse(device.Params["ProtectSwitchL"].ShowValue);
                                var array=new List<string>(device.Params["ProtectSwitchH"].Tag.Split('_'));
                                array.RemoveAll(a => a == "*");
                                int nLen = array.Count;
                                int data = data2 * (int)Math.Pow(2,nLen) + data1;
                                for (int i = 0; i < switchList.Count; i++)
                                {
                                        string name = Tool.GetValid(switchList[i]);
                                        OnOff btn = Tool.FindChild<OnOff>(param, name);
                                        btn.toggleSwitch.IsChecked = !Tool.isOne(data, i);
                                }
                        }
                }

                private void initRemote()
                {
                        Image img = new Image();
                        img.Width = 200;
                        img.Height = 300;
                        img.VerticalAlignment = VerticalAlignment.Top;
                        img.Margin = new Thickness(10);
                        img.SetBinding(Image.SourceProperty, Tool.addBinding("State", new StateToImageSourceConverter()));
                        remote.Children.Add(img);

                        OnOff btn = new OnOff();
                        btn.Width = 98;
                        btn.Height = 38; 
                        btn.VerticalAlignment = VerticalAlignment.Top;
                        btn.toggleSwitch.SetBinding(WPFSpark.ToggleSwitch.IsCheckedProperty, Tool.addBinding("State", new StateToBoolConverter()));
                        btn.toggleSwitch.Click +=new RoutedEventHandler(RemoteSwitch_Click);
                        btn.Margin = new Thickness(0,310,0,0);
                        remote.Children.Add(btn);
                        Binding bind = new Binding("State");
                        bind.Converter = new ControlToBtnEnableConverter();
                        btn.SetBinding(OnOff.IsEnabledProperty, bind);
                }

                private void initMonitor()
                {
                        switch (device.DvType)
                        {
                                case DeviceType.ACB:
                                        initAcbMonitor();
                                        break;
                                case DeviceType.MCCB:
                                        initMccbMonitor();
                                        break;
                        }
                }

                private void initMccbMonitor()
                {
                        string[] items = new string[] { "Ia", "Ib", "Ic"};
                        for (int i = 0; i < items.Length; i++)
                        {
                                Dial dial = initCurDial();
                                string path = string.Format("RealData[{0}].ShowValue", items[i]);
                                Binding bind = new Binding(path);
                                dial.myGauge2.SetBinding(CircularGauge.CircularGaugeControl.CurrentValueProperty, bind);
                                dial.HorizontalAlignment = HorizontalAlignment.Left;
                                dial.VerticalAlignment = VerticalAlignment.Top;
                                dial.Margin = new Thickness(20 + i* 220, 100, 0, 0);
                                monitor.Children.Add(dial);

                                addLabel(items[i], new Thickness(100 + i * 220, 220, 0, 0));
                        }
                }

                private void initAcbMonitor()
                {
                        string[] items = new string[] { "Ia", "Ib", "Ic", "Ua", "Ub", "Uc" };
                        for (int i = 0; i < items.Length; i++)
                        {
                                Dial dial = i < 3 ? initCurDial() : initVolDial(); 
                                string path = string.Format("State.{0}", items[i]);
                                //string path = string.Format("RealData[{0}].ShowValue", items[i]);
                                Binding bind = new Binding(path);
                                dial.myGauge2.SetBinding(CircularGauge.CircularGaugeControl.CurrentValueProperty, bind);
                                dial.HorizontalAlignment = HorizontalAlignment.Left;
                                dial.VerticalAlignment = VerticalAlignment.Top;
                                //dial.Margin = new Thickness(20+i%3*220, 14+i/3*214, 0, 0);
                                dial.Margin = new Thickness(20 + i % 3 * 220,  i / 3 * 220, 0, 0);
                                monitor.Children.Add(dial);

                                addLabel(items[i], new Thickness(100 + i % 3 * 220,120+ i / 3 * 220, 0, 0));
                        }
                }

                private Dial initCurDial()
                {
                        Dial dial = new Dial();
                        dial.myGauge2.OptimalRangeStartValue = 0.8;
                        dial.myGauge2.BelowOptimalRangeColor = Colors.Green;
                        dial.myGauge2.OptimalRangeColor = Colors.Yellow;
                        dial.myGauge2.AboveOptimalRangeColor = Colors.Red;
                        dial.myGauge2.GaugeBackgroundColor = Colors.DarkSlateGray;
                        dial.myGauge2.DialTextColor = Colors.Black;
                        int nIn;
                        int.TryParse(device.BasicData["In"].ShowValue, out nIn);
                        nIn = nIn == 0 ? 1000 : nIn;//当设备没有通信时In=0,初始化表盘时In会作为除数被使用
                        dial.myGauge2.RatedValue =nIn;
                        return dial;
                }

                private Dial initVolDial()
                {
                        Dial dial = new Dial();
                        dial.myGauge2.RatedValue = 127;//220相电压：127
                        return dial;
                }

                private Border addBorder(Thickness btk, double width, Thickness margin)
                {
                        Border border = new Border();
                        border.BorderBrush = Brushes.Black;
                        border.BorderThickness = btk;
                        border.Height = hei;
                        border.Width = width;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                        border.VerticalAlignment = VerticalAlignment.Top;
                        border.Margin = margin;
                        param.Children.Add(border);
                        return border;
                }

                private void addTitle(string title,ref int index)
                {
                        Border border = addBorder(new Thickness(0, 0, 0, 2), width, new Thickness(0,index/3*hei,0,0));
                        TextBlock tb = new TextBlock();
                        tb.TextAlignment = TextAlignment.Center;
                        tb.Text = title;
                        tb.FontSize = 14;
                        tb.Background = Brushes.Black;
                        tb.Width = 120;
                        tb.VerticalAlignment = VerticalAlignment.Bottom;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.Margin = new Thickness(0);
                        border.Child = tb;

                        index += 3;
                }

                private void addLabel(string text, Thickness tk)
                {
                        TextBlock label = new TextBlock();
                        label.Text = text;
                        label.FontWeight = FontWeights.Thin;
                        label.HorizontalAlignment = HorizontalAlignment.Left;
                        label.VerticalAlignment = VerticalAlignment.Top;
                        label.Margin = tk;
                        label.Width = 40;
                        label.TextAlignment = TextAlignment.Center;
                        monitor.Children.Add(label);
                }

                private void addParams(ref int index, List<string> keys)
                {
                        foreach (string key in keys)
                        {
                                addName(index, key);
                                addComBox(index, key);
                                index++;
                        }
                }

                private void addComBox(int index, string key)
                {
                        Border border = addBorder(new Thickness(0, 0, 1, 1), wid, new Thickness(index % 3 * 2 * wid + wid, index / 3 * hei, 0, 0));
                        StackPanel sp = new StackPanel();
                        sp.Orientation = Orientation.Horizontal;
                        border.Child = sp;

                        ComboBox cbox = new ComboBox();
                        cbox.HorizontalContentAlignment = HorizontalAlignment.Center;
                        cbox.FontStyle = FontStyles.Italic;
                        cbox.Width = 0.7 * wid;
                        cbox.Background = Brushes.Transparent;
                        cbox.Name = Tool.GetValid(key);
                        var tagArray = device.Params[key].Tag.Split('_');
                        List<string> items=new List<string>(tagArray);
                        items.RemoveAt(0);
                        cbox.ItemsSource = items;
                        sp.Children.Add(cbox);
                        cboxList.Add(key);

                        //临时方案：塑壳有些功能没有或不能关闭，但是上位机可以修改
                        if (items.Count == 1 && items[0] == "---")
                        {
                                cbox.IsEnabled = false;
                        }

                        string unit = device.Params[key].Unit;
                        if (unit != "/")
                        {
                                TextBlock text = new TextBlock();
                                text.TextAlignment = TextAlignment.Center;
                                text.VerticalAlignment = VerticalAlignment.Center;
                                text.FontSize = 14;
                                text.Width = 0.3 * wid;
                                text.Text = device.Params[key].Unit;
                                text.Foreground = Brushes.Black;
                                sp.Children.Add(text);
                        }
                }

                private void addSwitches(ref int index,string addr)
                {
                        string[] swList = device.Params[addr].Tag.Split('_');
                        foreach (string sw in swList)
                        {
                                if (sw == "*")
                                        continue;//MCCB中设置很多的空位

                                List<string> sws = new List<string>(sw.Split('/'));
                                addName(index, sws[0]);
                                addSwitch(index,sws);
                                index++;
                        }
                }

                private void addSwitch(int index,List<string> sws)
                {
                        Border border=addBorder(new Thickness(0, 0, 1, 1), wid, new Thickness(index % 3 * 2 * wid + wid, index / 3 * hei, 0, 0));
                        OnOff btn = new OnOff();
                        btn.Width = 71;
                        btn.Height = 27;
                        btn.Name =Tool.GetValid(sws[0]);
                        btn.toggleSwitch.Width = 71;
                        btn.toggleSwitch.Height = 27;
                        btn.toggleSwitch.FontSize = 15;
                        btn.toggleSwitch.CornerRadius = new CornerRadius(btn.Height/2);
                        if (sws.Count == 3)
                        {
                                //临时方案：塑壳有些功能没有或不能关闭，但是上位机可以修改
                                if (sws[1] == sws[2])
                                        btn.toggleSwitch.IsEnabled = false;

                                btn.toggleSwitch.CheckedText = sws[2];
                                btn.toggleSwitch.UncheckedText = sws[1];
                        }
                        border.Child = btn;
                        switchList.Add(sws[0]);
                }

                private void addName(int index, string sw)
                {
                        Border border = addBorder(new Thickness(1, 0, 1, 1), wid, new Thickness(index % 3 * 2 * wid, index / 3 * hei, 0, 0));
                        TextBlock text = new TextBlock();
                        text.HorizontalAlignment = HorizontalAlignment.Right;
                        text.VerticalAlignment = VerticalAlignment.Center;
                        text.Margin = new Thickness(0, 0, 10, 0);
                        text.TextAlignment = TextAlignment.Right;
                        text.FontWeight = FontWeights.Bold;
                        text.FontSize = 14;
                        text.Text = sw;
                        text.Foreground = Brushes.Black;
                        border.Child = text;
                }

                private void addSetBtn(int index)
                {
                        index = ((index-1) / 3 + 2) * 3;//另起一行
                        OnOff btn = new OnOff();
                        btn.Width = 105;
                        btn.Height = 40;
                        btn.toggleSwitch.Width = btn.Width;
                        btn.toggleSwitch.Height = btn.Height;
                        btn.toggleSwitch.FontSize = 17;
                        btn.toggleSwitch.CornerRadius = new CornerRadius(btn.Height / 2);
                        btn.toggleSwitch.CheckedText = "Monitor";
                        btn.toggleSwitch.UncheckedText = "Set";
                        btn.VerticalAlignment = VerticalAlignment.Top;
                        btn.Margin = new Thickness(0, hei * index / 3, 0, 0);
                        param.Children.Add(btn);

                        Binding bind = new Binding("State");
                        bind.Converter = new ControlToBtnEnableConverter();
                        btn.SetBinding(OnOff.IsEnabledProperty, bind);

                        btn.toggleSwitch.Click += setSwitch_Click;
                }

                void setSwitch_Click(object sender, RoutedEventArgs e)
                {
                        WPFSpark.ToggleSwitch ts = sender as WPFSpark.ToggleSwitch;
                        bool? b = ts.IsChecked;
                        if (b == true)
                        {
                                param.Background = Brushes.LightGray;
                        }
                        else
                        {
                                if (getSetting())
                                {
                                        MsgBox.Show("Params have been modified", "Succeed", MsgBox.Buttons.OK, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                                }
                                else
                                {
                                        MsgBox.Show("Params haven't been modified", "Fail", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                                }
                                param.Background = Brushes.Transparent;
                        }
                }

                void RemoteSwitch_Click(object sender, RoutedEventArgs e)
                {
                        WPFSpark.ToggleSwitch ts = sender as WPFSpark.ToggleSwitch;
                        string command;
                        if (ts.IsChecked==true)//分闸
                        {
                                command = "Open";
                        }
                        else//合闸
                        {
                                command = "Close";
                        }
                        var result= device.RemoteControl(command);
                        if (result.Length == 1)
                        {
                                MsgBox.Show(string.Format("{0} is {1}.",device.Name,command), "Succeed", MsgBox.Buttons.OK, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                        }
                        else
                        {
                                MsgBox.Show(string.Format("{0} can't be {1}.", device.Name, command), "Fail", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                        }
                }

                private bool getSetting()
                {
                        List<string> dataList = new List<string>();
                        int data = 0;
                        for (int i = 0; i < switchList.Count; i++)
                        {
                                string name = Tool.GetValid(switchList[i]);
                                OnOff btn = Tool.FindChild<OnOff>(param, name);
                                int para = btn.toggleSwitch.IsChecked == true ? 0 : 1;
                                data=Tool.BitSet(data, i, para);
                        }
                        dataList.Add(data.ToString());
                        for (int i = 0; i < cboxList.Count; i++)
                        {
                                string name = Tool.GetValid(cboxList[i]);
                                ComboBox cbox = Tool.FindChild<ComboBox>(param, name);
                                dataList.Add(cbox.SelectionBoxItem.ToString());
                        }
                        var t=device.SetParams(dataList);
                        return t.Length == 1;
                }

                private void Label_MouseDown(object sender, MouseButtonEventArgs e)
                {
                        setDeviceParams();
                }
	}
}