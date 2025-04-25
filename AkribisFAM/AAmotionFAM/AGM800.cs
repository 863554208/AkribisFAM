using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAMotion;

namespace AkribisFAM.AAmotionFAM
{
    public partial class AGM800
    {
        
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
        public bool disconnectStatus = false;

        public MotionController controller = AAMotionAPI.Initialize(ControllerType.AGM800);
    }
}
