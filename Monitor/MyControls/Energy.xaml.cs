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
        public class ViewModel
        {
                public XmlDataProvider XmlData
                {
                        get;
                        set;
                }

                public ViewModel()
                {
                        XmlData = new XmlDataProvider();
                        string dir = System.AppDomain.CurrentDomain.BaseDirectory;
                        XmlData.Source = new Uri(dir + @"Config\Grid.xml");
                        XmlData.XPath = "node";
                }
        }

        /// <summary>
        /// Energy.xaml 的交互逻辑
        /// </summary>
        public partial class Energy : UserControl
        {
                RadioButton[] rbs;
                Chart colChart, pieChart;
                System.Windows.Forms.DataVisualization.Charting.HitTestResult hitTest;
                DateTime start, end;
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
                        Series series = new Series("now");
                        series.ChartType = SeriesChartType.Column;
                        series.Color = System.Drawing.Color.FromArgb(0xff, 0x00, 0x96, 0x88);
                        colChart.Series.Add(series);
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
                        XmlElement selNode = tree_area.SelectedItem as XmlElement;
                        if (selNode != null)
                        {
                                progress.start();
                                myHost1.Visibility = myHost2.Visibility = Visibility.Hidden;
                                getDates();
                                int num = selNode.ChildNodes.Count;
                                int[] addrs = new int[num + 1];
                                addrs[0] = int.Parse(selNode.Attributes["Address"].Value);
                                for (int i = 0; i < num; i++)
                                {
                                        addrs[i + 1] = int.Parse(selNode.ChildNodes[i].Attributes["Address"].Value);
                                }
                                Task.Factory.StartNew(new Action(() =>
                                {
                                        List<EDSLot.Energy> dataList = DataLib.QueryEnergy(addrs, start, end);
                                        var dataForCol = dataList.FindAll(d => d.Address == addrs[0] && d.Time >= start && d.Time < end);
                                        var dataForPie = dataList.FindAll(d => d.Address != addrs[0]).ConvertAll(d => d.PE);
                                        double? now = dataForCol.Sum(d => d.PE);
                                        double? lastYear = dataList.Find(d => d.Time == start.AddYears(-1)).PE;
                                        double? lastMonth = dataList.Find(d => d.Time == start.AddMonths(-1)).PE;

                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                                progress.end();
                                                myHost1.Visibility = myHost2.Visibility = Visibility.Visible;
                                                title.Content = string.Format("{0}  能耗分析  {1}-{2}  (单位：kWh)", string.Join("/", selNode.Attributes["Name"].Value), start.ToString("yyyy/MM/dd"), end.ToString("yyyy/MM/dd"));
                                                setColData(dataForCol);
                                                setPieData(selNode, dataForPie);
                                                if (now != null && lastYear != null)
                                                {
                                                        double rorate = (double)now / (double)lastYear - 1;
                                                        txt_y2y.Content = Math.Round(rorate * 100, 1) + "%";
                                                        img_y2y.Source = getImgSource(rorate);
                                                }
                                                else
                                                {
                                                        txt_y2y.Content = "---%";
                                                }
                                                if (now != null && lastMonth != null)
                                                {
                                                        double rorate = (double)now / (double)lastMonth - 1;
                                                        txt_m2m.Content = Math.Round(rorate * 100, 1) + "%";
                                                        img_m2m.Source = getImgSource(rorate);
                                                }
                                                else
                                                {
                                                        txt_m2m.Content = "---%";
                                                }
                                                var points = colChart.Series[0].Points;
                                                if (points.Count > 0)
                                                {
                                                        txt_max.Content = points.FindMaxByValue().YValues[0].ToString() + "  kWh";
                                                        txt_min.Content = points.FindMinByValue().YValues[0].ToString() + "  kWh";
                                                        int sum = (int)points.Sum(pt => pt.YValues[0]);
                                                        txt_sum.Content = sum.ToString() + "  kWh";
                                                        txt_avg.Content = sum / points.Count + "  kWh";
                                                }
                                        }));
                                }));
                        }
                        else
                        {
                                MsgBox.Show("请选择需要显示的区域.", "未选择区域", MsgBox.Buttons.OK, MsgBox.Icons.Error);
                        }
                }

                private void setColData(List<EDSLot.Energy> dataList)
                {
                        bool shortTime = (end - start).TotalHours <= 24;
                        if (shortTime)
                        {
                                colChart.Series[0].Points.DataBindXY(dataList.ConvertAll(d => d.Time.ToShortTimeString()), dataList.ConvertAll(d => d.PE));
                        }
                        else
                        {
                                colChart.Series[0].Points.DataBindXY(dataList.ConvertAll(d => d.Time.ToString("M/d")), dataList.ConvertAll(d => d.PE));
                        }
                }

                private ImageSource getImgSource(double flag)
                {
                        string url = flag > 0 ? "pack://application:,,,/EDS;component/Images/up.png" : "pack://application:,,,/EDS;component/Images/down.png";
                        return (new ImageSourceConverter().ConvertFromString(url) as ImageSource);
                }

                private void setPieData(XmlElement selNode, List<double?> childValues)
                {
                        List<string> children = new List<string>();
                        foreach (XmlElement child in selNode.ChildNodes)
                        {
                                int address = int.Parse(child.Attributes["Address"].Value);
                                children.Add(child.Attributes["Name"].Value);
                        }
                        Series series = pieChart.Series[0];
                        series.Points.DataBindXY(children, childValues);
                        if (children.Count > 0)
                                series.Points.FindMaxByValue()["Exploded"] = "true";
                        for (int i = 0; i < series.Points.Count; i++)
                        {
                                series.Points[i].IsValueShownAsLabel = true;
                                series.Points[i].LegendText = children[i];
                        }
                }

                private void getDates()
                {
                        int index = rbs.ToList().FindIndex(r => r.IsChecked == true);
                        DateTime t = DateTime.Today;
                        switch (index)
                        {
                                case 0:
                                        start = t;
                                        end = DateTime.Now;
                                        break;
                                case 1:
                                        start = t.AddDays(-6);
                                        end = DateTime.Now;
                                        break;
                                case 2:
                                        start = t.AddMonths(-1).AddDays(1);
                                        end = DateTime.Now;
                                        break;
                                default:
                                        start = start > t ? t : start;
                                        end = end <= start ? start.AddDays(1) : end;
                                        end = end > t ? DateTime.Now : end;
                                        break;
                        }
                }

                private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
                {
                        DatePicker dp = sender as DatePicker;
                        if (dp.Name == "dp_start")
                        {
                                start = DateTime.Parse(dp.Text);
                        }
                        else
                        {
                                end = DateTime.Parse(dp.Text).AddDays(1);
                        }
                }
        }
}
