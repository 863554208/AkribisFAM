using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace AkribisFAM.ViewModel
{
    public class ParameterConfigViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ParameterConfigViewModel()
        {
            RectangleFill = new SolidColorBrush(Colors.Transparent);
            RectangleFillLaser = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateRectangleColorToGreen()
        {
            RectangleFill = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateLaserRectangleColorToGreen()
        {
            RectangleFillLaser = new SolidColorBrush(Colors.LightGreen);
        }

        private Brush _rectangleFill;

        public Brush RectangleFill
        {
            get => _rectangleFill;
            set
            {
                if (_rectangleFill != value)
                {
                    _rectangleFill = value;
                    OnPropertyChanged(nameof(RectangleFill));
                }
            }
        }


        private Brush _rectangleFillLaser;

        public Brush RectangleFillLaser
        {
            get => _rectangleFillLaser;
            set
            {
                if (_rectangleFillLaser != value)
                {
                    _rectangleFillLaser = value;
                    OnPropertyChanged(nameof(RectangleFillLaser));
                }
            }
        }

        private Thickness _rectangleMargin;

        public Thickness RectangleMargin
        {
            get => _rectangleMargin;
            set
            {
                if (_rectangleMargin != value)
                {
                    _rectangleMargin = value;
                    OnPropertyChanged(nameof(RectangleMargin));
                }
            }
        }

        public void UpdateRectanglePosition(double left, double top)
        {
            RectangleMargin = new Thickness(left, 0, 0, top);
        }


    }
}
