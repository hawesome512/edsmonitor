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

namespace Monitor
{
        /// <summary>
        /// LoginPage.xaml 的交互逻辑
        /// </summary>
        public partial class LoginPage : UserControl
        {
                public event EventHandler<EventArgs> LoginSucceeded;
                public LoginPage()
                {
                        InitializeComponent();
                }

                private void btn_login_Click(object sender, RoutedEventArgs e)
                {
                        bool bUser=validInput("Username",txt_user.Text);
                        bool bPwd=validInput("Password",txt_pwd.Password);
                        if (bUser && bPwd)
                        {
                                txt_incorrect.Visibility = Visibility.Hidden;
                                LoginSucceeded(this, new EventArgs());
                        }
                        else
                        {
                                txt_incorrect.Visibility = Visibility.Visible;
                                txt_pwd.Password = txt_user.Text = null;
                        }
                }

                bool validInput(string key, string value)
                {
                        string md5Key = Tool.GetConfig(key);
                        string md5Value = Tool.MD5Cryp(value);
                        return md5Key.Equals(md5Value);
                }
        }
}
