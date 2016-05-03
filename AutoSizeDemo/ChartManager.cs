//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Media.Imaging;
//using System.Drawing;
//using System.Windows.Forms;
//using System.Windows.Forms.DataVisualization.Charting;

//namespace AutoSizeDemo
//{
//        /// <summary>
//        /// 修改记录
//        /// =====2015/1/27===============================
//        /// 将所有Title注释，另外设置显示区域
//        /// </summary>
//        class ChartManager
//        {
//                public Chart MyChart;

//                private double multiple = 1.0;
//                private int? height;
//                //最起始时的高度，缩放以此为标准最小值
//                public int? Height
//                {
//                        get
//                        {
//                                return height;
//                        }
//                        set
//                        {
//                                if (height == null)
//                                        height = value;
//                                else
//                                        height = height;
//                        }
//                }

//                public ChartManager(Chart chart)
//                {
//                        MyChart = chart;
//                        initChart();
//                        initLegend();
//                }

//                private void initLegend()
//                {
//                        Legend legend = new Legend("default");
//                        legend.Docking = Docking.Top;
//                        legend.Alignment = StringAlignment.Center;
//                        MyChart.Legends.Add(legend);
//                }

//                private void initChart()
//                {
//                        MyChart.BackColor = Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
//                        MyChart.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
//                        MyChart.BackSecondaryColor = Color.White;
//                        MyChart.BorderlineColor = Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
//                        MyChart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
//                        MyChart.BorderlineWidth = 2;
//                        MyChart.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.Emboss;
//                }

//                public void Add(ICurve curve, int index)
//                {
//                        string name = curve.Name.ToString();
//                        ChartArea area = new ChartArea(name);
//                        area.CursorX.IsUserEnabled = true;
//                        area.CursorX.IsUserSelectionEnabled = true;
//                        area.CursorX.Interval = 0.05;
//                        area.AxisX.LabelStyle.Format = "0.##";
//                        area.AxisY.TextOrientation = TextOrientation.Horizontal;
//                        area.AxisX.MajorGrid.LineColor = Color.LightGray;
//                        area.AxisY.MajorGrid.LineColor = Color.LightGray;
//                        area.AxisY2.MajorGrid.LineColor = Color.LightGray;
//                        area.AxisX.LineColor = Color.LightGray;
//                        area.AxisX.Minimum = 0;
//                        if (curve.Name.CurveType == "A")
//                                area.AxisY.LabelStyle.Format = "{0:0,.#}";
//                        MyChart.ChartAreas.Insert(index, area);

//                        Series series = new Series(name);
//                        series.ChartType = SeriesChartType.Line;
//                        series.BorderWidth = 2;
//                        series.Legend = "default";
//                        series.ChartArea = name;
//                        int startTime = curve.ShowNodes.First().No;
//                        series.Points.DataBindXY(curve.ShowNodes.ConvertAll(n => Math.Round((n.No - startTime) * curve.DeltaTime, 3)), curve.ShowNodes.ConvertAll(n => n.Value));
//                        MyChart.Series.Insert(index, series);


//                        reDrawing();
//                }

//                public void Remove()
//                {
//                        MyChart.Series.Clear();
//                        MyChart.ChartAreas.Clear();
//                }

//                public void Remove(int index)
//                {
//                        MyChart.Series.RemoveAt(index);
//                        MyChart.ChartAreas.RemoveAt(index);
//                }

//                private string getMark(ICurve curve)
//                {
//                        string mark = string.Empty;
//                        switch (curve.Name.CurveType)
//                        {
//                                case "A":
//                                        mark = "電流[kA]";
//                                        break;
//                                case "V":
//                                        mark = "電壓[V]";
//                                        break;
//                        }
//                        foreach (KeyValuePair<string, string> kv in curve.Params)
//                        {
//                                mark += string.Format("\r\n\r\n{0,-5}:{1}", kv.Key, kv.Value);
//                        }
//                        return mark;
//                }

//                private void reLocate()
//                {
//                        List<ChartArea> vAreas = MyChart.ChartAreas.ToList().FindAll(a => a.Visible == true);
//                        string area0 = vAreas[0].Name;
//                        double size = vAreas[0].AxisX.ScaleView.Size;
//                        double position = vAreas[0].AxisX.ScaleView.Position;
//                        int count = vAreas.Count;
//                        for (int i = 0; i < count; i++)
//                        {
//                                ChartArea area = vAreas[i];
//                                if (area.AxisX.ScaleView.Size != size)
//                                {
//                                        area.AxisX.ScaleView.Size = size;
//                                        area.AxisX.ScaleView.Position = position;
//                                }
//                                if (showType == ShowTypes.Scatter)
//                                {
//                                        area.AxisY.Maximum = double.NaN;
//                                        area.AxisY.Minimum = double.NaN;
//                                        area.AxisY.Interval = double.NaN;
//                                        area.AxisY2.Maximum = double.NaN;
//                                        area.AxisY2.Minimum = double.NaN;
//                                        area.AxisY2.Interval = double.NaN;
//                                        area.RecalculateAxesScale();
//                                }
//                                else
//                                {
//                                        int max1 = 1000;
//                                        int max2 = 100;
//                                        var ses = MyChart.Series.ToList<Series>().FindAll(s => s.ChartArea == area.Name);
//                                        foreach (Series se in ses)
//                                        {
//                                                double m = se.Points.ToList().Max(p => Math.Abs(p.YValues.First()));
//                                                if (se.Name.Split('_').Last() == "A")
//                                                {
//                                                        int dt = m <= 5000 ? 1000 : 5000;
//                                                        max1 = Math.Max(max1, ((int)m / dt + 1) * dt);
//                                                }
//                                                else
//                                                        max2 = Math.Max(max2, ((int)m / 100 + 1) * 100);
//                                        }

//                                        area.AxisY.Maximum = max1;
//                                        area.AxisY.Minimum = -max1;
//                                        area.AxisY.Interval = max1 / 4;
//                                        area.AxisY2.Maximum = max2;
//                                        area.AxisY2.Minimum = -max2;
//                                        area.AxisY2.Interval = max2 / 4;
//                                        area.RecalculateAxesScale();
//                                }

//                                area.AlignWithChartArea = area0;
//                                area.AlignmentStyle = AreaAlignmentStyles.AxesView | AreaAlignmentStyles.Cursor | AreaAlignmentStyles.PlotPosition;
//                                area.AlignmentOrientation = AreaAlignmentOrientations.Vertical;
//                                area.AxisX.ScrollBar.Enabled = false;

//                                float delta = 85f / count;
//                                if (i != count - 1)
//                                {
//                                        area.Position = new ElementPosition(1, 10 + i * delta, 98, delta);//取消titles后85→98，下同
//                                        area.AxisX.LabelStyle.Enabled = false;
//                                        area.AxisX.MajorTickMark.Enabled = false;
//                                }
//                                else
//                                {
//                                        area.Position = new ElementPosition(1, 10 + i * delta, 98, delta + 5);
//                                        area.AxisX.LabelStyle.Enabled = true;
//                                        area.AxisX.ScrollBar.Enabled = true;
//                                }
//                        }
//                }

//                public void Rebinding(ICurve curve)
//                {
//                        Series series = MyChart.Series[curve.Name.ToString()];
//                        int startTime = curve.ShowNodes.First().No;
//                        series.Points.DataBindXY(curve.ShowNodes.ConvertAll(n => Math.Round((n.No - startTime) * curve.DeltaTime, 3)), curve.ShowNodes.ConvertAll(n => n.Value));
//                        //series.Tag = getMark(curve);
//                }

//                private void reDrawing()
//                {
//                        MyChart.Height = (int)height;
//                        switch (showType)
//                        {
//                                case ShowTypes.Scatter:
//                                        reDrawingInScatter();
//                                        break;
//                                case ShowTypes.Assembly:
//                                        reDrawingInAssembly();
//                                        break;
//                                case ShowTypes.Whole:
//                                        reDrawingInWhole();
//                                        break;
//                        }
//                        reLocate();
//                }

//                private void reDrawingInScatter()
//                {
//                        foreach (Series s in MyChart.Series)
//                        {
//                                string name = s.Name;
//                                s.ChartArea = name;
//                                s.YAxisType = AxisType.Primary;
//                                MyChart.ChartAreas[name].Visible = true;
//                        }
//                }

//                private void reDrawingInAssembly()
//                {
//                        for (int i = 0; i < MyChart.Series.Count; i++)
//                        {
//                                Series s = MyChart.Series[i];
//                                s.YAxisType = AxisType.Primary;
//                                s.ChartArea = s.Name;
//                                MyChart.ChartAreas[s.Name].Visible = true;
//                                if (i > 0)
//                                {
//                                        Series s1 = MyChart.Series[i - 1];
//                                        if (isMatch(s.Name, s1.Name))
//                                        {
//                                                //匹配时根据排序规则，电压在前电流在后，以后出现温度时再考虑新方法
//                                                s1.ChartArea = s.Name;
//                                                s1.YAxisType = AxisType.Secondary;
//                                                MyChart.ChartAreas[s1.Name].Visible = false;
//                                        }
//                                }
//                        }
//                }

//                private void reDrawingInWhole()
//                {
//                        foreach (Series s in MyChart.Series)
//                        {
//                                s.ChartArea = MyChart.ChartAreas.First().Name;
//                                //后期加入温度曲线时此处可能有变动
//                                s.YAxisType = s.Name.Split('_')[2] == "A" ? AxisType.Primary : AxisType.Secondary;
//                                MyChart.ChartAreas[s.Name].Visible = false;
//                        }
//                        MyChart.ChartAreas.First().Visible = true;
//                }

//                private bool isMatch(string name1, string name2)
//                {
//                        string[] ns1 = name1.Split('_');
//                        string[] ns2 = name2.Split('_');
//                        if (ns1[0] == ns2[0] && ns1[1] == ns2[1])
//                                return true;
//                        else
//                                return false;
//                }

//                public BitmapImage GetPrintImg()
//                {
//                        System.IO.MemoryStream stream = new System.IO.MemoryStream();
//                        MyChart.Serializer.Content = SerializationContents.All;
//                        MyChart.Serializer.Save(stream);

//                        Chart pChart = new Chart();
//                        stream.Seek(0, System.IO.SeekOrigin.Begin);
//                        pChart.Serializer.Load(stream);
//                        stream.Close();

//                        pChart.Legends.Clear();
//                        //pChart.Titles.Clear();
//                        pChart.BackColor = Color.White;
//                        pChart.BorderlineWidth = 0;
//                        pChart.BorderSkin.SkinStyle = BorderSkinStyle.None;

//                        pChart.PaletteCustomColors = new Color[] { Color.Black };
//                        pChart.Palette = ChartColorPalette.None;

//                        foreach (ChartArea area in pChart.ChartAreas)
//                        {
//                                area.CursorX.LineColor = Color.FromArgb(255, 255, 255, 255);
//                                area.AxisX.ScaleView.ZoomReset(10);
//                        }

//                        pChart.Height = (int)(pChart.Width / 1.15);

//                        System.IO.MemoryStream stream1 = new System.IO.MemoryStream();
//                        pChart.SaveImage(stream1, ChartImageFormat.Bmp);
//                        BitmapImage bmp = new BitmapImage();

//                        bmp.BeginInit();
//                        bmp.StreamSource = stream1;
//                        bmp.EndInit();
//                        return bmp;
//                }

//                public Chart GetSaveImg()
//                {
//                        System.IO.MemoryStream stream = new System.IO.MemoryStream();
//                        MyChart.Serializer.Content = SerializationContents.All;
//                        MyChart.Serializer.Save(stream);

//                        Chart pChart = new Chart();
//                        stream.Seek(0, System.IO.SeekOrigin.Begin);
//                        pChart.Serializer.Load(stream);
//                        stream.Close();

//                        pChart.BackColor = Color.White;
//                        pChart.BorderlineWidth = 0;
//                        pChart.BorderSkin.SkinStyle = BorderSkinStyle.None;

//                        pChart.Height = (int)(pChart.Width / 1.15);
//                        return pChart;
//                }
//        }
//}
