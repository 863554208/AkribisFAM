using AkribisFAM.CommunicationProtocol;
using MaterialDesignThemes.Wpf;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace AkribisFAM.Windows.Converters
{

    //public class EnumToBrushConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (ModbusTCPWorker.GetInstance() == null) return Brushes.Red;

    //        if (value is IO_INFunction_Table enumVal)
    //        {
    //            int index = (int)enumVal;
    //            bool stat = false;
    //            ModbusTCPWorker.GetInstance().Read_Coil(index, ref stat);

    //            return stat ? Brushes.LimeGreen : Brushes.Red;

    //        }

    //        return Brushes.Red; // default fallback
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class EnumToBrushConverter : IValueConverter
    {
        // Convert integer to SolidColorBrush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IO_INFunction_Table enumVal)
            {
                if (ModbusTCPWorker.GetInstance() == null) return new SolidColorBrush(Colors.Red);
                { 
                    int index = (int)enumVal;
                    bool stat = false;
                    ModbusTCPWorker.GetInstance().Read_Coil(index, ref stat);

                    return stat ? new SolidColorBrush(Colors.LimeGreen) :new SolidColorBrush(Colors.Red); ;

                }
            }
            return new SolidColorBrush(Colors.Red);  // Return transparent brush if value is not an integer
        }

        // This method is not used in one-way bindings but must be implemented
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        // Converts a boolean to Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Hidden;
            }
            throw new ArgumentException("Invalid argument type", nameof(value));
        }

        // Converts back from Visibility to boolean
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue)
            {
                return visibilityValue == Visibility.Visible;
            }
            throw new ArgumentException("Invalid argument type", nameof(value));
        }
    }
    public class BooleanToConnectionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? "Connected" : "Disconnected";
            }
            throw new ArgumentException("Expected boolean value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TextLengthToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                // Convert text length to boolean indicating whether the control should be enabled
                return !string.IsNullOrEmpty(text);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the value is null, return false; otherwise, return true
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter doesn't support converting back.
            throw new NotSupportedException();
        }
    }
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue.PadLeft(10, '0');
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }
    }


    public class GrayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.LightGreen;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BooleanToOnOffStringConverter : IValueConverter
    {
        // Converts a boolean value to a string ("ON" for true, "OFF" for false)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "ON" : "OFF";
            }

            throw new InvalidOperationException("The value must be a boolean.");
        }

        // ConvertBack is not needed for this scenario, but it must be implemented
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Green;
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

 
    //public class IntToColorConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is int intValue)
    //        {
    //            switch (intValue)
    //            {
    //                case 0:
    //                    return Brushes.LimeGreen;
    //                case 1:
    //                    return Brushes.OrangeRed;
    //                case 2:
    //                    return Brushes.Red;
    //                default:
    //                    return Brushes.LimeGreen; // Default color if integer value doesn't match
    //            }
    //        }
    //        return Brushes.LimeGreen;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class IntToColorBrushConverter : IValueConverter
    {
        // Convert integer to SolidColorBrush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int colorIndex)
            {
                // Map integer values to specific colors
                switch (colorIndex)
                {
                    case 0:
                        return new SolidColorBrush(Colors.LimeGreen);
                    case 1:
                        return new SolidColorBrush(Colors.Red);
                    case 2:
                        return new SolidColorBrush(Colors.OrangeRed);
                    default:
                        return Brushes.Transparent; // Default brush if integer is out of range
                }
            }
            return Brushes.Transparent; // Return transparent brush if value is not an integer
        }

        // This method is not used in one-way bindings but must be implemented
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class LockColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Gray;
            }
            return Brushes.Orange;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                // Replace "Icon1" and "Icon2" with the actual Material Design icons you want to use
                return booleanValue ? PackIconKind.Check : PackIconKind.CloseThick;
            }
            return PackIconKind.Help; // Default icon if the value is not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 }
