using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AkribisFAM.Manager;
using AkribisFAM.Util;

namespace AkribisFAM.ViewModel
{
    public class LaserStationVM : ViewModelBase
    {
        private AllProductTracker _productTracker;

        public AllProductTracker ProductTracker
        {
            get { return _productTracker; }
            set { _productTracker = value; OnPropertyChanged(); }
        }

        public LaserStationVM() 
        {
            ProductTracker = App.productTracker;
        }


    }
}
