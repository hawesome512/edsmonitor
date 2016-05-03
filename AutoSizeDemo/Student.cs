using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;

namespace AutoSizeDemo
{
        class Student:INotifyPropertyChanged
        {
                public event PropertyChangedEventHandler PropertyChanged;
                string name="a";
                public string Name
                {
                        get
                        {
                                return name;
                        }
                        set
                        {
                                name = value;
                                if (PropertyChanged != null)
                                {
                                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
                                }
                        }
                }
        }
        class Person : INotifyPropertyChanged
        {
                public event PropertyChangedEventHandler PropertyChanged;
                string name = "b";
                public string Name
                {
                        get
                        {
                                return name;
                        }
                        set
                        {
                                name = value;
                                if (PropertyChanged != null)
                                {
                                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
                                }
                        }
                }
        }

        class MultiBindingConvertor : IMultiValueConverter
        {
                public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        return values[0].ToString() + values[1].ToString();
                }

                public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }
}
