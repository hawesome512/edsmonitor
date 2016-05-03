using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Monitor
{
        /// <summary>
        /// Dial.xaml 的交互逻辑
        /// </summary>
        public partial class Dial : UserControl
        {
                public Dial()
                {
                        this.InitializeComponent();
                }

                public void setSize(double len)
                {
                        this.Width = this.Height = len;
                        myGauge2.Radius =100/200f*len;
                        myGauge2.ScaleRadius = 75/200f * len;
                        myGauge2.PointerLength = 70/200f * len;
                        myGauge2.PointerCapRadius = 15/200f * len;
                        myGauge2.RangeIndicatorRadius = 65/200f * len;
                        myGauge2.ScaleLabelRadius = 85/200f * len;
                        myGauge2.DialTextOffset = 60 / 200f * len;
                }
        }
}