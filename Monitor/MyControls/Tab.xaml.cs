﻿using System;
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
                Common common;
                double hei = 30;
                double factorX, factorY;
                double gridX, gridY;
                double wid;
                UIChart uiChart;
                List<string> switchList, cboxList;
                public Tab()
                {
                        this.InitializeComponent();
                        this.SizeChanged += Tab_SizeChanged;
                }

                void Tab_SizeChanged(object sender, SizeChangedEventArgs e)
                {
                        factorX = e.NewSize.Width / 694;
                        factorY = e.NewSize.Height / 500;
                        hei = 30 * factorY;
                        Grid grid = (Grid)((TabItem)myTab.SelectedItem).Content;
                        gridX = grid.ActualWidth;
                        gridY = grid.ActualHeight;
                        wid = gridX / 6;
                        if (device != null)
                        {
                                removeOldControls();
                                initMonitor();
                                initParams();
                                setDeviceParams();
                                initRemote();
                        }
                }

                public void InitTab(Device _device, Common _common)
                {
                        removeOldControls();
                        dt_day.Text = "";
                        myTab.SelectedIndex = 0;

                        this.DataContext = _device;
                        device = _device;
                        common = _common;
                        initMonitor();
                        initParams();
                        initRemote();
                        initChart();
                }

                private void removeOldControls()
                {
                        //待优化——ATS未上电时进入设备页面离开，在进入设备页面出现问题。
                        try
                        {
                                monitor.Children.Clear();
                        }
                        catch
                        {

                        }
                        param.Children.Clear();
                        remote.Children.Clear();
                }

                private void initChart()
                {
                        System.Windows.Forms.DataVisualization.Charting.Chart chart;
                        chart = myHost.Child as System.Windows.Forms.DataVisualization.Charting.Chart;
                        uiChart = new UIChart(chart);
                }

                private void initParams()
                {
                        switchList = new List<string>();
                        cboxList = new List<string>();
                        //hei = (device.DvType == DeviceType.MCCB) ? 30 : 30;

                        //布局：每行设置高度30,3列
                        int index = 0;

                        //配置：中英文切换
                        //addTitle("Protect Switch", ref index);
                        addTitle("保护开关", ref index);

                        addSwitches(ref index, "ProtectSwitchH");
                        addSwitches(ref index, "ProtectSwitchL");
                        index = (index / 3 + 1) * 3;//另起一行

                        //配置：中英文切换
                        //addTitle("Params", ref index);
                        addTitle("参数", ref index);

                        List<string> keys = new List<string>(device.Params.Keys);
                        keys.RemoveRange(0, 4);
                        addParams(ref index, keys);
                        addSetBtn(index);
                }

                private void setDeviceParams()
                {
                        setSwitches();
                        setParams();
                }

                private void setParams()
                {
                        foreach (string key in cboxList)
                        {
                                string name = Tool.GetValid(key);
                                ComboBox cbox = Tool.FindChild<ComboBox>(param, name);
                                string selection = device.Params[key].ShowValue;
                                if (cbox.Items.Contains(selection))
                                        cbox.SelectedValue = device.Params[key].ShowValue;
                                else
                                        cbox.SelectedIndex = Tool.MatchItems(cbox.Items,selection);
                        }
                }

                private void setSwitches()
                {
                        int data1;
                        if (int.TryParse(device.Params["ProtectSwitchH"].ShowValue, out data1))
                        {
                                int data2 = int.Parse(device.Params["ProtectSwitchL"].ShowValue);
                                var array = new List<string>(device.Params["ProtectSwitchH"].Tag.Split('_'));
                                array.RemoveAll(a => a == "*");
                                int nLen = array.Count;
                                int data = data2 * (int)Math.Pow(2, nLen) + data1;
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
                        if (device.DvType != DeviceType.ATS)
                        {
                                Image img = new Image();
                                img.Width = 200 * factorX;
                                img.Height = 300 * factorY;
                                img.VerticalAlignment = VerticalAlignment.Top;
                                img.Margin = new Thickness(10);
                                remote.Children.Add(img);
                                img.SetBinding(Image.SourceProperty, Tool.addBinding("State", new StateToImageSourceConverter()));

                                OnOff btn = new OnOff();
                                btn.Width = 98 * factorX;
                                btn.Height = 38 * factorY;
                                btn.VerticalAlignment = VerticalAlignment.Top;
                                btn.toggleSwitch.SetBinding(WPFSpark.ToggleSwitch.IsCheckedProperty, Tool.addBinding("State", new StateToBoolConverter()));
                                btn.toggleSwitch.Click += new RoutedEventHandler(RemoteSwitch_Click);
                                btn.Margin = new Thickness(0, 310 * factorY, 0, 0);
                                remote.Children.Add(btn);
                                setMultiBinding(btn);
                        }
                        else
                        {
                                ATSRemote atsRemote = new ATSRemote();
                                atsRemote.Width = 200 * factorX;
                                atsRemote.Height = 360 * factorY;
                                atsRemote.VerticalAlignment = VerticalAlignment.Top;
                                atsRemote.Margin = new Thickness(10);
                                remote.Children.Add(atsRemote);
                                atsRemote.img_state.SetBinding(Image.SourceProperty, Tool.addBinding("State", new StateToATSImageSourceConverter()));
                                atsRemote.btn_off.Click += ATSRemote_Click;
                                atsRemote.btn_ton.Click += ATSRemote_Click;
                                atsRemote.btn_tos.Click += ATSRemote_Click;
                        }
                }

                private void initMonitor()
                {
                        string[] items = null;
                        switch (device.DvType)
                        {
                                case DeviceType.ACB:
                                        items = new string[] { "Ia", "Ib", "Ic", "Ua", "Ub", "Uc" };
                                        break;
                                case DeviceType.MCCB:
                                        items = new string[] { "Ia", "Ib", "Ic" };
                                        break;
                                case DeviceType.ATS:
                                        items = new string[] { "Ia", "Ib", "Ic", "Ua", "Ub", "Uc" };
                                        break;
                        }
                        double spaceX = (gridX - 3 * 200 * factorY) / 4;
                        double spaceY = (gridY - items.Length / 3 * 200 * factorY) / (items.Length / 3 + 1);
                        for (int i = 0; i < items.Length; i++)
                        {
                                Dial dial = items[i].StartsWith("I") ? initCurDial() : initVolDial();
                                string path = string.Format("State.{0}", items[i]);
                                //string path = string.Format("RealData[{0}].ShowValue", items[i]);
                                Binding bind = new Binding(path);
                                dial.myGauge2.SetBinding(CircularGauge.CircularGaugeControl.CurrentValueProperty, bind);
                                dial.HorizontalAlignment = HorizontalAlignment.Left;
                                dial.VerticalAlignment = VerticalAlignment.Top;
                                //dial.Margin = new Thickness(20+i%3*220, 14+i/3*214, 0, 0);
                                double x = (i % 3 + 1) * spaceX + i % 3 * 200 * factorY;
                                double y = (i / 3 + 1) * spaceY + i / 3 * 200 * factorY;
                                dial.Margin = new Thickness(x, y, 0, 0);
                                monitor.Children.Add(dial);
                                x += dial.Width * 0.5 - 20;
                                y += dial.Width * 0.6;
                                addLabel(items[i], new Thickness(x, y, 0, 0));
                        }
                }

                private Dial initCurDial()
                {
                        Dial dial = new Dial();
                        dial.setSize(200 * factorY);
                        dial.myGauge2.OptimalRangeStartValue = 0.8;
                        dial.myGauge2.BelowOptimalRangeColor = Colors.Green;
                        dial.myGauge2.OptimalRangeColor = Colors.Yellow;
                        dial.myGauge2.AboveOptimalRangeColor = Colors.Red;
                        dial.myGauge2.GaugeBackgroundColor = Colors.DarkSlateGray;
                        dial.myGauge2.DialTextColor = Colors.Black;
                        int nIn;
                        int.TryParse(device.BasicData["In"].ShowValue, out nIn);
                        nIn = nIn == 0 ? 1000 : nIn;//当设备没有通信时In=0,初始化表盘时In会作为除数被使用
                        dial.myGauge2.RatedValue = nIn;
                        return dial;
                }

                private Dial initVolDial()
                {
                        Dial dial = new Dial();
                        dial.setSize(200 * factorY);
                        int nUn = 127;
                        if (device.BasicData.ContainsKey("Un"))
                        {
                                int.TryParse(device.BasicData["Un"].ShowValue, out nUn);
                        }
                        dial.myGauge2.RatedValue = nUn;//220相电压：127
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

                private void addTitle(string title, ref int index)
                {
                        Border border = addBorder(new Thickness(0, 0, 0, 2), gridX, new Thickness(0, index / 3 * hei, 0, 0));
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
                                //配置：中英文切换
                                //addName(index, key);
                                addName(index, device.Params[key].Alias);
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
                        List<string> items = new List<string>(tagArray);
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

                private void addSwitches(ref int index, string addr)
                {
                        string[] swList = device.Params[addr].Tag.Split('_');
                        foreach (string sw in swList)
                        {
                                if (sw == "*")
                                        continue;//MCCB中设置很多的空位

                                List<string> sws = new List<string>(sw.Split('/'));
                                addName(index, sws[0]);
                                addSwitch(index, sws);
                                index++;
                        }
                }

                private void addSwitch(int index, List<string> sws)
                {
                        Border border = addBorder(new Thickness(0, 0, 1, 1), wid, new Thickness(index % 3 * 2 * wid + wid, index / 3 * hei, 0, 0));
                        OnOff btn = new OnOff();
                        btn.Width = 71*factorX;
                        btn.Height = 27*factorY;
                        btn.Name = Tool.GetValid(sws[0]);
                        //btn.toggleSwitch.Width = 71;
                        //btn.toggleSwitch.Height = 27;
                        btn.toggleSwitch.FontSize = 15;
                        btn.toggleSwitch.CornerRadius = new CornerRadius(btn.Height / 2);
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
                        index = ((index - 1) / 3 + 2) * 3;//另起一行
                        OnOff btn = new OnOff();
                        btn.Width = 105;
                        btn.Height = 40;
                        btn.toggleSwitch.FontSize = 17;
                        btn.toggleSwitch.CheckedText = "测量中";
                        btn.toggleSwitch.UncheckedText = "设置";
                        btn.VerticalAlignment = VerticalAlignment.Top;
                        btn.Margin = new Thickness(0, hei * index / 3, 0, 0);
                        param.Children.Add(btn);

                        //Binding bind = new Binding("State");
                        //bind.Converter = new ControlToBtnEnableConverter();
                        //btn.SetBinding(OnOff.IsEnabledProperty, bind);
                        setMultiBinding(btn);

                        btn.toggleSwitch.Click += setSwitch_Click;
                }

                private void addATSButton(string name, Thickness thickness, string imgName)
                {
                        ImageButton btn = new ImageButton();
                        btn.HorizontalAlignment = HorizontalAlignment.Left;
                        btn.VerticalAlignment = VerticalAlignment.Top;
                        btn.Margin = thickness;
                        btn.Content = name;
                        btn.Width = 50;
                        btn.Height = 60;
                        btn.ImgPath = string.Format("/Images/ats/{0}.png", imgName);
                        btn.Click += new RoutedEventHandler((o, s) =>
                        {
                        });
                        btn.Template = (ControlTemplate)Application.Current.Resources["ImageButtonTemplate"];
                        remote.Children.Add(btn);
                }

                private void setMultiBinding(OnOff btn)
                {
                        MultiBinding mBinding = new MultiBinding();
                        mBinding.Converter = new MultiToBtnEnableConvertoer();
                        mBinding.Bindings.Add(new Binding("State")
                        {
                                Source = device
                        });
                        mBinding.Bindings.Add(new Binding("UserLevel")
                        {
                                Source = common
                        });
                        btn.SetBinding(OnOff.IsEnabledProperty, mBinding);
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
                                        MsgBox.Show("参数修改成功", "成功", MsgBox.Buttons.OK, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                                }
                                else
                                {
                                        MsgBox.Show("参数修改失败", "失败", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                                }
                                param.Background = Brushes.Transparent;
                        }
                }

                void RemoteSwitch_Click(object sender, RoutedEventArgs e)
                {
                        WPFSpark.ToggleSwitch ts = sender as WPFSpark.ToggleSwitch;
                        string command;
                        if (ts.IsChecked == true)//分闸
                        {
                                command = "Open";
                        }
                        else//合闸
                        {
                                command = "Close";
                        }
                        var result = device.RemoteControl(command);
                        if (result.Length == 1)
                        {
                                MsgBox.Show(string.Format("{0} 处于 {1}状态.", device.Name, command), "Succeed", MsgBox.Buttons.OK, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                        }
                        else
                        {
                                MsgBox.Show(string.Format("{0} {1}操作失败.", device.Name, command), "Fail", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                        }
                }

                void ATSRemote_Click(object sender, RoutedEventArgs e)
                {
                        Button btn=sender as Button;
                        string command,cmdAlias;
                        switch (btn.Name)
                        {
                                case "btn_ton":
                                        command = "N";
                                        cmdAlias = "投常";
                                        break;
                                case "btn_tos":
                                        command = "S";
                                        cmdAlias = "投备";
                                        break;
                                default:
                                        command = "Open";
                                        cmdAlias = "双分";
                                        break;
                        }
                        var result = device.RemoteControl(command);
                        if (result.Length == 1)
                        {
                                MsgBox.Show(string.Format("{0} 处于 {1}状态.", device.Name, cmdAlias), "成功", MsgBox.Buttons.OK, MsgBox.Icon.Shield, MsgBox.AnimateStyle.FadeIn);
                        }
                        else
                        {
                                MsgBox.Show(string.Format("{0} {1}操作失败.", device.Name, command), "失败", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
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
                                data = Tool.BitSet(data, i, para);
                        }
                        dataList.Add(data.ToString());
                        for (int i = 0; i < cboxList.Count; i++)
                        {
                                string name = Tool.GetValid(cboxList[i]);
                                ComboBox cbox = Tool.FindChild<ComboBox>(param, name);
                                dataList.Add(cbox.SelectionBoxItem.ToString());
                        }
                        var t = device.SetParams(dataList);
                        return t.Length == 1;
                }

                private void Label_MouseDown(object sender, MouseButtonEventArgs e)
                {
                        setDeviceParams();
                }
                private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
                {
                        int nIn=1000,nUn=127;
                        int.TryParse(device.BasicData["In"].ShowValue, out nIn);
                        if (device.BasicData.ContainsKey("Un"))
                        {
                                int.TryParse(device.BasicData["Un"].ShowValue, out nUn);
                        }
                        uiChart.SetData(nIn,nUn);
                }
        }
}