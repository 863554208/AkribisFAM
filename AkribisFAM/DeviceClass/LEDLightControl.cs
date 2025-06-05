using AkribisFAM.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.DeviceClass
{
    public class LEDLightControl
    {
        public enum ColorCode
        {
            Default,
            Green,
            Orange,
            Red,
            Yellow,
        }
        public bool SystemLightON()
        {
            return IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_3light1, 1) &
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_4light2, 1);
        }
        public bool SystemLightOFF()
        {
            return IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_3light1, 0) &
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_4light2, 0);
        }
        public bool ChangeColor(ColorCode color)
        {
            bool red = false;
            bool yellow = false;
            bool green = false;
            switch (color)
            {
                case ColorCode.Default:
                    red = false;
                    yellow = false;
                    green = false;
                    break;
                case ColorCode.Green:
                    red = false;
                    yellow = false;
                    green = true;
                    break;
                case ColorCode.Orange:
                    red = false;
                    yellow = true;
                    green = true;
                    break;
                case ColorCode.Red:
                    red = true;
                    yellow = false;
                    green = false;
                    break;
                case ColorCode.Yellow:
                    red = false;
                    yellow = true;
                    green = false;
                    break;
                default:
                    break;
            }

            return IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_0Tri_color_light_red, red ? 1 : 0) &
             IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, yellow ? 1 : 0) &
             IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, green ? 1 : 0);
        }
    }
}
