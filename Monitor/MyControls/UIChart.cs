using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Monitor
{
        class UIChart
        {
                private Chart MyChart;
                string[] items = new string[] { "A", "B", "C" };
                public UIChart(Chart chart)
                {
                        MyChart = chart;
                        Clear();
                        initChart();
                        initLegend();
                        foreach (string it in items)
                        {
                                addArea(it);
                                addSeries("U".ToString() + it.ToLower(), it);
                                addSeries("I".ToString() + it.ToLower(), it);
                        }
                }

                private void Clear()
                {
                        MyChart.Legends.Clear();
                        MyChart.ChartAreas.Clear();
                        MyChart.Series.Clear();
                }

                private void initLegend()
                {
                        Legend legend = new Legend("default");
                        legend.Docking = Docking.Top;
                        legend.Alignment = StringAlignment.Center;
                        MyChart.Legends.Add(legend);
                }

                private void initChart()
                {
                        MyChart.BackColor = Color.Transparent;
                        //MyChart.BackColor = Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
                        //MyChart.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
                        //MyChart.BackSecondaryColor = Color.White;
                        //MyChart.BorderlineColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
                        //MyChart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
                        //MyChart.BorderlineWidth = 2;
                        //MyChart.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.Emboss;
                        MyChart.Palette = ChartColorPalette.BrightPastel;
                }

                void addArea(string name)
                {
                        ChartArea area = new ChartArea(name);
                        area.CursorX.IsUserEnabled = true;
                        area.CursorX.IsUserSelectionEnabled = true;
                        area.AxisY.TextOrientation = TextOrientation.Horizontal;
                        area.AxisX.MajorGrid.LineColor = Color.LightGray;
                        area.AxisY.MajorGrid.LineColor = Color.LightGray;
                        area.AxisY.MajorGrid.Enabled = false;
                        area.AxisY2.MajorGrid.LineColor = Color.LightGray;
                        area.AxisX.LineColor = Color.LightGray;
                        //area.AxisY.Maximum = 1000;
                        area.AxisY2.Maximum = 400;
                        //if (name == "A")
                        //{
                        //        area.AxisY.Title = "I(A)";
                        //        area.AxisY2.Title = "U(V)";
                        //        area.AxisY.TitleAlignment = area.AxisY2.TitleAlignment = StringAlignment.Far;
                        //}
                        if (name != "C")
                        {
                                area.AxisX.LabelStyle.Enabled = false;
                                area.AxisX.MajorTickMark.Enabled = false;
                                area.AxisX.ScrollBar.Enabled = false;
                        }

                        area.AxisY2.TextOrientation = TextOrientation.Horizontal;
                        MyChart.ChartAreas.Add(area);

                        area.AlignWithChartArea = "A";
                        area.AlignmentStyle = AreaAlignmentStyles.AxesView | AreaAlignmentStyles.Cursor | AreaAlignmentStyles.PlotPosition;
                        area.AlignmentOrientation = AreaAlignmentOrientations.Vertical;
                }

                void addSeries(string name, string area)
                {
                        bool isU = name.Contains("U");
                        Series series = new Series(name);
                        series.ChartArea = area;
                        series.ChartType = isU ? SeriesChartType.Column : SeriesChartType.Line;
                        series.BorderWidth = 2;
                        series.Legend = "default";
                        series.YAxisType = isU ? AxisType.Secondary : AxisType.Primary;
                        Random random = new Random();
                        MyChart.Series.Add(series);
                }

                public void SetData(int In,int Un)
                {
                        Random random = new Random();
                        foreach (Series s in MyChart.Series)
                        {
                                DateTime dt = DateTime.Today;
                                string[] times = Enumerable.Repeat(0, 120).Select(r => (dt = dt.AddMinutes(12)).ToShortTimeString()).ToArray();
                                int nMin = (int)(0.95*Un);
                                int nMax = (int)(1.05*Un);
                                if (s.Name.Contains("I"))
                                {
                                        nMin = (int)(0.6 * In);
                                        nMax = (int)(0.8 * In);
                                }
                                s.Points.DataBindXY(times, Enumerable.Repeat(0, 120).Select(r => random.Next(nMin, nMax)).ToArray());
                        }
                        foreach (ChartArea area in MyChart.ChartAreas)
                        {
                                area.AxisY.Interval = area.AxisY.Maximum / 4;
                        }
                }

        }
}
