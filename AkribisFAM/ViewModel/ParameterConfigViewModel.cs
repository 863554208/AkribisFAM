using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AkribisFAM.Manager;


namespace AkribisFAM.ViewModel
{

    public class ErrorIconViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _ErrorNum;
        public int promptCount
        {
            get => _ErrorNum;
            set
            {
                if (_ErrorNum != value)
                {
                    _ErrorNum = value;
                    OnPropertyChanged("promptCount");
                }
            }
        }

        public ErrorIconViewModel()
        {
            promptCount = 0;
        }

        public void UpdateIcon()
        {
            promptCount = ErrorManager.Current.ErrorCnt;
        }


    }
}
