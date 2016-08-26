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

namespace AutoSizeDemo
{
        public class School
        {
                public Student1 Stu;
        }
        public class Student1
        {
                public string Name1;
        }

        /// <summary>
        /// MainWindow.xaml 的交互逻辑
        /// </summary>
        public partial class MainWindow : Window
        {
                System.Windows.Forms.DataVisualization.Charting.Chart myChart;
                public MainWindow()
                {
                        InitializeComponent();

                        myChart = myHost.Child as System.Windows.Forms.DataVisualization.Charting.Chart;

                        UIChart hChart = new UIChart(myChart);
                }
        }
}
