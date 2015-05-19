using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;

namespace AGV_WPF
{
    /// <summary>
    /// AGV编号转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(String))]
    public class AGVNumConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                return "AGV" + value.ToString();
            }
            else
            {
                return "AGV0";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV无线连接转换器
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(String))]
    public class WlLinkConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return "成功";
            }
            else
            {
                return "失败";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV无线连接字体颜色转换器
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(String))]
    public class WlLinkColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return "Green";
            }
            else
            {
                return "Red";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV管制状态转换器
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(String))]
    public class TrafficStatusConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (Boolean)value)
            {
                return "管制中";
            }
            else
            {
                return "非管制";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV管制状态字体颜色转换器
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(String))]
    public class TrafficStatusColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (Boolean)value)
            {
                return "Yellow";
            }
            else
            {
                return "Green";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV速度转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(String))]
    public class AGVSpeedConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                int index = Convert.ToInt32(value);
                if (index >= 0 && index < MainWindow.SpeedOpt.Count)
                {
                    return MainWindow.SpeedOpt[index].SpeedGrade;
                }
            }
            return "未定义";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV速度字体颜色转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(SolidColorBrush))]
    public class AGVSpeedColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                int index = Convert.ToInt32(value);
                if (index >= 0 && index < MainWindow.SpeedOpt.Count)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, Convert.ToByte(255 - 255 / (MainWindow.SpeedOpt.Count - 1) * index), 0));
                }
            }
            return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// AGV地标功能转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(String))]
    public class MarkFunctionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                int markfunindex = Convert.ToInt32(value);
                if (markfunindex >= 0 && markfunindex < MainWindow.MarkFuncOpt.Length)
                {
                    return MainWindow.MarkFuncOpt[markfunindex];
                }
            }
            return "未定义";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV状态转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(String))]
    public class AGVStatusConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                int statusindex = Convert.ToInt32(value);
                if (statusindex == 0x40)
                {
                    return "运行";
                }
                else if (statusindex == 0x41)
                {
                    return "暂停";
                }
                else if (statusindex == 0x42)
                {
                    return "结束地标停止";
                }
                else if (statusindex >= 0 && statusindex < MainWindow.StatusOpt.Length)
                {
                    return MainWindow.StatusOpt[statusindex];
                }
            }
            return "未定义";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV状态字体颜色转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(SolidColorBrush))]
    public class AGVStatusColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                int statusindex = Convert.ToInt32(value);
                if (statusindex == 0x40)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else if (statusindex == 0x41)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else if (statusindex == 0x42)
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else if (statusindex >= 0 && statusindex < MainWindow.StatusOpt.Length)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 125, Convert.ToByte(255 - 255 / (MainWindow.StatusOpt.Length - 1) * statusindex), 0));
                }
            }
            return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AGV电量字体颜色转换器
    /// </summary>
    [ValueConversion(typeof(Byte), typeof(SolidColorBrush))]
    public class AGVPowerColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Byte)
            {
                byte power = (byte)value;
                if (power < 20)
                {
                    return "Red";
                }
                else if (power < 60)
                {
                    return "Orange";
                }
                else
                {
                    return "Green";
                }
            }
            else
            { return "Black"; }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
