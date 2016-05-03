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

namespace Monitor
{
	/// <summary>
	/// OnOff.xaml 的交互逻辑
	/// </summary>
	public partial class OnOff : UserControl
	{
		public OnOff()
		{
			this.InitializeComponent();
		}

                private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
                {
                        toggleSwitch.Width = e.NewSize.Width;
                        toggleSwitch.Height = e.NewSize.Height;
                        toggleSwitch.CornerRadius =new CornerRadius(e.NewSize.Height / 2);
                }
	}
}