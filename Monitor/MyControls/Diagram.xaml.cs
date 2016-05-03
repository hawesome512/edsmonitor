using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Expression.Shapes;

namespace Monitor
{
	/// <summary>
	/// Diagram.xaml 的交互逻辑
	/// </summary>
	public partial class Diagram : UserControl
	{
                public event EventHandler<EnterDeviceArgs> EnterDevice;
                List<Device> devices;
                int countInit;
                double factorX ;
                double factorY ;
		public Diagram()
		{
                        this.InitializeComponent();
		}

                void drawBase()
                {
                        addLine(new Point(50 * factorX, 20 * factorY), new Point(950 * factorX, 20 * factorY), Brushes.Red, 3);
                        addLine(new Point(75 * factorX, 20 * factorY), new Point(75 * factorX, 150 * factorY), Brushes.Red, 2);
                        addLine(new Point(75 * factorX, 150 * factorY+40), new Point(75 * factorX, 330 * factorY), Brushes.Red, 2);
                        addTriangle(new Thickness(75 * factorX - 10, 330 * factorY-20, 0, 0), 20, 20, 0, Brushes.Red);
                        addCircle(new Thickness(75 * factorX - 12.5, 149.75 * factorY, 0, 0), 25);
                        addCircle(new Thickness(75 * factorX - 12.5, 150 * factorY + 15, 0, 0), 25);
                        Rectangle rect = addRect(new Thickness(0, 150 * factorY-50, 20, 0), 40, 20, Brushes.Red);
                        rect.HorizontalAlignment = HorizontalAlignment.Right;
                        rect = addRect(new Thickness(0, 150 * factorY, 20,0), 40, 20, Brushes.SeaGreen);
                        rect.HorizontalAlignment = HorizontalAlignment.Right;
                        rect = addRect(new Thickness(0, 150 * factorY+50, 20, 0), 40, 20, Brushes.LightGray);
                        rect.HorizontalAlignment = HorizontalAlignment.Right;
                        addText(new Thickness(0, 150 * factorY - 30, 20, 0), "报警");
                        addText(new Thickness(0, 150 * factorY+20, 20, 0), "正常");
                        addText(new Thickness(0, 150 * factorY + 70, 20, 0), "无信号");

                        //备电支线
                        addLine(new Point(875 * factorX, 20 * factorY), new Point(875 * factorX, 330 * factorY-30), Brushes.Red, 2);
                        Ellipse ellipse= addCircle(new Thickness(875 * factorX-15, 330 * factorY-30, 0, 0), 30);
                        TextBlock tb= addText(new Thickness(0, 330 * factorY - 27, 125 * factorX-5, 0), "G");
                        tb.FontSize = 18;

                        countInit = DgmGrid.Children.Count;
                }

                public void InitDevices(List<Device> _devices)
                {
                        devices = _devices;
                        drawDevices();
                }

                void drawDevices()
                {
                        int count = devices.Count;
                        if (count == 0)
                                return;
                        int countNow = DgmGrid.Children.Count;
                        DgmGrid.Children.RemoveRange(countInit, countNow - countInit);
                        this.DataContext = devices;
                        double deltaX = 800*factorX / (count+1);
                        int offset = 5;
                        for (int i = 1; i <= count; i++)
                        {
                                double x = 75 * factorX + deltaX * i;
                                string path = string.Format("[{0}].State", i - 1);
                                if (devices[i - 1].DvType == DeviceType.ATS)
                                {
                                        addLine(new Point(x-15, 20 * factorY), new Point(x-15, 150 * factorY), Brushes.Red);
                                        addLine(new Point(x + 15, 20 * factorY), new Point(x + 15, 150 * factorY), Brushes.Red);
                                        addLine(new Point(x - 12, 20 * factorY), new Point(x + 12, 20 * factorY), Brushes.White, 5);
                                        ATS ats = addATS(new Thickness(x - 30, 150 * factorY - 10, 0, 0));
                                        ats.SetBinding(ATS.BackgroundProperty, Tool.addBinding(path, new StateToRectFillConverter()));
                                        Line lineOpen = addLine(new Point(x, 150 * factorY + 5), new Point(x, 150 * factorY + 40), Brushes.Black, 3);
                                        lineOpen.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToOpenConverter()));
                                        Line lineN = addLine(new Point(x - 15, 150 * factorY + 5), new Point(x, 150 * factorY + 40), Brushes.Black, 3);
                                        lineN.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToATS_NConverter()));
                                        Line lineR = addLine(new Point(x + 15, 150 * factorY + 5), new Point(x, 150 * factorY + 40), Brushes.Black, 3);
                                        lineR.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToATS_RConverter()));
                                }
                                else
                                {
                                        addLine(new Point(x, 20 * factorY), new Point(x, 150 * factorY), Brushes.Red);
                                        addLine(new Point(x - offset, 150 * factorY - offset), new Point(x + offset, 150 * factorY + offset), Brushes.Black);
                                        addLine(new Point(x - offset, 150 * factorY + offset), new Point(x + offset, 150 * factorY - offset), Brushes.Black);
                                        Rectangle rect = addRect(new Thickness(x - 20, 150 * factorY - 10, 0, 0), 40, 60, Brushes.LightGray);//SeaGreen/LightGray/Red
                                        rect.SetBinding(Rectangle.FillProperty, Tool.addBinding(path, new StateToRectFillConverter()));
                                        Line lineOpen = addLine(new Point(x - 15, 150 * factorY + 5), new Point(x, 150 * factorY + 40), Brushes.Black, 3);
                                        lineOpen.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToOpenConverter()));
                                        Line lineClose = addLine(new Point(x, 150 * factorY), new Point(x, 150 * factorY + 40), Brushes.Black, 3);
                                        lineClose.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToCloseConverter()));
                                }
                                addButton(new Thickness(x - 30, 0, 0, 0), devices[i - 1]);
                                Line line = addLine(new Point(x, 150 * factorY+40), new Point(x, 330*factorY), Brushes.Gray);
                                line.SetBinding(Line.StrokeProperty, Tool.addBinding(path, new StateToLineStrokeConverter()));
                                RegularPolygon triangle = addTriangle(new Thickness(x + 10, 330*factorY, 0, 0), 20, 20, 180, Brushes.Gray);
                                triangle.SetBinding(RegularPolygon.FillProperty, Tool.addBinding(path, new StateToLineStrokeConverter()));
                        }
                }

                ATS addATS(Thickness thickness)
                {
                        ATS ats = new ATS();
                        ats.Width = ats.Height = 60;
                        ats.VerticalAlignment = VerticalAlignment.Top;
                        ats.HorizontalAlignment = HorizontalAlignment.Left;
                        ats.Margin = thickness;
                        Grid.SetRow(ats, 1);
                        Grid.SetColumn(ats, 0);
                        DgmGrid.Children.Add(ats);
                        return ats;
                }

                Line addLine(Point p1, Point p2,Brush brush,double thick=2,string name=null)
                {
                        Line line = new Line();
                        line.X1 = p1.X;
                        line.Y1 = p1.Y;
                        line.X2 = p2.X;
                        line.Y2 = p2.Y;
                        line.StrokeThickness=thick;
                        line.Stroke = brush;
                        line.Name = name; 
                        Grid.SetRow(line, 1);
                        Grid.SetColumn(line, 0);
                        DgmGrid.Children.Add(line);
                        return line;
                }

                Rectangle addRect(Thickness thickness, double width, double height, Brush brush)
                {
                        Rectangle rect = new Rectangle();
                        rect.HorizontalAlignment = HorizontalAlignment.Left;
                        rect.VerticalAlignment = VerticalAlignment.Top;
                        rect.Margin = thickness;
                        rect.Width = width;
                        rect.Height = height;
                        rect.Fill = brush;
                        rect.Opacity = 0.7;
                        Grid.SetRow(rect, 1);
                        Grid.SetColumn(rect, 0);
                        DgmGrid.Children.Add(rect);
                        return rect;
                }

                RegularPolygon addTriangle(Thickness thickness, double width, double height,int angle,Brush brush)
                {
                        RegularPolygon pg = new RegularPolygon();
                        pg.HorizontalAlignment = HorizontalAlignment.Left;
                        pg.VerticalAlignment = VerticalAlignment.Top;
                        pg.Margin = thickness;
                        pg.PointCount = 3;
                        pg.Width = width;
                        pg.Height = height;
                        pg.Fill = brush;
                        RotateTransform rotate = new RotateTransform(angle);
                        pg.RenderTransform = rotate;
                        Grid.SetRow(pg, 1);
                        Grid.SetColumn(pg, 0);
                        DgmGrid.Children.Add(pg);
                        return pg;
                }

                Ellipse addCircle(Thickness thickness, double circle)
                {
                        Ellipse ellipse = new Ellipse();
                        ellipse.HorizontalAlignment = HorizontalAlignment.Left;
                        ellipse.VerticalAlignment = VerticalAlignment.Top;
                        ellipse.Margin = thickness;
                        ellipse.StrokeThickness = 2;
                        ellipse.Stroke = Brushes.Red;
                        ellipse.Width = ellipse.Height = circle;
                        Grid.SetRow(ellipse, 1);
                        Grid.SetColumn(ellipse, 0);
                        DgmGrid.Children.Add(ellipse);
                        return ellipse;
                }

                ImageButton addButton(Thickness thickness, Device dv)
                {
                        ImageButton btn = new ImageButton();
                        btn.HorizontalAlignment = HorizontalAlignment.Left;
                        btn.VerticalAlignment = VerticalAlignment.Bottom;
                        btn.Margin = thickness;
                        btn.Content = dv.Name;
                        btn.Width = 60;
                        btn.Height = 30;
                        btn.ImgPath = "/Images/detial.png";
                        btn.Click += new RoutedEventHandler((o, s) => { EnterDevice(this,new EnterDeviceArgs(dv)); });
                        btn.Template=(ControlTemplate)Application.Current.Resources["ImageButtonTemplate"];
                        Grid.SetRow(btn, 0);
                        Grid.SetColumn(btn, 0);
                        DgmGrid.Children.Add(btn);
                        return btn;
                }

                TextBlock addText(Thickness thickness, string text)
                {
                        TextBlock tb=new TextBlock();
                        tb.HorizontalAlignment=HorizontalAlignment.Right;
                        tb.VerticalAlignment=VerticalAlignment.Top;
                        tb.Margin=thickness;
                        tb.Text=text;
                        //tb.Foreground = Brushes.Black;
                        Grid.SetRow(tb, 1);
                        Grid.SetColumn(tb, 0);
                        DgmGrid.Children.Add(tb);
                        return tb;
                }

                private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
                {
                        //int countNow = DgmGrid.Children.Count;
                        //DgmGrid.Children.RemoveRange(countInit, countNow - countInit);
                        DgmGrid.Children.Clear();
                        factorX = e.NewSize.Width / 1000;
                        factorY = e.NewSize.Height / 500;
                        drawBase();
                        if (devices != null)
                        {
                                drawDevices();
                        }
                }
	}


        public class EnterDeviceArgs : EventArgs
        {
                public Device Dv;
                public EnterDeviceArgs(Device dv)
                {
                        Dv = dv;
                }
        }

}