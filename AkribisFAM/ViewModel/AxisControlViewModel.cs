using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.ViewModel
{
    public class AxisControlViewModel : INotifyPropertyChanged
    {

        private string _axisPosition;
        public string AxisPosition
        {
            get { return _axisPosition; }
            set
            {
                _axisPosition = value;
                OnPropertyChanged(nameof(AxisPosition));
            }
        }

        private static AxisControlViewModel _instance;

        public static AxisControlViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AxisControlViewModel();
                }
                return _instance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
