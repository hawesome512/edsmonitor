using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AutoSizeDemo
{
        class UIChart
        {
                private Chart MyChart;
                public UIChart(Chart chart)
                {
                        MyChart = chart;
                        initChart();
                        initLegend();
                        addArea("A");
                        addArea("B");
                        addArea("C");
                        addSeries("Ia", "A");
                        addSeries("Ib", "B");
                        addSeries("Ic", "C");
                        addSeries("Ua", "A");
                        addSeries("Ub", "B");
                        addSeries("Uc", "C");
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
                        MyChart.BackColor = Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
                        MyChart.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
                        MyChart.BackSecondaryColor = Color.White;
                        MyChart.BorderlineColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
                        MyChart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
                        MyChart.BorderlineWidth = 2;
                        MyChart.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.Emboss;
                        MyChart.Palette = ChartColorPalette.SemiTransparent;
                }

                void addArea(string name)
                {
                        ChartArea area = new ChartArea(name);
                        area.CursorX.IsUserEnabled = true;
                        area.CursorX.IsUserSelectionEnabled = true;
                        area.AxisY.TextOrientation = TextOrientation.Horizontal;
                        area.AxisX.MajorGrid.LineColor = Color.LightGray;
                        area.AxisY.MajorGrid.LineColor = Color.LightGray;
                        area.AxisY2.MajorGrid.LineColor = Color.LightGray;
                        area.AxisX.LineColor = Color.LightGray;
                        area.AxisY.Maximum = 1000;
                        area.AxisY2.Maximum = 250;
                        if (name == "A")
                        {
                                area.AxisY.Title = "I(A)";
                                area.AxisY2.Title = "U(V)";
                                area.AxisY.TitleAlignment = area.AxisY2.TitleAlignment = StringAlignment.Far;
                        }
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

                void addSeries(string name,string area)
                {
                        bool isU = name.Contains("U");
                        Series series = new Series(name);
                        series.ChartArea = area;
                        series.ChartType =isU?SeriesChartType.Column:SeriesChartType.Line;
                        series.BorderWidth = 2;
                        series.Legend = "default";
                        series.YAxisType = isU ? AxisType.Secondary : AxisType.Primary;
                        Random random=new Random();
                        int nMin = isU ? 120 : 600;
                        int nMax = isU ? 135 : 800;
                        DateTime dt = DateTime.Today;
                        string[] times = Enumerable.Repeat(0, 120).Select(s => (dt=dt.AddMinutes(12)).ToShortTimeString()).ToArray();
                        series.Points.DataBindXY(times,Enumerable.Repeat(0, 120).Select(s => random.Next(nMin, nMax)).ToArray());
                        MyChart.Series.Add(series);
                }

        }
}
