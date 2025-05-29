using AkribisFAM.CommunicationProtocol;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

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
}
