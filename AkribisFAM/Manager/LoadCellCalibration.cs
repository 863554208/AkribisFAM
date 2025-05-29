using AkribisFAM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Manager
{
    public class LoadCellCalibration
    {
        public LoadCellCalibration() 
        {
            for (int i = 0; i < NewtonCurrentList.Length; i++)
            {
                NewtonCurrentList[i] = new List<NewtonCurrent>();
            }
            LoadCellCalibration calib;
            if (!FileHelper.Load(out calib))
            {
                
            }

        }
        public LoadCellModel[] Models = new LoadCellModel[4];
        public class LoadCellModel
        {
            public double m { get; set; }
            public double C { get; set; }
        }
        public List<NewtonCurrent>[] NewtonCurrentList = new List<NewtonCurrent>[4];
        public class NewtonCurrent
        {

            private double current;

            public double Current
            {
                get { return current; }
                set { current = value; }
            }

            private double newton;

            public double Newton
            {
                get { return newton; }
                set { newton = value; }
            }
        }



    }
}
