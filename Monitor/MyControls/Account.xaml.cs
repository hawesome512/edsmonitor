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
        /// Account.xaml 的交互逻辑
        /// </summary>
        public partial class Account : Window
        {
                public Account()
                {
                        InitializeComponent();
                }

                private void btn_cancel_Click(object sender, RoutedEventArgs e)
                {
                        this.Close();
                }

                private void txt_pwd_LostFocus(object sender, RoutedEventArgs e)
                {
                        bool bUser = validInput("Username", txt_user.Text);
                        bool bPwd = validInput("Password", txt_pwd.Password);
                        if (!(bUser && bPwd))
                        {
                                txt_incorrect.Visibility = Visibility.Visible;
                                txt_pwd.Password = txt_user.Text = null;
                        }

                }

                private void btn_save_Click(object sender, RoutedEventArgs e)
                {
                        bool bUser=string.IsNullOrEmpty(txt_pwd_new.Password);
                        bool bPsw=string.IsNullOrEmpty(txt_user_new.Text);
                        if (bUser || bPsw)
                        {
                                txt_incorrect_new.Visibility = Visibility.Visible;
                        }
                        else
                        {
                                string md5User = Tool.MD5Cryp(txt_user_new.Text);
                                string md5Psw = Tool.MD5Cryp(txt_pwd_new.Password);
                                Tool.SetConfig("Username", md5User);
                                Tool.SetConfig("Password", md5Psw);
                                this.Close();
                        }
                }

                private void txt_user_LostFocus(object sender, RoutedEventArgs e)
                {
                        if (txt_user.Text != null)
                                txt_user_new.Text = txt_user.Text;
                }

                bool validInput(string key, string value)
                {
                        string md5Key = Tool.GetConfig(key);
                        string md5Value = Tool.MD5Cryp(value);
                        return md5Key.Equals(md5Value);
                }

                private void txt_user_GotFocus(object sender, RoutedEventArgs e)
                {
                        txt_incorrect_new.Visibility = Visibility.Hidden;
                        txt_incorrect.Visibility = Visibility.Hidden;
                }
        }
}
