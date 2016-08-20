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

using System.Windows.Forms.DataVisualization.Charting;

namespace Monitor
{
        /// <summary>
        /// Record.xaml 的交互逻辑
        /// </summary>
        public partial class RecordPage : UserControl
        {
                Device device;
                Chart lineChart;
                System.Windows.Forms.DataVisualization.Charting.HitTestResult hitTest;
                public RecordPage()
                {
                        InitializeComponent();
                        initControls();
                }

                private void initControls()
                {
                        DateTime t = DateTime.Today;
                        dp_start.SelectedDate = new DateTime(t.Year, t.Month, 1);
                        dp_end.SelectedDate = t;
                        initChart();
                }

                private void initChart()
                {
                        lineChart = myHost.Child as Chart;
                        clearChart();
                        ChartArea area = new ChartArea();
                        area.AxisX.LabelStyle.Format = "HH:mm";
                        area.CursorX.IsUserEnabled = true;
                        area.CursorX.IsUserSelectionEnabled = true;
                        StripLine stripLow = new StripLine();
                        stripLow.BackColor = System.Drawing.Color.FromArgb(64, System.Drawing.Color.Green);
                        StripLine stripMed = new StripLine();
                        stripMed.BackColor = System.Drawing.Color.FromArgb(64, System.Drawing.Color.Orange);
                        StripLine stripHigh = new StripLine();
                        stripHigh.BackColor = System.Drawing.Color.FromArgb(64, System.Drawing.Color.Red);
                        area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Gray;
                        area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
                        area.AxisY.StripLines.Add(stripLow);
                        area.AxisY.StripLines.Add(stripMed);
                        area.AxisY.StripLines.Add(stripHigh);
                        lineChart.ChartAreas.Add(area);
                        string[] lines = new string[] { "Ia", "Ib", "Ic", "IN" };
                        foreach (string line in lines)
                        {
                                Series series = new Series(line);
                                series.ChartType = SeriesChartType.Line;
                                series.BorderWidth = 2;
                                lineChart.Series.Add(series);
                        }
                        Legend legend = new Legend();
                        legend.Docking = Docking.Top;
                        legend.Alignment = System.Drawing.StringAlignment.Center;
                        lineChart.Legends.Add(legend);
                }

                void clearChart()
                {
                        if (lineChart != null)
                        {
                                lineChart.ChartAreas.Clear();
                                lineChart.Series.Clear();
                                lineChart.Legends.Clear();
                        }
                }

                public void InitRecord(Device device)
                {
                        this.device = device;
                        initControls();
                        if (device.TripData != null)
                        {
                                txt_error.Visibility = Visibility.Hidden;
                                dg_trips.Visibility = Visibility.Visible;
                                btn_query.IsEnabled = true;
                        }
                        else
                        {
                                txt_error.Visibility = Visibility.Visible;
                                dg_trips.Visibility = Visibility.Hidden;
                                btn_query.IsEnabled = false;
                        }
                }

                public void queryTrip()
                {
                        if (device.TripData != null)
                        {
                                DateTime start = (DateTime)dp_start.SelectedDate;
                                DateTime end = ((DateTime)dp_end.SelectedDate).AddDays(1);
                                Task.Factory.StartNew(() =>
                                {
                                        var trips = device.QueryTrip(start, end);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                                this.DataContext = trips.ConvertAll(t => t as EDSLot.Trip);
                                        }));
                                });
                        }
                }

                private void btn_query_Click(object sender, RoutedEventArgs e)
                {
                        queryTrip();
                }

                private void dg_trips_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                        var trip = dg_trips.SelectedItem as EDSLot.Trip;
                        if (trip != null)
                        {
                                Axis axisY = lineChart.ChartAreas[0].AxisY;
                                axisY.StripLines[0].IntervalOffset = 0;
                                axisY.StripLines[0].StripWidth = (double)(0.9 * trip.Ir);
                                axisY.StripLines[1].IntervalOffset = (double)(0.9 * trip.Ir);
                                axisY.StripLines[1].StripWidth = (double)(0.15 * trip.Ir);
                                axisY.StripLines[2].IntervalOffset = (double)(1.05 * trip.Ir);
                                axisY.StripLines[2].StripWidth = 10000;
                                Task.Factory.StartNew(new Action(() =>
                                {
                                        var data = device.QueryData(trip.Time.AddMinutes(-5), trip.Time.AddMinutes(1)).ConvertAll(r => r as Record);
                                        var xValues = data.ConvertAll(d => d.Time.ToString("HH:mm:ss"));
                                        var IaValues = data.ConvertAll(d => d.Ia);
                                        var IbValues = data.ConvertAll(d => d.Ib);
                                        var IcValues = data.ConvertAll(d => d.Ic);
                                        var INValues = data.ConvertAll(d => d.IN);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                                lineChart.Series["Ia"].Points.DataBindXY(xValues, IaValues);
                                                lineChart.Series["Ib"].Points.DataBindXY(xValues, IbValues);
                                                lineChart.Series["Ic"].Points.DataBindXY(xValues, IcValues);
                                                lineChart.Series["IN"].Points.DataBindXY(xValues, INValues);
                                        }));

                                }));
                        }
                }

                private void myChart_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
                {
                        hitTest = lineChart.HitTest(e.X, e.Y);
                        if (hitTest.ChartElementType == ChartElementType.DataPoint)
                        {
                                DataPoint point = hitTest.Object as DataPoint;
                                point.IsValueShownAsLabel = true;
                        }

                }

                private void myChart_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
                {
                        hitTest = lineChart.HitTest(e.X, e.Y);
                        if (hitTest.ChartElementType == ChartElementType.DataPoint)
                        {
                                DataPoint point = hitTest.Object as DataPoint;
                                point.IsValueShownAsLabel = false;
                        }

                }

        }
}
