using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace AkribisFAM.Models
{

    public class AKBint
    {
        public string PropertyName { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (Min <= value && value <= Max)
                {
                    _value = value;
                }
                else
                {

                    if (value > Max)
                        _value = Max;
                    if (value < Min)
                        _value = Min;
                   // AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid value [ {value} ] is set. Valid range is from [ {Min} ] to [ {Max} ].", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon: AKBMessageBox.MessageBoxIcon.Warning);
                }
            }
        }
        public AKBint(int defaultVal = 0 ,int min = 0, int max = 0 , [CallerMemberName] string prop = null)
        {

            Min = min;
            Max = max;
            PropertyName = prop;

            if (!(Min <= defaultVal && defaultVal <= Max))
            {

                //Task.Run(() => AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid value [ {defaultVal} ] is set. Valid range is from [ {Min} ] to [ {Max} ].", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon: AKBMessageBox.MessageBoxIcon.Warning));
            }

            Value = defaultVal;
        }

    }

    public class AKBenum
    {
        public string PropertyName { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (Min <= value && value <= Max)
                {
                    _value = value;
                }
                else
                {
                    if (value > Max)
                        _value = Max;
                    if (value < Min)
                        _value = Min;
                   // AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid value [ {value} ] is set. Valid range is from [ {Min} ] to [ {Max} ].", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon: AKBMessageBox.MessageBoxIcon.Warning);
                }

            }
        }
        public AKBenum(Type enumType, int defaultVal, [CallerMemberName] string prop = null)
        {

            Min = Enum.GetValues(enumType).Cast<int>().Min();
            Max = Enum.GetValues(enumType).Cast<int>().Max();
            PropertyName = prop;

            if (!(Min <= defaultVal && defaultVal <= Max)) 
            {
                //Task.Run(() => AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid value [ {defaultVal} ] is set. Valid range is from [ {Min} ] to [ {Max} ].", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon : AKBMessageBox.MessageBoxIcon.Warning));
            }

            Value = defaultVal;
        }
    }

    public class AKBstring
    {
        public string PropertyName { get; set; }

        AKBIPValidator _validator;
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (_validator != null)
                    if (!_validator.IsValid(value))
                    {

                       // AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid format value [ {value} ] is set.", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon: AKBMessageBox.MessageBoxIcon.Warning);
                        return;
                    }
                
               _value = value;
            }
        }

        public AKBstring(string defaultVal="", AKBIPValidator validator = null ,[CallerMemberName] string prop = null)
        {
            _validator = validator;
            Value = defaultVal;
            PropertyName = prop;
        }
    }

    public class AKBbool 
    {
        public string PropertyName { get; set; }
        public bool Value { get; set; }
        public AKBbool(bool defaultVal = false, [CallerMemberName] string prop = null)
        {
            Value = defaultVal;
            PropertyName = prop;
        }
    }


    public class AKBdouble 
    {
        public string PropertyName { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                if (Min <= value && value <= Max)
                {
                    _value = value;
                }
                else
                {
                    if (value > Max)
                        _value = Max;
                    if (value < Min)
                        _value = Min;
                    //AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid value [ {value} ] is set. Valid range is from [ {Min} ] to [ {Max} ].", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon: AKBMessageBox.MessageBoxIcon.Warning);
                }

            }
        }

        public AKBdouble(double defaultVal = 0, double min = 0, double max = 0, [CallerMemberName] string prop = null)
        {
            PropertyName = prop;

            Min = min;
            Max = max;

            if (!(Min <= defaultVal && defaultVal <= Max))
            {

                //Task.Run(() => AKBMessageBox.ShowDialog($"Parameter [ {PropertyName} ] \n\rInvalid value [ {defaultVal} ] is set. Valid range is from [ {Min} ] to [ {Max} ].", "PARAMETER OUT OF RANGE", msgBtn: MessageBoxButton.OK, msgIcon: AKBMessageBox.MessageBoxIcon.Warning));
            }

            Value = defaultVal;
        }
    }


    public class AKBIPValidator : IValidator
    {
        public bool IsValid(string value)
        {
            // Regular expression pattern for IPv4 address
            string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            // Check if the provided IP address matches the pattern
            return Regex.IsMatch(value, pattern);
        }
    }
    public interface IValidator
    {
        bool IsValid(string value);

    }

}
