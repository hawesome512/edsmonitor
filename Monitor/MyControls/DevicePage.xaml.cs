using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Monitor
{
        /// <summary>
        /// DevicePage.xaml 的交互逻辑
        /// </summary>
        public partial class DevicePage : UserControl
        {
                Device device;
                int countInit;
                public DevicePage()
                {
                        this.InitializeComponent();
                        countInit = LayoutRoot.Children.Count;
                        myTab.PreRemoteModify += (s, o) =>
                        {
                                progress.start();
                        };
                        myTab.RemoteModified += (s, o) =>
                        {
                                progress.end(o.Fail);
                                if (o.Fail)
                                {
                                        MsgBox.Show("操作失败，请稍后重试", "失败", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                                }
                        };
                }

                public byte GetAddr()
                {
                        return device.Address;
                }

                public void InitDevice(Device _device, Common common)
                {
                        int countNow = LayoutRoot.Children.Count;
                        LayoutRoot.Children.RemoveRange(countInit, countNow - countInit);
                        device = _device;
                        this.DataContext = device;
                        bindingState();
                        initBase(device.BasicData);
                        myTab.InitTab(device, common);
                }

                private void bindingState()
                {
                        Binding bind = new Binding("State");
                        bind.Converter = new ControlToStringConverter();
                        lblMode.SetBinding(Label.ContentProperty, bind);
                        Binding bind1 = new Binding("State");
                        bind1.Converter = new StateToStringConverter();
                        lblState.SetBinding(Label.ContentProperty, bind1);
                        Binding bind2 = new Binding("State.ErrorMsg");
                        lbl_error.SetBinding(Label.ContentProperty, bind2);
                        Binding bind3 = new Binding("State.ErrorMsg");
                        bind3.Converter = new StringToVisibility();
                        lbl_error.SetBinding(Label.VisibilityProperty, bind3);

                        string packUri = string.Format("pack://application:,,,/Monitor;component/Images/Types/{0}.jpg", device.Name);
                        dvImg.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }

                private void initBase(Dictionary<string, DValues> dictionary)
                {
                        double deltaH = 320 / dictionary.Count;
                        int index = 0;
                        foreach (KeyValuePair<string, DValues> d in dictionary)
                        {
                                Border border1 = addBorder(new Thickness(0, 0, 1, 1), deltaH, 150, new Thickness(20, index * deltaH, 0, 0));
                                TextBlock text = new TextBlock();
                                text.HorizontalAlignment = HorizontalAlignment.Right;
                                text.VerticalAlignment = VerticalAlignment.Center;
                                text.Margin = new Thickness(0, 0, 20, 0);
                                text.TextAlignment = TextAlignment.Right;
                                text.FontWeight = FontWeights.Bold;
                                text.FontSize = 14;

                                //配置：中英文切换
                                //text.Text = d.Key;
                                text.Text = d.Value.Alias;

                                border1.Child = text;

                                Border border2 = addBorder(new Thickness(0, 0, 0, 1), deltaH, 120, new Thickness(170, index * deltaH, 0, 0));
                                text = new TextBlock();
                                text.HorizontalAlignment = HorizontalAlignment.Left;
                                text.VerticalAlignment = VerticalAlignment.Center;
                                text.Margin = new Thickness(20, 0, 0, 0);
                                text.FontStyle = FontStyles.Italic;
                                text.FontSize = 14;
                                text.Text = d.Value.ShowValue;
                                if (d.Value.Unit != "/")
                                        text.Text += d.Value.Unit;
                                border2.Child = text;

                                index++;
                        }
                }

                private Border addBorder(Thickness btk, double height, double width, Thickness margin)
                {
                        Border border = new Border();
                        border.BorderBrush = Brushes.Black;
                        border.BorderThickness = btk;
                        border.Height = height;
                        border.Width = width;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                        border.VerticalAlignment = VerticalAlignment.Top;
                        border.Margin = margin;
                        Grid.SetRow(border, 1);
                        LayoutRoot.Children.Add(border);
                        return border;
                }
        }
}