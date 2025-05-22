using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAMotion;

namespace AkribisFAM.AAmotionFAM
{
    public class AGM800
    {
        private static AGM800 _current;
        public static AGM800 Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AGM800();
                }
                return _current;
            }
        }

        public readonly Dictionary<string, AxisRef> axisRefs = new Dictionary<string, AxisRef>
        {
            { "A", AxisRef.A },
            { "B", AxisRef.B },
            { "C", AxisRef.C },
            { "D", AxisRef.D },
            { "E", AxisRef.E },
            { "F", AxisRef.F },
            { "G", AxisRef.G },
            { "H", AxisRef.H },
            { "I", AxisRef.I },
            { "J", AxisRef.J },
            { "K", AxisRef.K },
            { "L", AxisRef.L }
        };


        public bool connectStatus = false;

        public MotionController[] controller = new MotionController[4];

    }
}
