using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;

namespace AkribisFAM.ViewModel
{
    public class ColorChangeViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Brush _color1;

        public Brush Color1
        {
            get => _color1;
            set
            {
                if (_color1 != value)
                {
                    _color1 = value;
                    OnPropertyChanged(nameof(Color1));
                }
            }
        }

        private Brush _color2;

        public Brush Color2
        {
            get => _color2;
            set
            {
                if (_color2 != value)
                {
                    _color2 = value;
                    OnPropertyChanged(nameof(Color2));
                }
            }
        }

        private Brush _color3;

        public Brush Color3
        {
            get => _color3;
            set
            {
                if (_color3 != value)
                {
                    _color3 = value;
                    OnPropertyChanged(nameof(Color3));
                }
            }
        }

        private Brush _color4;

        public Brush Color4
        {
            get => _color4;
            set
            {
                if (_color4 != value)
                {
                    _color4 = value;
                    OnPropertyChanged(nameof(Color4));
                }
            }
        }

        private Brush _color_lailiao_1;

        public Brush Color_lailiao_1
        {
            get => _color_lailiao_1;
            set
            {
                if (_color_lailiao_1 != value)
                {
                    _color_lailiao_1 = value;
                    OnPropertyChanged(nameof(Color_lailiao_1));
                }
            }
        }

        private Brush _color_lailiao_2;

        public Brush Color_lailiao_2
        {
            get => _color_lailiao_2;
            set
            {
                if (_color_lailiao_2 != value)
                {
                    _color_lailiao_2 = value;
                    OnPropertyChanged(nameof(Color_lailiao_2));
                }
            }
        }

        private Brush _color_lailiao_3;

        public Brush Color_lailiao_3
        {
            get => _color_lailiao_3;
            set
            {
                if (_color_lailiao_3 != value)
                {
                    _color_lailiao_3 = value;
                    OnPropertyChanged(nameof(Color_lailiao_3));
                }
            }
        }

        private Brush _color_fujian_1;

        public Brush Color_FuJian_1
        {
            get => _color_fujian_1;
            set
            {
                if (_color_fujian_1 != value)
                {
                    _color_fujian_1 = value;
                    OnPropertyChanged(nameof(Color_FuJian_1));
                }
            }
        }

        private Brush _color_fujian_2;

        public Brush Color_FuJian_2
        {
            get => _color_fujian_2;
            set
            {
                if (_color_fujian_2 != value)
                {
                    _color_fujian_2 = value;
                    OnPropertyChanged(nameof(Color_FuJian_2));
                }
            }
        }

        private Brush _color_fujian_3;

        public Brush Color_FuJian_3
        {
            get => _color_fujian_3;
            set
            {
                if (_color_fujian_3 != value)
                {
                    _color_fujian_3 = value;
                    OnPropertyChanged(nameof(Color_FuJian_3));
                }
            }
        }


        public ColorChangeViewModel()
        {
            Color1 = new SolidColorBrush(Colors.Transparent);
            Color2 = new SolidColorBrush(Colors.Transparent);
            Color3 = new SolidColorBrush(Colors.Transparent);
            Color4 = new SolidColorBrush(Colors.Transparent);
            Color_lailiao_1 = new SolidColorBrush(Colors.Transparent);
            Color_lailiao_2 = new SolidColorBrush(Colors.Transparent);
            Color_lailiao_3 = new SolidColorBrush(Colors.Transparent);
            Color_FuJian_1 = new SolidColorBrush(Colors.Transparent);
            Color_FuJian_2 = new SolidColorBrush(Colors.Transparent);
            Color_FuJian_3 = new SolidColorBrush(Colors.Transparent);

        }
        #region 组装部分颜色变换
        public void UpdateTest1ColorToGreen()
        {
            Color1 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateTest1ColorToTransparent()
        {
            Color1 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateTest2ColorToGreen()
        {
            Color2 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateTest2ColorToTransparent()
        {
            Color2 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateTest3ColorToGreen()
        {
            Color3 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateTest3ColorToTransparent()
        {
            Color3 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateTest4ColorToGreen()
        {
            Color4 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateTest4ColorToTransparent()
        {
            Color4 = new SolidColorBrush(Colors.Transparent);
        }
        #endregion

        #region 来料部分颜色变化
        public void UpdateLailiao1ColorToGreen()
        {
            Color_lailiao_1 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateLailiao1ColorToTransparent()
        {
            Color_lailiao_1 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateLailiao2ColorToGreen()
        {
            Color_lailiao_2 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateLailiao2ColorToTransparent()
        {
            Color_lailiao_2 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateLailiao3ColorToGreen()
        {
            Color_lailiao_3 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateLailiao3ColorToTransparent()
        {
            Color_lailiao_3 = new SolidColorBrush(Colors.Transparent);
        }
        #endregion

        #region 复检部分颜色变化
        public void UpdateFuJian_1ColorToGreen()
        {
            Color_FuJian_1 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateFuJian_1ColorToTransparent()
        {
            Color_FuJian_1 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateFuJian_2ColorToGreen()
        {
            Color_FuJian_2 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateFuJian_2ColorToTransparent()
        {
            Color_FuJian_2 = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateFuJian_3ColorToGreen()
        {
            Color_FuJian_3 = new SolidColorBrush(Colors.LightGreen);
        }

        public void UpdateFuJian_3ColorToTransparent()
        {
            Color_FuJian_3 = new SolidColorBrush(Colors.Transparent);
        }
        #endregion
    }
}
