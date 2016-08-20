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
using System.Windows.Media.Animation;

namespace Monitor
{
        /// <summary>
        /// Start.xaml 的交互逻辑
        /// </summary>
        public partial class Start : UserControl
        {
                public EventHandler<EventArgs> Enter;
                public Start()
                {
                        InitializeComponent();
                }

                private void Button_Click(object sender, RoutedEventArgs e)
                {
                        setScaleAnimation(1, 0);
                        Enter(this, null);
                }

                public void showStart()
                {
                        setScaleAnimation(0, 1);
                }

                private void setScaleAnimation(double from, double to)
                {
                        DoubleAnimation da = new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(0.5)));
                        ScaleTransform st = new ScaleTransform();
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, da);
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, da);
                        this.RenderTransform = st;
                }
        }
}
