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
using System.Windows.Shapes;

namespace Monitor
{
        /// <summary>
        /// Plan.xaml 的交互逻辑
        /// </summary>
        public partial class Plan : Window
        {
                Device myDevice;
                public Plan()
                {
                        InitializeComponent();
                }

                public void InitPlan(Device device)
                {
                        myDevice = device;
                        this.DataContext = device.Plans;
                }

                private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
                {
                        datePicker.IsDropDownOpen = true;
                }

                private void ImageButton_Click(object sender, RoutedEventArgs e)
                {
                        if (dg_plan.SelectedIndex < myDevice.Plans.Count)
                        {
                                myDevice.Plans.RemoveAt(dg_plan.SelectedIndex);
                                dg_plan.Items.Refresh();
                        }
                }

                private void btn_close_Click(object sender, RoutedEventArgs e)
                {
                        this.Close();
                }

                private void btn_add_Click(object sender, RoutedEventArgs e)
                {
                        string date = dateComBox.SelectedIndex == 2 ? datePicker.Text : dateComBox.Text;
                        string time = string.Format("{0} {1}", date, timePicker.Text);
                        var plan = myDevice.Plans.Find(p => p.Time == time);
                        if (plan != null)
                        {
                                plan.Action = actionComBox.Text;
                        }
                        else
                        {
                                if (myDevice.Plans.Count == 3)
                                {
                                        myDevice.Plans.RemoveAt(0);
                                }
                                myDevice.Plans.Add(new PlanData(time, actionComBox.Text));
                        }
                        dg_plan.Items.Refresh();
                }
        }

        public class PlanData
        {
                public String Time
                {
                        get;
                        set;
                }
                public string Action
                {
                        get;
                        set;
                }

                private bool executed;

                public PlanData(string time, string action)
                {
                        Time = time;
                        Action = action;
                        executed = false;
                }

                public bool ExecuteNow()
                {
                        if (executed)
                        {
                                return false;
                        }
                        var items = Time.Split(' ');
                        int dateType = Tool.CheckDateType(DateTime.Today);
                        string strTime = string.Format("{0} {1}", DateTime.Today.ToString("yyyy/MM/dd"), items[1]);
                        DateTime time = DateTime.Parse(strTime);
                        switch (items[0])
                        {
                                case "工作日":
                                        if (dateType > 0)
                                        {
                                                executed = true;
                                                return false;
                                        }
                                        break;
                                case "节假日":
                                        if (dateType == 0)
                                        {
                                                executed = true;
                                                return false;
                                        }
                                        break;
                                default:
                                        break;
                        }
                        double m = (DateTime.Now-time).TotalMinutes;
                        if (m > 0)
                        {
                                executed = true;
                                return true;
                        }
                        return false;
                }

                public string getAction()
                {
                        return Action == "合闸" ? "Close" : "Open";
                }
        }
}
