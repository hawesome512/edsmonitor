using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Monitor
{

        class StateToRectFillConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        Brush brush = Brushes.LightGray;
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
                        Brush brush = Brushes.SeaGreen;
                        switch (state.SwitchState)
                        {
                                case SwitchStatus.Close:
                                case SwitchStatus.ATS_N:
                                case SwitchStatus.ATS_R:
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

        class StrokeToOpacityConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        Brush brush = value as Brush;
                        if (brush == Brushes.SeaGreen || brush == Brushes.Gray)
                        {
                                return 0;
                        }
                        else
                        {
                                return 1;
                        }
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class MulStatesToLineStrokeConverter : IMultiValueConverter
        {
                public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        if (values[0] is DState)
                        {
                                DState state0 = (DState)values[0];
                                switch (state0.RunState)
                                {
                                        case Run.Alarm:
                                                return Brushes.Orange;
                                        case Run.NonSignal:
                                                return Brushes.Gray;
                                        default:
                                                foreach (var value in values)
                                                {
                                                        DState state = (DState)value;
                                                        if (state.SwitchState == SwitchStatus.Open)
                                                        {
                                                                return Brushes.SeaGreen;
                                                        }
                                                        else if (state.SwitchState == SwitchStatus.ATS_R)
                                                        {
                                                                break;
                                                        }
                                                }
                                                return Brushes.Red;
                                }
                        }
                        return null;
                }

                public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
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
                                case SwitchStatus.Open:
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
                                case SwitchStatus.Close:
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

        class StateToATSConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        double x2 = 0;
                        switch (state.SwitchState)
                        {
                                case SwitchStatus.ATS_N:
                                        x2=15;
                                        break;
                                case SwitchStatus.ATS_R:
                                        x2 = 45;
                                        break;
                                default:
                                        x2 = 30;
                                        break;
                        }
                        return x2;
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
                                case SwitchStatus.Close:
                                        return "合闸";
                                case SwitchStatus.Open:
                                        return "分闸";
                                case SwitchStatus.ATS_N:
                                        return "投常";
                                case SwitchStatus.ATS_R:
                                        return "投备";
                                case SwitchStatus.Run:
                                        return "运行";
                                case SwitchStatus.Wait:
                                        return "等待";
                                case SwitchStatus.Ready:
                                        return "就绪";
                                default:
                                        return "未知";
                        }
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StringToVisibility : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        string str = value == null ? null : value.ToString();
                        System.Windows.Visibility vsb = string.IsNullOrEmpty(str) ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
                        return vsb;
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
                                        return "本地";
                                case ControlMode.Remote:
                                        return "远程";
                                default:
                                        return "未知";
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

        class MultiToBtnEnableConvertoer : IMultiValueConverter
        {
                public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)values[0];
                        User user = (User)values[1];
                        if (state.ControlState == ControlMode.Remote && user == User.ADMIN)
                        {
                                return true;
                        }
                        else
                        {
                                return false;
                        }
                }

                public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
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
                                case SwitchStatus.Close:
                                        packUri = "pack://application:,,,/EDS;component/Images/close.png";
                                        break;
                                case SwitchStatus.Open:
                                        packUri = "pack://application:,,,/EDS;component/Images/open.png";
                                        break;
                                default:
                                        packUri = "pack://application:,,,/EDS;component/Images/unknown.png";
                                        break;
                        }
                        return (new ImageSourceConverter().ConvertFromString(packUri) as ImageSource);
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToATSImageSourceConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        string packUri;
                        switch (state.SwitchState)
                        {
                                case SwitchStatus.ATS_N:
                                        packUri = "pack://application:,,,/EDS;component/Images/ATS/ATS_N.png";
                                        break;
                                case SwitchStatus.ATS_R:
                                        packUri = "pack://application:,,,/EDS;component/Images/ATS/ATS_R.png";
                                        break;
                                case SwitchStatus.Open:
                                        packUri = "pack://application:,,,/EDS;component/Images/ATS/ATS_OPEN.png";
                                        break;
                                default:
                                        packUri = "pack://application:,,,/EDS;component/Images/ATS/ATS_UK.png";
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
                                case SwitchStatus.Close:
                                        isChecked = false;
                                        break;
                                case SwitchStatus.Open:
                                        isChecked = true;
                                        break;
                                default:
                                        isChecked = null;
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

        class UserToBoolConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        User user = (User)value;
                        return user == User.ADMIN ? true : false;
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class UserToAccountText : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        User user = (User)value;
                        return user == User.ADMIN ? "退出" : "登录";
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class DoubleToInt : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        double dValue = (double)value;
                        return Math.Round(dValue, 0);
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }

        class StateToBreakerImageSource : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        DState state = (DState)value;
                        string switchState = state.SwitchState == SwitchStatus.Close ? "_ON" : string.Empty;
                        string packUri=string.Format("pack://application:,,,/EDS;component/Images/Types/{0}{1}.png", parameter.ToString(),switchState);
                        return (new ImageSourceConverter().ConvertFromString(packUri) as ImageSource);
                }

                public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }
}
