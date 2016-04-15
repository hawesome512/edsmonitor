using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Monitor
{
        class StateToRectFillConverter:IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        Brush brush=Brushes.LightGray;
                        switch (state.RunState)
                        {
                                case Run.Alarm:
                                        brush = Brushes.Red;
                                        break;
                                case Run.Normal:
                                        brush = Brushes.SeaGreen;
                                        break;
                                default:
                                        brush = Brushes.LightGray;
                                        break;
                        }
                        return brush;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToLineStrokeConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        Brush brush = Brushes.Gray;
                        switch (state.SwitchState)
                        {
                                case Switch.Close:
                                        brush = Brushes.Red;
                                        break;
                                default:
                                        break;
                        }
                        return brush;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToOpenConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        double opacity = 0;
                        switch (state.SwitchState)
                        {
                                case Switch.Open:
                                        opacity = 1;
                                        break;
                                default:
                                        break;
                        }
                        return opacity;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToCloseConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        double opacity = 0;
                        switch (state.SwitchState)
                        {
                                case Switch.Close:
                                        opacity = 1;
                                        break;
                                default:
                                        break;
                        }
                        return opacity;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToStringConverter : IValueConverter
        {

                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        switch (state.SwitchState)
                        {
                                case Switch.Close:
                                        return "Close";
                                case Switch.Open:
                                        return "Open";
                                default:
                                        return "Unknown";
                        }
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class ControlToStringConverter : IValueConverter
        {

                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        switch (state.ControlState)
                        {
                                case ControlMode.Local:
                                        return "Local";
                                case ControlMode.Remote:
                                        return "Remote";
                                default:
                                        return "Unknown";
                        }
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class ControlToBtnEnableConverter : IValueConverter
        {

                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        switch (state.ControlState)
                        {
                                case ControlMode.Remote:
                                        return true;
                                default:
                                        return false;
                        }
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToImageSourceConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        string packUri;
                        switch (state.SwitchState)
                        {
                                case Switch.Close:
                                        packUri = "pack://application:,,,/Monitor;component/Images/close.png";
                                        break;
                                case Switch.Open:
                                        packUri = "pack://application:,,,/Monitor;component/Images/open.png";
                                        break;
                                default:
                                        packUri = "pack://application:,,,/Monitor;component/Images/unknown.png";
                                        break;
                        }
                        return (new ImageSourceConverter().ConvertFromString(packUri) as ImageSource);
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToBoolConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        bool? isChecked;
                        switch (state.SwitchState)
                        {
                                case Switch.Close:
                                        isChecked=false;
                                        break;
                                case Switch.Open:
                                        isChecked=true;
                                        break;
                                default:
                                        isChecked=null;
                                        break;
                        }
                        return isChecked;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        //throw new NotImplementedException();
                        return "";
                }
        }

        class RemoteToBoolConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        string remote = (string)value;
                        bool? isChecked;
                        switch (remote)
                        {
                                case "Remote":
                                        isChecked = true;
                                        break;
                                case "Local":
                                        isChecked = false;
                                        break;
                                default:
                                        isChecked = null;
                                        break;
                        }
                        return isChecked;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class BoolToVisibilityConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        bool b = (bool)value;
                        System.Windows.Visibility vsb = b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                        return vsb;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }
}
