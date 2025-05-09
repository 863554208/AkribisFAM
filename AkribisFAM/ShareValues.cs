using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM
{
    public static class ShareValues
    {
        const int AxisNum = 50;
        public static double[] AxisSpeedMin = new double[AxisNum];
        public static double[] AxisSpeedMax = new double[AxisNum];
        public static double[] AxisAccMin = new double[AxisNum];
        public static double[] AxisAccMax = new double[AxisNum];
        public static double[] AxisDecMin = new double[AxisNum];
        public static double[] AxisDecMax = new double[AxisNum];
        public static double[] AxisMin = new double[AxisNum];
        public static double[] AxisMax = new double[AxisNum];

        public static int MAXPOINTSNUM = 4;
        public static int MAXPALLETNUM = 6;
        public static int MAXMODULENUMX = 5;
        public static int MAXMODULENUMY = 5;


    }

    public static class IOState
    {
        public static bool INPUT_IO_1 = false;
        public static bool INPUT_IO_2 = false;
    }
}
