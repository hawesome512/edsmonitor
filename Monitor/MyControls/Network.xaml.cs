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
using System.Threading.Tasks;

namespace Monitor
{
        /// <summary>
        /// Diagram.xaml 的交互逻辑
        /// </summary>
        public partial class Network : UserControl
        {
                public event EventHandler<EnterDeviceArgs> EnterDevice;
                double factorX;
                double factorY;
                List<Device> devices;
                public Network()
                {
                        this.InitializeComponent();
                }

                public void InitNetwork(List<Device> _devices)
                {
                        DgmGrid.Children.Clear();
                        devices = _devices;
                        this.DataContext = devices;
                        draw();
                }

                public void ClearNetwork()
                {
                        this.DataContext = null;
                        DgmGrid.Children.Clear();
                }

                private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
                {
                        DgmGrid.Children.Clear();
                        factorX = e.NewSize.Width / 1000;
                        factorY = e.NewSize.Height / 500;
                        if (devices != null)
                                draw();
                }

                void draw(bool showImage=true)
                {
                        if (devices.Count == 0)
                        {
                                return;
                        }
                        switch (devices[0].ZID)
                        {
                                case 1:
                                        drawLaGanXiang(showImage);
                                        break;
                                case 2:
                                        drawChengYu(showImage);
                                        break;
                                default:
                                        if (devices[0].ZoneName == "模拟演示")
                                        {
                                                drawSimulation(showImage);
                                        }
                                        else
                                        {
                                                drawTest();
                                        }
                                        break;
                        }
                        addSwitch(showImage);
                }

                private void drawSimulation(bool showImage)
                {
                        //电源进线
                        addPower(new Thickness(62.5 * factorX-20, 15 * factorY, 0, 0), 460 * factorY,180);
                        addLine(new Point(62.5 * factorX, 15 * factorY), new Point(187.5 * factorX, 15 * factorY),1,0);
                        addLine(new Point(187.5 * factorX, 15 * factorY), new Point(187.5 * factorX, 25 * factorY),1,0);
                        addLine(new Point(110 * factorX, 25 * factorY), new Point(202.5 * factorX, 25 * factorY), 3,0);
                        //备用电源
                        addLine(new Point(812.5 * factorX + 15, 15 * factorY), new Point(812.5 * factorX + 15, 150 * factorY - 20), 1,0);
                        addLine(new Point(812.5 * factorX + 15, 15 * factorY), new Point(937.5 * factorX, 15 * factorY), 1,0);
                        addLine(new Point(937.5 * factorX, 15 * factorY), new Point(937.5 * factorX, 475 * factorY), 1,0);
                        addLine(new Point(812.5 * factorX + 10, 25 * factorY), new Point(890 * factorX, 25 * factorY), 3,0);
                        addCircle(new Thickness(937.5 * factorX - 20, 475 * factorY - 40, 0, 0), 40, 40);
                        addText(new Thickness(937.5 * factorX - 20, 475 * factorY - 40, 0, 0), 40, 40, "G");

                        //开关1&ACREL12
                        addBreaker(200 * factorY, new Thickness(187.5 * factorX - 100, 25 * factorY, 0, 0), 1,showImage,false, false, 0);
                        addLine(new Point(187.5 * factorX, 225 * factorY), new Point(245 * factorX, 225 * factorY),1,1);
                        addLine(new Point(245 * factorX, 25 * factorY), new Point(245 * factorX, 225 * factorY),1,1);
                        addMeter(270*factorY+45,new Thickness(187.5 * factorX - 75, 225 * factorY-45, 0, 0),12);
                        addLine(new Point(235 * factorX, 25 * factorY), new Point(452.5 * factorX, 25 * factorY), 3,1);
                        //开关2&MIC4
                        addBreaker(200 * factorY, new Thickness(312.5 * factorX - 100, 25 * factorY, 0, 0), 2,showImage, false, true, 0);
                        addMIC(250 * factorY, new Thickness(312.5 * factorX - 100, 225 * factorY, 0, 0),4,showImage);
                        //开关3
                        addBreaker(200 * factorY, new Thickness(437.5 * factorX - 100, 25 * factorY, 0, 0), 3,showImage, false, false, 0);
                        addLine(new Point(437.5 * factorX, 225 * factorY), new Point(495 * factorX, 225 * factorY),1,3);
                        addLine(new Point(495 * factorX, 25 * factorY), new Point(495 * factorX, 225 * factorY),1,3);
                        addLine(new Point(485 * factorX, 25 * factorY), new Point(812.5 * factorX-10, 25 * factorY), 3,3);
                        addLine(new Point(812.5 * factorX - 15, 25 * factorY), new Point(812.5 * factorX - 15, 150 * factorY - 20),1,3);
                        //开关5
                        addBreaker(200 * factorY, new Thickness(562.5 * factorX - 100, 25 * factorY, 0, 0), 5,showImage, true, true, 0);
                        //开关6
                        addBreaker(200 * factorY, new Thickness(687.5 * factorX - 100, 25 * factorY, 0, 0), 6,showImage, true, true, 0);
                        //开关8
                        addBreaker(200 * factorY, new Thickness(812.5 * factorX - 100, 275 * factorY, 0, 0), 8,showImage, true, true, 0);
                        addSlider(8);
                        //开关9
                        addBreaker(200 * factorY, new Thickness(687.5 * factorX - 100, 275 * factorY, 0, 0), 9,showImage, true, true, 0);
                        //开关10
                        addBreaker(200 * factorY, new Thickness(562.5 * factorX - 100, 275 * factorY, 0, 0), 10,showImage, true, true, 0);
                        //开关11
                        addBreaker(200 * factorY, new Thickness(437.5 * factorX - 100, 275 * factorY, 0, 0), 11,showImage, true, true, 0);
                        //ATS7
                        addLine(new Point(812.5 * factorX, 150 * factorY + 20), new Point(812.5 * factorX, 275 * factorY),1,7);
                        addLine(new Point(360 * factorX, 275 * factorY), new Point(890 * factorX, 275 * factorY), 3,7);
                        addATS(new Thickness(812.5 * factorX - 50, 150 * factorY - 50, 0, 0),7,showImage);
                }

                private void drawTest()
                {
                        Task.Factory.StartNew(new Action(() =>
                        {
                                while (true)
                                {
                                        System.Threading.Thread.Sleep(1000);
                                        if (devices[0].NeedParamsNum==0)
                                        {
                                                this.Dispatcher.Invoke(new Action(() =>
                                                {
                                                        EnterDevice(this, new EnterDeviceArgs(devices[0]));
                                                }));
                                                break;
                                        }
                                }
                        }));
                }

                void drawChengYu(bool showImage)
                {
                        addPower(new Thickness(50 * factorX - 20, 25 * factorY, 0, 0), 450 * factorY);
                        addLine(new Point(50 * factorX, 475 * factorY), new Point(300 * factorX, 475 * factorY), 1,0);

                        addBreaker(450 * factorY, new Thickness(300 * factorX - 100, 25 * factorY, 0, 0), 1,showImage, false,true);
                        Line line = addLine(new Point(290 * factorX, 25 * factorY), new Point(910 * factorX, 25 * factorY), 3,1);

                        addBreaker(450 * factorY, new Thickness(600 * factorX - 100, 25 * factorY, 0, 0), 2,showImage, true,true);
                        addBreaker(450 * factorY, new Thickness(900 * factorX - 100, 25 * factorY, 0, 0), 3,showImage, true,true);
                }

                void drawLaGanXiang(bool showImage)
                {
                        addPower(new Thickness(50 * factorX - 20, 25 * factorY, 0, 0), 450 * factorY);
                        addLine(new Point(50 * factorX, 475 * factorY), new Point(250 * factorX, 475 * factorY), 1,0);

                        addBreaker(450 * factorY, new Thickness(250 * factorX - 100, 25 * factorY, 0, 0), 1,showImage, false,true);
                        Line line = addLine(new Point(240 * factorX, 25 * factorY), new Point(885 * factorX, 25 * factorY), 3,1);
                        //line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(new List<string> { "[0].State" }));

                        addBreaker(450 * factorY, new Thickness(500 * factorX - 100, 25 * factorY, 0, 0), 2,showImage, true,false);
                        double y = 475 * factorY - (450 * factorY - 120) / 4 - 110;
                        addMessure(new Thickness(500 * factorX - 75, y, 0, 0), 3,showImage);

                        addBreaker(180 * factorY, new Thickness(800 * factorX - 100, 25 * factorY, 0, 0), 4,showImage, false,false);
                        line = addLine(new Point(725 * factorX, 205 * factorY), new Point(875 * factorX, 205 * factorY),1,4);
                        //line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(new List<string> { "[0].State", "[4].State" }));
                        addBreaker(270 * factorY, new Thickness(725 * factorX - 100, 205 * factorY, 0, 0), 5,showImage, true, true);
                        addBreaker(270 * factorY, new Thickness(875 * factorX - 100, 205 * factorY, 0, 0), 6,showImage, true, true);
                }

                Breaker addBreaker(double height, Thickness margin, byte address,bool showImage, bool hasTriangle, bool hasCurrent, int angle = 0)
                {
                        Breaker breaker = new Breaker();
                        Device device = devices.Find(d => d.Address == address);
                        breaker.txt_name.Content = device.Name;
                        //breaker.txt_name.MouseLeftButtonDown += new MouseButtonEventHandler((o, s) =>
                        //{
                        //        EnterDevice(this, new EnterDeviceArgs(device));
                        //});
                        breaker.breakerMenu.EnterDevice += (o, s) =>
                        {
                                EnterDevice(this, new EnterDeviceArgs(device));
                        };
                        breaker.Width = 200;
                        breaker.Height = height;
                        breaker.Margin = margin;
                        breaker.VerticalAlignment = VerticalAlignment.Top;
                        breaker.HorizontalAlignment = HorizontalAlignment.Left;
                        breaker.Triangle.Visibility = hasTriangle ? Visibility.Visible : Visibility.Hidden;
                        breaker.grid_current.Visibility = hasCurrent ? Visibility.Visible : Visibility.Hidden;
                        RotateTransform rotate = new RotateTransform(angle);
                        rotate.CenterX = breaker.Width / 2;
                        rotate.CenterY = breaker.Height / 2;
                        breaker.RenderTransform = rotate;
                        breaker.InitBreaker(device,showImage);
                        Grid.SetZIndex(breaker, 100);
                        DgmGrid.Children.Add(breaker);
                        return breaker;
                }

                Meter1P addMessure(Thickness margin, byte address,bool showImage)
                {
                        Device device = devices.Find(d => d.Address == address);
                        Meter1P ms = new Meter1P();
                        ms.InitMeter1P(device,showImage);
                        ms.Width = 240;
                        ms.Height = 220;
                        ms.HorizontalAlignment = HorizontalAlignment.Left;
                        ms.VerticalAlignment = VerticalAlignment.Top;
                        ms.Margin = margin;
                        DgmGrid.Children.Add(ms);
                        return ms;
                }

                Meter3P addMeter(double height,Thickness margin,byte address)
                {
                        Device device = devices.Find(d => d.Address == address);
                        Meter3P meter = new Meter3P();
                        meter.InitMeter3P(device);
                        meter.HorizontalAlignment = HorizontalAlignment.Left;
                        meter.VerticalAlignment = VerticalAlignment.Top;
                        meter.Margin = margin;
                        meter.Width = 150;
                        meter.Height = height;
                        DgmGrid.Children.Add(meter);
                        return meter;
                }

                MIC addMIC(double height, Thickness margin,byte address,bool showImage)
                {
                        MIC mic = new MIC();
                        Device device = devices.Find(d => d.Address == address);
                        mic.breakerMenu.EnterDevice += (o, s) =>{};
                        mic.InitMIC(device,showImage);
                        mic.Height = height;
                        mic.Width = 200;
                        mic.HorizontalAlignment = HorizontalAlignment.Left;
                        mic.VerticalAlignment = VerticalAlignment.Top;
                        mic.Margin = margin;
                        DgmGrid.Children.Add(mic);
                        return mic;
                }

                ATS addATS(Thickness margin,byte address,bool showImage)
                {
                        ATS ats = new ATS();
                        Device device = devices.Find(d => d.Address == address);
                        ats.InitATS(device,showImage);
                        ats.Height = 100;
                        ats.Width = 100;
                        ats.HorizontalAlignment = HorizontalAlignment.Left;
                        ats.VerticalAlignment = VerticalAlignment.Top;
                        ats.Margin = margin;
                        DgmGrid.Children.Add(ats);
                        return ats;
                }

                Line addLine(Point p1, Point p2, double thick, int address)
                {
                        Line line = new Line();
                        line.X1 = p1.X;
                        line.Y1 = p1.Y;
                        line.X2 = p2.X;
                        line.Y2 = p2.Y;
                        line.StrokeThickness = thick;
                        line.Stroke = Brushes.Red;
                        if (address != 0)
                        {
                                Device device = devices.Find(d => d.Address == address);
                                line.DataContext = device.Dependence;
                                List<string> sources = Tool.GetDeviceDependence(device);
                                line.SetBinding(Line.StrokeProperty, Tool.addMulBinding(sources));
                        }
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

                Power_Mains addPower(Thickness margin, double height,double angle=0)
                {
                        Power_Mains power = new Power_Mains();
                        power.Height = height;
                        power.Width = 40;
                        power.HorizontalAlignment = HorizontalAlignment.Left;
                        power.VerticalAlignment = VerticalAlignment.Top;
                        power.Margin = margin;
                        RotateTransform rotate = new RotateTransform(angle);
                        rotate.CenterX = power.Width / 2;
                        rotate.CenterY = power.Height / 2;
                        power.RenderTransform = rotate;
                        DgmGrid.Children.Add(power);
                        return power;
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
                        circle.Fill = Brushes.White;
                        DgmGrid.Children.Add(circle);
                        return circle;
                }

                Label addText(Thickness thickness,double width,double height,string text)
                {
                        Label tb = new Label();
                        tb.Width = width;
                        tb.Height = height;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.Margin = thickness;
                        tb.Content = text;
                        tb.FontSize = 16;
                        tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                        tb.VerticalContentAlignment = VerticalAlignment.Center;
                        tb.FontWeight = FontWeights.Bold;
                        tb.Foreground = Brushes.Black;
                        DgmGrid.Children.Add(tb);
                        return tb;
                }

                Slider addSlider(byte address)
                {
                        Slider slider = new Slider();
                        slider.Orientation = Orientation.Vertical;
                        slider.VerticalAlignment = VerticalAlignment.Bottom;
                        slider.HorizontalAlignment = HorizontalAlignment.Right;
                        slider.Margin = new Thickness(0, 0, 5 * factorX, 25 * factorY);
                        slider.Height = 200 * factorY;
                        slider.Maximum = 1;
                        slider.Minimum = 0.1;
                        slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both;
                        slider.TickFrequency = 0.1;
                        slider.LargeChange = 0.1;
                        slider.IsSnapToTickEnabled = true;
                        slider.Value = devices.Find(d => d.Address == address).ISlider;
                        slider.ValueChanged += (s, o) =>
                        {
                                devices.Find(d => d.Address == address).ISlider = o.NewValue;
                        };
                        DgmGrid.Children.Add(slider);
                        return slider;
                }

                ComboBox addSwitch(bool showImage)
                {
                        ComboBox box = new ComboBox();
                        box.HorizontalAlignment = HorizontalAlignment.Right;
                        box.VerticalAlignment = VerticalAlignment.Top;
                        box.Items.Add("实物图");
                        box.Items.Add("电气图");
                        box.SelectedIndex = showImage? 0:1;
                        box.SelectionChanged += box_SelectionChanged;
                        box.Height = 20;
                        box.Width = 100;
                        box.Background = Brushes.Transparent;
                        DgmGrid.Children.Add(box);
                        return box;
                }

                void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                        ComboBox box = sender as ComboBox;
                        DgmGrid.Children.Clear();
                        if (box.SelectedIndex == 0)
                        {
                                draw(true);
                        }
                        else
                        {
                                draw(false);
                        }
                }
        }
}