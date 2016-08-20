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
        /// Progress.xaml 的交互逻辑
        /// </summary>
        public partial class Progress : UserControl
        {
                public bool Show;
                public Progress()
                {
                        InitializeComponent();
                }

                public void start()
                {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                                if (!Show)
                                {
                                        Card.Background = new SolidColorBrush(Color.FromArgb(0xff, 0xfa, 0xfa, 0xfa));
                                        lbl_wait.Visibility = Visibility.Visible;
                                        img_result.Visibility = Visibility.Hidden;
                                        setAnimation(0, 1);
                                        Show = true;
                                }
                        }));
                }

                public void end(bool hasError=false)
                {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                                if (Show)
                                {
                                        Brush bg = new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0x96, 0x88));
                                        string img = "pack://application:,,,/Monitor;component/Images/done.png";
                                        if (hasError)
                                        {
                                                bg = Brushes.Red;
                                                img = "pack://application:,,,/Monitor;component/Images/wrong.png";
                                        }
                                        Card.Background = bg;
                                        lbl_wait.Visibility = Visibility.Hidden;
                                        img_result.Visibility = Visibility.Visible;
                                        img_result.Source = new BitmapImage(new Uri(img, UriKind.RelativeOrAbsolute));
                                        setAnimation(2, 0,1);
                                        Show = false;
                                }
                        }));
                }

                void setAnimation(double from, double to,double duration=0.5)
                {
                        DoubleAnimation da = new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(duration)));
                        ScaleTransform st = new ScaleTransform();
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, da);
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, da);
                        Card.RenderTransform = st;
                }
        }
}
