using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAMotion;
using LiveCharts;
using LiveCharts.Wpf;

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

        private static AxisControlViewModel _current;

        public static AxisControlViewModel Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AxisControlViewModel();
                }
                return _current;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateAxisPostion()
        {
            var temp = GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).Pos.ToString();
            Debug.WriteLine("temp:" + temp);
            if (AxisPosition != temp) // 避免不必要的更新
            {
                AxisPosition = temp;
            }
        }
    }
}
