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
                int countInit;
		public Diagram()
		{
                        this.InitializeComponent();
                        countInit = DgmGrid.Children.Count;
		}
                
                public void InitDevices(List<Device> devices)
                {
                        int countNow = DgmGrid.Children.Count;
                        DgmGrid.Children.RemoveRange(countInit, countNow - countInit);
                        this.DataContext = devices;
                        int count = devices.Count;
                        int deltaX = 800 / count;
                        int offset = 5;
                        for (int i = 1; i <= count; i++)
                        {
                                int x=75+deltaX*i;
                                addLine(new Point(x,20),new Point(x,150),Brushes.Red);
                                addLine(new Point(x - offset, 150 - offset), new Point(x + offset, 150 + offset), Brushes.Black);
                                addLine(new Point(x - offset, 150 + offset), new Point(x + offset, 150 - offset), Brushes.Black);
                                addButton(new Thickness(x - 30, 0, 0, 0), devices[i-1]);
                                string path = string.Format("[{0}].State", i - 1);
                                Rectangle rect= addRect(new Thickness(x - 20, 150 - 10, 0, 0), 40, 60, Brushes.LightGray);//SeaGreen/LightGray/Red
                                rect.SetBinding(Rectangle.FillProperty, Tool.addBinding(path, new StateToRectFillConverter()));
                                Line lineOpen = addLine(new Point(x - 15, 150 + 5), new Point(x, 190), Brushes.Black, 3);
                                lineOpen.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToOpenConverter()));
                                Line lineClose = addLine(new Point(x, 150), new Point(x, 190), Brushes.Black, 3);
                                lineClose.SetBinding(Line.OpacityProperty, Tool.addBinding(path, new StateToCloseConverter()));
                                Line line=addLine(new Point(x, 190), new Point(x, 330), Brushes.Gray);
                                line.SetBinding(Line.StrokeProperty, Tool.addBinding(path, new StateToLineStrokeConverter()));
                                RegularPolygon triangle= addTriangle(new Thickness(x+10, 330, 0, 0), 20, 20);
                                triangle.SetBinding(RegularPolygon.FillProperty, Tool.addBinding(path, new StateToLineStrokeConverter()));
                        }
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

                RegularPolygon addTriangle(Thickness thickness, double width, double height)
                {
                        RegularPolygon pg = new RegularPolygon();
                        pg.HorizontalAlignment = HorizontalAlignment.Left;
                        pg.VerticalAlignment = VerticalAlignment.Top;
                        pg.Margin = thickness;
                        pg.PointCount = 3;
                        pg.Width = width;
                        pg.Height = height;
                        pg.Fill = Brushes.Gray;
                        RotateTransform rotate = new RotateTransform(180);
                        pg.RenderTransform = rotate;
                        Grid.SetRow(pg, 1);
                        Grid.SetColumn(pg, 0);
                        DgmGrid.Children.Add(pg);
                        return pg;
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