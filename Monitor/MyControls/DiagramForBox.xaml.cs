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
        public partial class DiagramForBox : UserControl
        {
                public event EventHandler<EnterDeviceArgs> EnterDevice;
                double factorX;
                double factorY;
                List<Device> devices;
                public DiagramForBox()
                {
                        this.InitializeComponent();
                }

                public void InitDiagram(List<Device> _devices)
                {
                        devices = _devices;
                        this.DataContext = devices;
                        draw();
                }

                private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
                {
                        try
                        {
                                //待优化，多重绑定冲突
                                DgmGrid.Children.Clear();
                        }
                        catch
                        {
                        }
                        factorX = e.NewSize.Width / 1000;
                        factorY = e.NewSize.Height / 500;
                        if (devices != null)
                                draw();
                }

                void draw()
                {
                        addTriangle(new Thickness(50 * factorX - 10, 25 * factorY + 20, 0, 0), 20, 20, 180);
                        addLine(new Point(50 * factorX, 25 * factorY), new Point(50 * factorX, 250 * factorY - 25)).Stroke = Brushes.Red;
                        addCircle(new Thickness(50 * factorX - 15, 250 * factorY - 25, 0, 0), 30, 30);
                        addCircle(new Thickness(50 * factorX - 15, 250 * factorY - 5, 0, 0), 30, 30);
                        addLine(new Point(50 * factorX, 250 * factorY + 25), new Point(50 * factorX, 475 * factorY)).Stroke = Brushes.Red;
                        addLine(new Point(50 * factorX, 475 * factorY), new Point(250 * factorX, 475 * factorY)).Stroke = Brushes.Red;

                        addBreaker(450 * factorY, new Thickness(250 * factorX - 20, 25 * factorY, 0, 0), 1, false);
                        addMessure(new Thickness(250 * factorX - 30, 75 * factorY, 0, 0),8);
                        Line line = addLine(new Point(240 * factorX, 25 * factorY), new Point(885 * factorX, 25 * factorY), 3);
                        line.SetBinding(Line.StrokeProperty, Tool.addBinding("[0].State", new StateToLineStrokeConverter()));

                        addBreaker(180 * factorY, new Thickness(500 * factorX - 20, 25 * factorY, 0, 0), 2, false);
                        line = addLine(new Point(425 * factorX, 205 * factorY), new Point(575 * factorX, 205 * factorY));
                        //line.SetBinding(Line.StrokeProperty, Tool.addBinding("[1].State", new StateToLineStrokeConverter()));
                        line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(new List<string> { "[0].State", "[1].State" }));
                        addBreaker(270 * factorY, new Thickness(425 * factorX - 20, 205 * factorY, 0, 0), 4, true);
                        addBreaker(270 * factorY, new Thickness(575 * factorX - 20, 205 * factorY, 0, 0), 5, true);

                        addBreaker(180 * factorY, new Thickness(800 * factorX - 20, 25 * factorY, 0, 0), 3, false);
                        line = addLine(new Point(725 * factorX, 205 * factorY), new Point(875 * factorX, 205 * factorY));
                        //line.SetBinding(Line.StrokeProperty, Tool.addBinding("[2].State", new StateToLineStrokeConverter()));
                        line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(new List<string> { "[0].State", "[2].State" }));
                        addBreaker(270 * factorY, new Thickness(725 * factorX - 20, 205 * factorY, 0, 0), 6, true);
                        addBreaker(270 * factorY, new Thickness(875 * factorX - 20, 205 * factorY, 0, 0), 7, true);
                }

                Breaker addBreaker(double height, Thickness margin, byte address, bool hasTriangle, int angle = 0)
                {
                        Breaker breaker = new Breaker();
                        breaker.txt_name.Content = devices[0].Name;
                        Device device = devices.Find(d => d.Address == address);
                        breaker.txt_name.MouseLeftButtonDown += new MouseButtonEventHandler((o, s) =>
                        {
                                EnterDevice(this, new EnterDeviceArgs(device));
                        });
                        breaker.Width = 90;
                        breaker.Height = height;
                        breaker.Margin = margin;
                        breaker.VerticalAlignment = VerticalAlignment.Top;
                        breaker.HorizontalAlignment = HorizontalAlignment.Left;
                        breaker.Triangle.Visibility = hasTriangle ? Visibility.Visible : Visibility.Hidden;
                        RotateTransform rotate = new RotateTransform(angle);
                        rotate.CenterX = breaker.Width / 2;
                        rotate.CenterY = breaker.Height / 2;
                        breaker.RenderTransform = rotate;
                        breaker.InitBreaker(device);
                        DgmGrid.Children.Add(breaker);
                        return breaker;
                }

                Messure addMessure(Thickness margin,byte address)
                {
                        Device device = devices.Find(d => d.Address == address);
                        Messure ms = new Messure();
                        ms.InitMessure(device);
                        ms.Width = 160;
                        ms.Height = 90;
                        ms.HorizontalAlignment = HorizontalAlignment.Left;
                        ms.VerticalAlignment = VerticalAlignment.Top;
                        ms.Margin = margin;
                        DgmGrid.Children.Add(ms);
                        return ms;
                }

                Line addLine(Point p1, Point p2, double thick = 2, string name = null)
                {
                        Line line = new Line();
                        line.X1 = p1.X;
                        line.Y1 = p1.Y;
                        line.X2 = p2.X;
                        line.Y2 = p2.Y;
                        line.StrokeThickness = thick;
                        line.Stroke = Brushes.SeaGreen;
                        line.Name = name;
                        DgmGrid.Children.Add(line);
                        return line;
                }

                RegularPolygon addTriangle(Thickness thickness, double width, double height, int angle = 0)
                {
                        RegularPolygon pg = new RegularPolygon();
                        pg.HorizontalAlignment = HorizontalAlignment.Left;
                        pg.VerticalAlignment = VerticalAlignment.Top;
                        pg.Margin = thickness;
                        pg.PointCount = 3;
                        pg.Width = width;
                        pg.Height = height;
                        pg.Stroke = Brushes.Red;
                        RotateTransform rotate = new RotateTransform(angle);
                        rotate.CenterX = pg.Width / 2;
                        rotate.CenterY = pg.Height / 2;
                        pg.RenderTransform = rotate;
                        DgmGrid.Children.Add(pg);
                        return pg;
                }

                Ellipse addCircle(Thickness margin, double width, double height)
                {
                        Ellipse circle = new Ellipse();
                        circle.Width = width;
                        circle.Height = height;
                        circle.Margin = margin;
                        circle.HorizontalAlignment = HorizontalAlignment.Left;
                        circle.VerticalAlignment = VerticalAlignment.Top;
                        circle.Stroke = Brushes.Red;
                        DgmGrid.Children.Add(circle);
                        return circle;
                }

                TextBlock addText(Thickness thickness, string text)
                {
                        TextBlock tb = new TextBlock();
                        tb.HorizontalAlignment = HorizontalAlignment.Right;
                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.Margin = thickness;
                        tb.Text = text;
                        tb.Foreground = Brushes.Black;
                        DgmGrid.Children.Add(tb);
                        return tb;
                }
        }
}