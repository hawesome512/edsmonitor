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
using System.Xml;

using System.Windows.Forms.DataVisualization.Charting;

namespace Monitor
{
        /// <summary>
        /// Energy.xaml 的交互逻辑
        /// </summary>
        public partial class Energy : UserControl
        {
                RadioButton[] rbs;
                Chart colChart, pieChart;
                System.Windows.Forms.DataVisualization.Charting.HitTestResult hitTest;
                DateTime start, end;
                Random random;
                public Energy()
                {
                        InitializeComponent();
                        initControls();
                }

                private void initControls()
                {
                        rbs = new RadioButton[] { rb_today, rb_week, rb_month, rb_other };
                        colChart = myHost1.Child as Chart;
                        pieChart = myHost2.Child as Chart;
                        initColChart();
                        initPieChart();
                        start = end = DateTime.Today;
                        rb_Click(rb_month, null);
                        random = new Random();
                }

                private void initColChart()
                {
                        colChart.BackColor = System.Drawing.Color.Transparent;
                        colChart.MouseDown += colChart_MouseDown;
                        colChart.MouseUp += colChart_MouseUp;
                        ChartArea area = new ChartArea();
                        area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                        area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                        area.AxisY.IsInterlaced = true;
                        area.AxisY.InterlacedColor = System.Drawing.Color.FromArgb(0x88, 0xdd, 0xdd, 0xdd);
                        area.BackColor = System.Drawing.Color.Transparent;
                        colChart.ChartAreas.Add(area);
                        Series series = new Series("default");
                        colChart.Series.Add(series);
                        series.ChartType = SeriesChartType.Column;
                        series.Color = System.Drawing.Color.FromArgb(0xff, 0x00, 0x96, 0x88);
                }

                void colChart_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
                {
                        hitTest = colChart.HitTest(e.X, e.Y);
                        if (hitTest.ChartElementType == ChartElementType.DataPoint)
                        {
                                DataPoint point = hitTest.Object as DataPoint;
                                point.IsValueShownAsLabel = false;
                                point.BackHatchStyle = ChartHatchStyle.None;
                        }
                }

                void colChart_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
                {
                        hitTest = colChart.HitTest(e.X, e.Y);
                        if (hitTest.ChartElementType == ChartElementType.DataPoint)
                        {
                                DataPoint point = hitTest.Object as DataPoint;
                                point.IsValueShownAsLabel = true;
                                point.BackHatchStyle = ChartHatchStyle.Percent25;
                        }
                }

                private void initPieChart()
                {
                        pieChart.BackColor = System.Drawing.Color.Transparent;
                        Legend legend1 = new Legend();
                        legend1.BackColor = System.Drawing.Color.Transparent;
                        legend1.IsTextAutoFit = false;
                        legend1.Name = "Default";
                        pieChart.Legends.Add(legend1);
                        ChartArea area = new ChartArea();
                        area.BackColor = System.Drawing.Color.Transparent;
                        pieChart.ChartAreas.Add(area);
                        Series series = new Series();
                        pieChart.Series.Add(series);
                        series.ChartType = SeriesChartType.Pie;
                        series["PieLabelStyle"] = "Inside";
                        series.Label = "#PERCENT{P1}";
                        series.Legend = "Default";
                }

                private void rb_Click(object sender, RoutedEventArgs e)
                {
                        foreach (RadioButton r in rbs)
                                r.BorderBrush = Brushes.Transparent;
                        RadioButton rb = sender as RadioButton;
                        rb.IsChecked = true;
                        rb.BorderBrush = Brushes.Red;
                }

                private void btn_query_Click(object sender, RoutedEventArgs e)
                {
                        TreeViewItem tvi = tree_area.SelectedItem as TreeViewItem;
                        if (tvi != null)
                        {
                                getDates();
                                setDaysData(int.Parse(tvi.Tag.ToString()));
                                List<string> parents = new List<string>();
                                TreeViewItem p = tvi;
                                do
                                {
                                        parents.Insert(0,p.Header.ToString());
                                        p = p.Parent as TreeViewItem;
                                } while (p != null);
                                List<string> children = new List<string>();
                                foreach (var t in tvi.Items)
                                {
                                        TreeViewItem tv = t as TreeViewItem;
                                        children.Add(tv.Header.ToString());
                                }
                                setPieData(children);
                                title.Content = string.Format("{0}  能耗分析  {1}-{2}  (单位：kWh)", string.Join("/", parents), start.ToString("yyyy/MM/dd"), end.ToString("yyyy/MM/dd"));
                                var points = colChart.Series[0].Points;
                                txt_max.Content = points.FindMaxByValue().YValues[0].ToString()+"  kWh";
                                txt_min.Content = points.FindMinByValue().YValues[0].ToString() + "  kWh";
                                int sum = (int)points.Sum(pt => pt.YValues[0]);
                                txt_sum.Content = sum.ToString() + "  kWh";
                                txt_avg.Content = sum / points.Count + "  kWh";

                                int rorate = random.Next(-10, 10);
                                txt_y2y.Content = Math.Abs(rorate)+"%";
                                img_y2y.Source = getImgSource(rorate);
                                rorate = random.Next(-10, 10);
                                txt_m2m.Content = Math.Abs(rorate)+"%";
                                img_m2m.Source = getImgSource(rorate);
                        }
                        else
                        {
                                MsgBox.Show("请选择需要显示的区域.", "未选择区域", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                        }
                }

                private ImageSource getImgSource(int flag)
                {
                        string url = flag > 0 ? "pack://application:,,,/Monitor;component/Images/up.png" : "pack://application:,,,/Monitor;component/Images/down.png";
                        return (new ImageSourceConverter().ConvertFromString(url) as ImageSource);
                }

                private void setPieData(List<string> xValues)
                {
                        var yValues = xValues.ConvertAll<int>(i => random.Next(5, 10));
                        Series series = pieChart.Series[0];
                        series.Points.DataBindXY(xValues, yValues);
                        if(xValues.Count>0)
                                series.Points.FindMaxByValue()["Exploded"] = "true";
                        for (int i = 0; i < series.Points.Count; i++)
                        {
                                series.Points[i].IsValueShownAsLabel = true;
                                series.Points[i].LegendText = xValues[i];
                        }
                }

                private void setDaysData(int level)
                {
                        List<string> days = new List<string>();
                        List<int> yValues = new List<int>();
                        int min,max;
                        switch (level)
                        {
                                case 0:
                                        min = 1400;
                                        max = 1800;
                                        break;
                                case 1:
                                        min = 300;
                                        max = 480;
                                        break;
                                default:
                                        min = 80;
                                        max = 125;
                                        break;

                        }
                        if (end.AddDays(-1) <= start)
                        {
                                for (DateTime s = start; s <= end; s = s.AddHours(1))
                                {
                                        days.Add(s.ToShortTimeString());
                                        yValues.Add(random.Next(min/24, max/24));
                                }
                        }
                        else
                        {
                                for (DateTime s = start; s <= end; s = s.AddDays(1))
                                {
                                        days.Add(s.Date.ToString("MM-dd"));
                                        yValues.Add(random.Next(min, max));
                                }
                        }
                        colChart.Series[0].Points.DataBindXY(days, yValues);
                }

                private void getDates()
                {
                        int index=rbs.ToList().FindIndex(r => r.IsChecked == true);
                        DateTime t=DateTime.Today;
                        switch (index)
                        {
                                case 0:
                                        start = t;
                                        end = DateTime.Now;
                                        break;
                                case 1:
                                        start = t.AddDays(DayOfWeek.Monday - t.DayOfWeek);
                                        end = t;
                                        break;
                                case 2:
                                        start = new DateTime(t.Year, t.Month, 1);
                                        end = t;
                                        break;
                                default :
                                        break;
                        }
                }

                private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
                {
                        DatePicker dp = sender as DatePicker;
                        if (dp.Name == "dp1")
                        {
                                start = DateTime.Parse(dp.Text);
                        }
                        else
                        {
                                end = DateTime.Parse(dp.Text);
                        }
                }

        }
}
