using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using LiveCharts;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// ParameterConfig.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterConfig : UserControl
    {
        public ScanningAreaParams scanningareaparams = new ScanningAreaParams();
        public AssemblyAreaParams assemblyareaparams = new AssemblyAreaParams();
        public RecheckAreaParams recheckareaparams = new RecheckAreaParams();
        PalletPointsWindow palletpointswindow;
        JObject LimitJsonObject;
        public Dictionary<string, double> AxisLimitList;
        bool init = false;

        public ParameterConfig()
        {
            InitializeComponent();
            ReadLimitJson();

        }

        private void ReadLimitJson() {

            try {
                string folder = Directory.GetCurrentDirectory(); //获取应用程序的当前工作目录。 
                string path = folder + "\\Limit.json";

                StreamReader file = File.OpenText(path);
                JsonTextReader reader = new JsonTextReader(file);
                LimitJsonObject = (JObject)JToken.ReadFrom(reader);
            }
            catch
            {
                MessageBox.Show("Read Json Failed!");
                return;
            }

            Beltspeed1min.Text = LimitJsonObject["ScanningArea"]["Belt"]["Speed"][0].ToString();
            Beltspeed1max.Text = LimitJsonObject["ScanningArea"]["Belt"]["Speed"][1].ToString();
            Liftspeed1min.Text = LimitJsonObject["ScanningArea"]["Lift"]["Speed"][0].ToString();
            Liftspeed1max.Text = LimitJsonObject["ScanningArea"]["Lift"]["Speed"][1].ToString();
            GantryXspeed1min.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Speed"][0].ToString();
            GantryXspeed1max.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Speed"][1].ToString();
            GantryYspeed1min.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Speed"][0].ToString();
            GantryYspeed1max.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Speed"][1].ToString();
            GantryZspeed1min.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Speed"][0].ToString();
            GantryZspeed1max.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Speed"][1].ToString();
            ShareValues.AxisSpeedMin[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Speed"][1].ToString());

            Beltacc1min.Text = LimitJsonObject["ScanningArea"]["Belt"]["Acc"][0].ToString();
            Beltacc1max.Text = LimitJsonObject["ScanningArea"]["Belt"]["Acc"][1].ToString();
            Liftacc1min.Text = LimitJsonObject["ScanningArea"]["Lift"]["Acc"][0].ToString();
            Liftacc1max.Text = LimitJsonObject["ScanningArea"]["Lift"]["Acc"][1].ToString();
            GantryXacc1min.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Acc"][0].ToString();
            GantryXacc1max.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Acc"][1].ToString();
            GantryYacc1min.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Acc"][0].ToString();
            GantryYacc1max.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Acc"][1].ToString();
            GantryZacc1min.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Acc"][0].ToString();
            GantryZacc1max.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Acc"][1].ToString();
            ShareValues.AxisAccMin[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Acc"][1].ToString());

            Beltdec1min.Text = LimitJsonObject["ScanningArea"]["Belt"]["Dec"][0].ToString();
            Beltdec1max.Text = LimitJsonObject["ScanningArea"]["Belt"]["Dec"][1].ToString();
            Liftdec1min.Text = LimitJsonObject["ScanningArea"]["Lift"]["Dec"][0].ToString();
            Liftdec1max.Text = LimitJsonObject["ScanningArea"]["Lift"]["Dec"][1].ToString();
            GantryXdec1min.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Dec"][0].ToString();
            GantryXdec1max.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Dec"][1].ToString();
            GantryYdec1min.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Dec"][0].ToString();
            GantryYdec1max.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Dec"][1].ToString();
            GantryZdec1min.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Dec"][0].ToString();
            GantryZdec1max.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Dec"][1].ToString();
            ShareValues.AxisDecMin[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Dec"][1].ToString());

            Beltaxis1min.Text = LimitJsonObject["ScanningArea"]["Belt"]["Axis"][0].ToString();
            Beltaxis1max.Text = LimitJsonObject["ScanningArea"]["Belt"]["Axis"][1].ToString();
            Liftaxis1min.Text = LimitJsonObject["ScanningArea"]["Lift"]["Axis"][0].ToString();
            Liftaxis1max.Text = LimitJsonObject["ScanningArea"]["Lift"]["Axis"][1].ToString();
            GantryXaxis1min.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Axis"][0].ToString();
            GantryXaxis1max.Text = LimitJsonObject["ScanningArea"]["GantryX"]["Axis"][1].ToString();
            GantryYaxis1min.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Axis"][0].ToString();
            GantryYaxis1max.Text = LimitJsonObject["ScanningArea"]["GantryY"]["Axis"][1].ToString();
            GantryZaxis1min.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Axis"][0].ToString();
            GantryZaxis1max.Text = LimitJsonObject["ScanningArea"]["GantryZ"]["Axis"][1].ToString();
            ShareValues.AxisMin[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Axis"][0].ToString());
            ShareValues.AxisMax[0] = double.Parse(LimitJsonObject["ScanningArea"]["Belt"]["Axis"][1].ToString());
            ShareValues.AxisMin[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Axis"][0].ToString());
            ShareValues.AxisMax[1] = double.Parse(LimitJsonObject["ScanningArea"]["Lift"]["Axis"][1].ToString());
            ShareValues.AxisMin[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Axis"][0].ToString());
            ShareValues.AxisMax[2] = double.Parse(LimitJsonObject["ScanningArea"]["GantryX"]["Axis"][1].ToString());
            ShareValues.AxisMin[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Axis"][0].ToString());
            ShareValues.AxisMax[3] = double.Parse(LimitJsonObject["ScanningArea"]["GantryY"]["Axis"][1].ToString());
            ShareValues.AxisMin[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Axis"][0].ToString());
            ShareValues.AxisMax[4] = double.Parse(LimitJsonObject["ScanningArea"]["GantryZ"]["Axis"][1].ToString());

            Beltspeed2min.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Speed"][0].ToString();
            Beltspeed2max.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Speed"][1].ToString();
            Liftspeed2min.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Speed"][0].ToString();
            Liftspeed2max.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Speed"][1].ToString();
            GantryXspeed2min.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Speed"][0].ToString();
            GantryXspeed2max.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Speed"][1].ToString();
            GantryYspeed2min.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Speed"][0].ToString();
            GantryYspeed2max.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Speed"][1].ToString();
            GantryZspeed2min.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Speed"][0].ToString();
            GantryZspeed2max.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Speed"][1].ToString();
            PickerZspeed2min.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Speed"][0].ToString();
            PickerZspeed2max.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Speed"][1].ToString();
            PickerRspeed2min.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Speed"][0].ToString();
            PickerRspeed2max.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Speed"][1].ToString();
            ShareValues.AxisSpeedMin[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Speed"][1].ToString());

            Beltacc2min.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Acc"][0].ToString();
            Beltacc2max.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Acc"][1].ToString();
            Liftacc2min.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Acc"][0].ToString();
            Liftacc2max.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Acc"][1].ToString();
            GantryXacc2min.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Acc"][0].ToString();
            GantryXacc2max.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Acc"][1].ToString();
            GantryYacc2min.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Acc"][0].ToString();
            GantryYacc2max.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Acc"][1].ToString();
            GantryZacc2min.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Acc"][0].ToString();
            GantryZacc2max.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Acc"][1].ToString();
            PickerZacc2min.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Acc"][0].ToString();
            PickerZacc2max.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Acc"][1].ToString();
            PickerRacc2min.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Acc"][0].ToString();
            PickerRacc2max.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Acc"][1].ToString();
            ShareValues.AxisAccMin[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Acc"][1].ToString());

            Beltdec2min.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Dec"][0].ToString();
            Beltdec2max.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Dec"][1].ToString();
            Liftdec2min.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Dec"][0].ToString();
            Liftdec2max.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Dec"][1].ToString();
            GantryXdec2min.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Dec"][0].ToString();
            GantryXdec2max.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Dec"][1].ToString();
            GantryYdec2min.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Dec"][0].ToString();
            GantryYdec2max.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Dec"][1].ToString();
            GantryZdec2min.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Dec"][0].ToString();
            GantryZdec2max.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Dec"][1].ToString();
            PickerZdec2min.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Dec"][0].ToString();
            PickerZdec2max.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Dec"][1].ToString();
            PickerRdec2min.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Dec"][0].ToString();
            PickerRdec2max.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Dec"][1].ToString();
            ShareValues.AxisDecMin[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Dec"][1].ToString());

            Beltaxis2min.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Axis"][0].ToString();
            Beltaxis2max.Text = LimitJsonObject["AssemblyArea"]["Belt"]["Axis"][1].ToString();
            Liftaxis2min.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Axis"][0].ToString();
            Liftaxis2max.Text = LimitJsonObject["AssemblyArea"]["Lift"]["Axis"][1].ToString();
            GantryXaxis2min.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Axis"][0].ToString();
            GantryXaxis2max.Text = LimitJsonObject["AssemblyArea"]["GantryX"]["Axis"][1].ToString();
            GantryYaxis2min.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Axis"][0].ToString();
            GantryYaxis2max.Text = LimitJsonObject["AssemblyArea"]["GantryY"]["Axis"][1].ToString();
            GantryZaxis2min.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Axis"][0].ToString();
            GantryZaxis2max.Text = LimitJsonObject["AssemblyArea"]["GantryZ"]["Axis"][1].ToString();
            PickerZaxis2min.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Axis"][0].ToString();
            PickerZaxis2max.Text = LimitJsonObject["AssemblyArea"]["PickerZ"]["Axis"][1].ToString();
            PickerRaxis2min.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Axis"][0].ToString();
            PickerRaxis2max.Text = LimitJsonObject["AssemblyArea"]["PickerR"]["Axis"][1].ToString();
            ShareValues.AxisMin[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Axis"][0].ToString());
            ShareValues.AxisMax[5] = double.Parse(LimitJsonObject["AssemblyArea"]["Belt"]["Axis"][1].ToString());
            ShareValues.AxisMin[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Axis"][0].ToString());
            ShareValues.AxisMax[6] = double.Parse(LimitJsonObject["AssemblyArea"]["Lift"]["Axis"][1].ToString());
            ShareValues.AxisMin[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Axis"][0].ToString());
            ShareValues.AxisMax[7] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryX"]["Axis"][1].ToString());
            ShareValues.AxisMin[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Axis"][0].ToString());
            ShareValues.AxisMax[8] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryY"]["Axis"][1].ToString());
            ShareValues.AxisMin[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Axis"][0].ToString());
            ShareValues.AxisMax[9] = double.Parse(LimitJsonObject["AssemblyArea"]["GantryZ"]["Axis"][1].ToString());
            ShareValues.AxisMin[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Axis"][0].ToString());
            ShareValues.AxisMax[10] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerZ"]["Axis"][1].ToString());
            ShareValues.AxisMin[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Axis"][0].ToString());
            ShareValues.AxisMax[11] = double.Parse(LimitJsonObject["AssemblyArea"]["PickerR"]["Axis"][1].ToString());

            Beltspeed3min.Text = LimitJsonObject["RecheckArea"]["Belt"]["Speed"][0].ToString();
            Beltspeed3max.Text = LimitJsonObject["RecheckArea"]["Belt"]["Speed"][1].ToString();
            Liftspeed3min.Text = LimitJsonObject["RecheckArea"]["Lift"]["Speed"][0].ToString();
            Liftspeed3max.Text = LimitJsonObject["RecheckArea"]["Lift"]["Speed"][1].ToString();
            GantryXspeed3min.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Speed"][0].ToString();
            GantryXspeed3max.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Speed"][1].ToString();
            GantryYspeed3min.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Speed"][0].ToString();
            GantryYspeed3max.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Speed"][1].ToString();
            GantryZspeed3min.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Speed"][0].ToString();
            GantryZspeed3max.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Speed"][1].ToString();
            ShareValues.AxisSpeedMin[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Speed"][1].ToString());
            ShareValues.AxisSpeedMin[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Speed"][0].ToString());
            ShareValues.AxisSpeedMax[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Speed"][1].ToString());

            Beltacc3min.Text = LimitJsonObject["RecheckArea"]["Belt"]["Acc"][0].ToString();
            Beltacc3max.Text = LimitJsonObject["RecheckArea"]["Belt"]["Acc"][1].ToString();
            Liftacc3min.Text = LimitJsonObject["RecheckArea"]["Lift"]["Acc"][0].ToString();
            Liftacc3max.Text = LimitJsonObject["RecheckArea"]["Lift"]["Acc"][1].ToString();
            GantryXacc3min.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Acc"][0].ToString();
            GantryXacc3max.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Acc"][1].ToString();
            GantryYacc3min.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Acc"][0].ToString();
            GantryYacc3max.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Acc"][1].ToString();
            GantryZacc3min.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Acc"][0].ToString();
            GantryZacc3max.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Acc"][1].ToString();
            ShareValues.AxisAccMin[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Acc"][1].ToString());
            ShareValues.AxisAccMin[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Acc"][0].ToString());
            ShareValues.AxisAccMax[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Acc"][1].ToString());

            Beltdec3min.Text = LimitJsonObject["RecheckArea"]["Belt"]["Dec"][0].ToString();
            Beltdec3max.Text = LimitJsonObject["RecheckArea"]["Belt"]["Dec"][1].ToString();
            Liftdec3min.Text = LimitJsonObject["RecheckArea"]["Lift"]["Dec"][0].ToString();
            Liftdec3max.Text = LimitJsonObject["RecheckArea"]["Lift"]["Dec"][1].ToString();
            GantryXdec3min.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Dec"][0].ToString();
            GantryXdec3max.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Dec"][1].ToString();
            GantryYdec3min.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Dec"][0].ToString();
            GantryYdec3max.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Dec"][1].ToString();
            GantryZdec3min.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Dec"][0].ToString();
            GantryZdec3max.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Dec"][1].ToString();
            ShareValues.AxisDecMin[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Dec"][1].ToString());
            ShareValues.AxisDecMin[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Dec"][0].ToString());
            ShareValues.AxisDecMax[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Dec"][1].ToString());

            Beltaxis3min.Text = LimitJsonObject["RecheckArea"]["Belt"]["Axis"][0].ToString();
            Beltaxis3max.Text = LimitJsonObject["RecheckArea"]["Belt"]["Axis"][1].ToString();
            Liftaxis3min.Text = LimitJsonObject["RecheckArea"]["Lift"]["Axis"][0].ToString();
            Liftaxis3max.Text = LimitJsonObject["RecheckArea"]["Lift"]["Axis"][1].ToString();
            GantryXaxis3min.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Axis"][0].ToString();
            GantryXaxis3max.Text = LimitJsonObject["RecheckArea"]["GantryX"]["Axis"][1].ToString();
            GantryYaxis3min.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Axis"][0].ToString();
            GantryYaxis3max.Text = LimitJsonObject["RecheckArea"]["GantryY"]["Axis"][1].ToString();
            GantryZaxis3min.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Axis"][0].ToString();
            GantryZaxis3max.Text = LimitJsonObject["RecheckArea"]["GantryZ"]["Axis"][1].ToString();
            ShareValues.AxisMin[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Axis"][0].ToString());
            ShareValues.AxisMax[12] = double.Parse(LimitJsonObject["RecheckArea"]["Belt"]["Axis"][1].ToString());
            ShareValues.AxisMin[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Axis"][0].ToString());
            ShareValues.AxisMax[13] = double.Parse(LimitJsonObject["RecheckArea"]["Lift"]["Axis"][1].ToString());
            ShareValues.AxisMin[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Axis"][0].ToString());
            ShareValues.AxisMax[14] = double.Parse(LimitJsonObject["RecheckArea"]["GantryX"]["Axis"][1].ToString());
            ShareValues.AxisMin[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Axis"][0].ToString());
            ShareValues.AxisMax[15] = double.Parse(LimitJsonObject["RecheckArea"]["GantryY"]["Axis"][1].ToString());
            ShareValues.AxisMin[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Axis"][0].ToString());
            ShareValues.AxisMax[16] = double.Parse(LimitJsonObject["RecheckArea"]["GantryZ"]["Axis"][1].ToString());

            AxisLimitList = new Dictionary<string, double>
            {
                { "Beltspeed1min", ShareValues.AxisSpeedMin[0]},
                { "Beltspeed1max", ShareValues.AxisSpeedMax[0]},
                { "Liftspeed1min", ShareValues.AxisSpeedMin[1]},
                { "Liftspeed1max", ShareValues.AxisSpeedMax[1]},
                { "GantryXspeed1min", ShareValues.AxisSpeedMin[2]},
                { "GantryXspeed1max", ShareValues.AxisSpeedMax[2]},
                { "GantryYspeed1min", ShareValues.AxisSpeedMin[3]},
                { "GantryYspeed1max", ShareValues.AxisSpeedMax[3]},
                { "GantryZspeed1min", ShareValues.AxisSpeedMin[4]},
                { "GantryZspeed1max", ShareValues.AxisSpeedMax[4]},
                { "Beltacc1min", ShareValues.AxisAccMin[0]},
                { "Beltacc1max", ShareValues.AxisAccMax[0]},
                { "Liftacc1min", ShareValues.AxisAccMin[1]},
                { "Liftacc1max", ShareValues.AxisAccMax[1]},
                { "GantryXacc1min", ShareValues.AxisAccMin[2]},
                { "GantryXacc1max", ShareValues.AxisAccMax[2]},
                { "GantryYacc1min", ShareValues.AxisAccMin[3]},
                { "GantryYacc1max", ShareValues.AxisAccMax[3]},
                { "GantryZacc1min", ShareValues.AxisAccMin[4]},
                { "GantryZacc1max", ShareValues.AxisAccMax[4]},
                { "Beltdec1min", ShareValues.AxisDecMin[0]},
                { "Beltdec1max", ShareValues.AxisDecMax[0]},
                { "Liftdec1min", ShareValues.AxisDecMin[1]},
                { "Liftdec1max", ShareValues.AxisDecMax[1]},
                { "GantryXdec1min", ShareValues.AxisDecMin[2]},
                { "GantryXdec1max", ShareValues.AxisDecMax[2]},
                { "GantryYdec1min", ShareValues.AxisDecMin[3]},
                { "GantryYdec1max", ShareValues.AxisDecMax[3]},
                { "GantryZdec1min", ShareValues.AxisDecMin[4]},
                { "GantryZdec1max", ShareValues.AxisDecMax[4]},

                { "Beltspeed2min", ShareValues.AxisSpeedMin[5]},
                { "Beltspeed2max", ShareValues.AxisSpeedMax[5]},
                { "Liftspeed2min", ShareValues.AxisSpeedMin[6]},
                { "Liftspeed2max", ShareValues.AxisSpeedMax[6]},
                { "GantryXspeed2min", ShareValues.AxisSpeedMin[7]},
                { "GantryXspeed2max", ShareValues.AxisSpeedMax[7]},
                { "GantryYspeed2min", ShareValues.AxisSpeedMin[8]},
                { "GantryYspeed2max", ShareValues.AxisSpeedMax[8]},
                { "GantryZspeed2min", ShareValues.AxisSpeedMin[9]},
                { "GantryZspeed2max", ShareValues.AxisSpeedMax[9]},
                { "PickerZspeedmin", ShareValues.AxisSpeedMin[10]},
                { "PickerZspeedmax", ShareValues.AxisSpeedMax[10]},
                { "PickerRspeedmin", ShareValues.AxisSpeedMin[11]},
                { "PickerRspeedmax", ShareValues.AxisSpeedMax[11]},
                { "Beltacc2min", ShareValues.AxisAccMin[5]},
                { "Beltacc2max", ShareValues.AxisAccMax[5]},
                { "Liftacc2min", ShareValues.AxisAccMin[6]},
                { "Liftacc2max", ShareValues.AxisAccMax[6]},
                { "GantryXacc2min", ShareValues.AxisAccMin[7]},
                { "GantryXacc2max", ShareValues.AxisAccMax[7]},
                { "GantryYacc2min", ShareValues.AxisAccMin[8]},
                { "GantryYacc2max", ShareValues.AxisAccMax[8]},
                { "GantryZacc2min", ShareValues.AxisAccMin[9]},
                { "GantryZacc2max", ShareValues.AxisAccMax[9]},
                { "PickerZaccmin", ShareValues.AxisAccMin[10]},
                { "PickerZaccmax", ShareValues.AxisAccMax[10]},
                { "PickerRaccmin", ShareValues.AxisAccMin[11]},
                { "PickerRaccmax", ShareValues.AxisAccMax[11]},
                { "Beltdec2min", ShareValues.AxisDecMin[5]},
                { "Beltdec2max", ShareValues.AxisDecMax[5]},
                { "Liftdec2min", ShareValues.AxisDecMin[6]},
                { "Liftdec2max", ShareValues.AxisDecMax[6]},
                { "GantryXdec2min", ShareValues.AxisDecMin[7]},
                { "GantryXdec2max", ShareValues.AxisDecMax[7]},
                { "GantryYdec2min", ShareValues.AxisDecMin[8]},
                { "GantryYdec2max", ShareValues.AxisDecMax[8]},
                { "GantryZdec2min", ShareValues.AxisDecMin[9]},
                { "GantryZdec2max", ShareValues.AxisDecMax[9]},
                { "PickerZdecmin", ShareValues.AxisDecMin[10]},
                { "PickerZdecmax", ShareValues.AxisDecMax[10]},
                { "PickerRdecmin", ShareValues.AxisDecMin[11]},
                { "PickerRdecmax", ShareValues.AxisDecMax[11]},

                { "Beltspeed3min", ShareValues.AxisSpeedMin[12]},
                { "Beltspeed3max", ShareValues.AxisSpeedMax[12]},
                { "Liftspeed3min", ShareValues.AxisSpeedMin[13]},
                { "Liftspeed3max", ShareValues.AxisSpeedMax[13]},
                { "GantryXspeed3min", ShareValues.AxisSpeedMin[14]},
                { "GantryXspeed3max", ShareValues.AxisSpeedMax[14]},
                { "GantryYspeed3min", ShareValues.AxisSpeedMin[15]},
                { "GantryYspeed3max", ShareValues.AxisSpeedMax[15]},
                { "GantryZspeed3min", ShareValues.AxisSpeedMin[16]},
                { "GantryZspeed3max", ShareValues.AxisSpeedMax[16]},
                { "Beltacc3min", ShareValues.AxisAccMin[12]},
                { "Beltacc3max", ShareValues.AxisAccMax[12]},
                { "Liftacc3min", ShareValues.AxisAccMin[13]},
                { "Liftacc3max", ShareValues.AxisAccMax[13]},
                { "GantryXacc3min", ShareValues.AxisAccMin[14]},
                { "GantryXacc3max", ShareValues.AxisAccMax[14]},
                { "GantryYacc3min", ShareValues.AxisAccMin[15]},
                { "GantryYacc3max", ShareValues.AxisAccMax[15]},
                { "GantryZacc3min", ShareValues.AxisAccMin[16]},
                { "GantryZacc3max", ShareValues.AxisAccMax[16]},
                { "Beltdec3min", ShareValues.AxisDecMin[12]},
                { "Beltdec3max", ShareValues.AxisDecMax[12]},
                { "Liftdec3min", ShareValues.AxisDecMin[13]},
                { "Liftdec3max", ShareValues.AxisDecMax[13]},
                { "GantryXdec3min", ShareValues.AxisDecMin[14]},
                { "GantryXdec3max", ShareValues.AxisDecMax[14]},
                { "GantryYdec3min", ShareValues.AxisDecMin[15]},
                { "GantryYdec3max", ShareValues.AxisDecMax[15]},
                { "GantryZdec3min", ShareValues.AxisDecMin[16]},
                { "GantryZdec3max", ShareValues.AxisDecMax[16]}
            };
            init = true;
        }

        private void DoubleText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.Handled =!( new Regex(@"^\d*\.?\d*$").IsMatch(e.Text));
            Regex regex = new Regex("[^0-9\\-\\.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Axis_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                AxisLimitList[textbox.Name] = result;
            }
        }
        private void AxisParam_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == ""){
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void PickerSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void PickerRSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void ACC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void PickerACC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void PickerRACC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void DEC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void PickerDEC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void PickerRDEC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textbox.Text, out num))
                {
                    textbox.Text = textbox.Text.Remove(offset, change[0].AddedLength);
                    textbox.Select(offset, 0);
                }
            }
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            if (textbox.Name.Substring(textbox.Name.Length - 3, 3) == "min" || textbox.Name.Substring(textbox.Name.Length - 3, 3) == "max")
                return;
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success && init)
            {
                if (result < AxisLimitList[textbox.Name + "min"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "min"].ToString();
                }
                else if (result > AxisLimitList[textbox.Name + "max"])
                {
                    textbox.Text = AxisLimitList[textbox.Name + "max"].ToString();
                }
            }
        }

        private void LaserpointsFiledialog_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            // 创建 OpenFileDialog 实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置过滤器
            openFileDialog.Filter = "Text files (*.json)|*.json";
            // 设置默认的文件名或扩展名
            openFileDialog.DefaultExt = ".json";
            // 设置初始目录
            openFileDialog.InitialDirectory = @"D:\";
            // 设置对话框标题
            openFileDialog.Title = "Select a json file";
            // 显示打开文件对话框，并检查用户是否点击了“打开”按钮
            if (openFileDialog.ShowDialog() == true)
            {
                // 获取用户选择的文件路径
                string filePath = openFileDialog.FileName;
                string jsonString = File.ReadAllText(filePath);
                palletpointswindow = new PalletPointsWindow();
                try
                {
                    StreamReader file = File.OpenText(filePath);
                    JsonTextReader reader = new JsonTextReader(file);
                    JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                    if (button != null)
                    {
                        if (button.Name == "LaserpointsFiledialog")
                        {
                            Laserpointsconfig.Text = filePath;
                            palletpointswindow.jsontype = PointsType.Laser;
                            scanningareaparams.PalletID = int.Parse(jsonObject["PalletID"].ToString());
                            scanningareaparams.PalletW = int.Parse(jsonObject["PalletW"].ToString());
                            scanningareaparams.PalletH = int.Parse(jsonObject["PalletH"].ToString());
                        }
                        else if (button.Name == "ModulepointsFiledialog")
                        {
                            Modulepointsconfig.Text = filePath;
                            palletpointswindow.jsontype = PointsType.Laser;
                            assemblyareaparams.PalletID = int.Parse(jsonObject["PalletID"].ToString());
                            assemblyareaparams.PalletW = int.Parse(jsonObject["PalletW"].ToString());
                            assemblyareaparams.PalletH = int.Parse(jsonObject["PalletH"].ToString());
                        }
                        else if (button.Name == "CheckpointsFiledialog")
                        {
                            Checkpointsconfig.Text = filePath;
                            palletpointswindow.jsontype = PointsType.Feeder;
                            assemblyareaparams.PalletID = int.Parse(jsonObject["PalletID"].ToString());
                            assemblyareaparams.PalletW = int.Parse(jsonObject["PalletW"].ToString());
                            assemblyareaparams.PalletH = int.Parse(jsonObject["PalletH"].ToString());
                        }
                        else if (button.Name == "FeederpointsFiledialog")
                        {
                            Feederpointsconfig.Text = filePath;
                            palletpointswindow.jsontype = PointsType.Feeder;
                            assemblyareaparams.PalletID = int.Parse(jsonObject["PalletID"].ToString());
                            assemblyareaparams.PalletW = int.Parse(jsonObject["PalletW"].ToString());
                            assemblyareaparams.PalletH = int.Parse(jsonObject["PalletH"].ToString());
                        }
                    }

                    palletpointswindow.jsonObject = jsonObject;
                    palletpointswindow.jsonpath = filePath;
                    palletpointswindow.modulenumX = int.Parse(jsonObject["PalletW"].ToString());
                    palletpointswindow.modulenumY = int.Parse(jsonObject["PalletH"].ToString());
                    palletpointswindow.pointsnum = int.Parse(jsonObject["PointsNum"].ToString());
                    palletpointswindow.setWindow();
                    if (file != null)
                    {
                        file.Dispose();
                    }
                    palletpointswindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    //读取json失败
                }
            }
        }

        private void Genratescanjson_Click(object sender, RoutedEventArgs e)
        {
            GenrateJson genratescanjson = new GenrateJson();
            Button button = (Button)sender;
            string name = button.Name.Substring(7, button.Name.Length - 11);
            genratescanjson.jsonname = name + "points";
            if (name == "Feeder" || name == "Check")
            {
                genratescanjson.jsontype = PointsType.Feeder;
                genratescanjson.ZR1.Content = "R1";
                genratescanjson.ZRW.Content = "RW";
                genratescanjson.ZRH.Content = "RH";
            }
            else
            {
                genratescanjson.jsontype = PointsType.Laser;
            }
            genratescanjson.ShowDialog();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            JObject jsObj = new JObject();
            JObject jsScanningArea = new JObject();
            JObject jsAssemblyArea = new JObject();
            JObject jsRecheckArea = new JObject();

            double result; bool success;
            JObject jsAxisInfo1 = new JObject();
            success = double.TryParse(Beltspeed1.Text, out result);
            jsAxisInfo1.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(Beltacc1.Text, out result);
            jsAxisInfo1.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(Beltdec1.Text, out result);
            jsAxisInfo1.Add("Dec", Math.Round(result, 4));
            jsScanningArea.Add("Belt", jsAxisInfo1);

            JObject jsAxisInfo2 = new JObject();
            success = double.TryParse(Liftspeed1.Text, out result);
            jsAxisInfo2.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(Liftacc1.Text, out result);
            jsAxisInfo2.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(Liftdec1.Text, out result);
            jsAxisInfo2.Add("Dec", Math.Round(result, 4));
            jsScanningArea.Add("Lift", jsAxisInfo2);

            JObject jsAxisInfo3 = new JObject();
            success = double.TryParse(GantryXspeed1.Text, out result);
            jsAxisInfo3.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryXacc1.Text, out result);
            jsAxisInfo3.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryXdec1.Text, out result);
            jsAxisInfo3.Add("Dec", Math.Round(result, 4));
            jsScanningArea.Add("GantryX", jsAxisInfo3);

            JObject jsAxisInfo4 = new JObject();
            success = double.TryParse(GantryYspeed1.Text, out result);
            jsAxisInfo4.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryYacc1.Text, out result);
            jsAxisInfo4.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryYdec1.Text, out result);
            jsAxisInfo4.Add("Dec", Math.Round(result, 4));
            jsScanningArea.Add("GantryY", jsAxisInfo4);

            JObject jsAxisInfo5 = new JObject();
            success = double.TryParse(GantryZspeed1.Text, out result);
            jsAxisInfo5.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryZacc1.Text, out result);
            jsAxisInfo5.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryZdec1.Text, out result);
            jsAxisInfo5.Add("Dec", Math.Round(result, 4));
            jsScanningArea.Add("GantryZ", jsAxisInfo5);

            JObject jsAxisInfo6 = new JObject();
            success = double.TryParse(Beltspeed2.Text, out result);
            jsAxisInfo6.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(Beltacc2.Text, out result);
            jsAxisInfo6.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(Beltdec2.Text, out result);
            jsAxisInfo6.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("Belt", jsAxisInfo6);

            JObject jsAxisInfo7 = new JObject();
            success = double.TryParse(Liftspeed2.Text, out result);
            jsAxisInfo7.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(Liftacc2.Text, out result);
            jsAxisInfo7.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(Liftdec2.Text, out result);
            jsAxisInfo7.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("Lift", jsAxisInfo7);

            JObject jsAxisInfo8 = new JObject();
            success = double.TryParse(GantryXspeed2.Text, out result);
            jsAxisInfo8.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryXacc2.Text, out result);
            jsAxisInfo8.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryXdec2.Text, out result);
            jsAxisInfo8.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("GantryX", jsAxisInfo8);

            JObject jsAxisInfo9 = new JObject();
            success = double.TryParse(GantryYspeed2.Text, out result);
            jsAxisInfo9.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryYacc2.Text, out result);
            jsAxisInfo9.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryYdec2.Text, out result);
            jsAxisInfo9.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("GantryY", jsAxisInfo9);

            JObject jsAxisInfo10 = new JObject();
            success = double.TryParse(GantryZspeed2.Text, out result);
            jsAxisInfo10.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryZacc2.Text, out result);
            jsAxisInfo10.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryZdec2.Text, out result);
            jsAxisInfo10.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("GantryZ", jsAxisInfo10);

            JObject jsAxisInfo11 = new JObject();
            success = double.TryParse(PickerZspeed.Text, out result);
            jsAxisInfo11.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(PickerZacc.Text, out result);
            jsAxisInfo11.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(PickerZdec.Text, out result);
            jsAxisInfo11.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("PickerZ", jsAxisInfo11);

            JObject jsAxisInfo12 = new JObject();
            success = double.TryParse(PickerRspeed.Text, out result);
            jsAxisInfo12.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(PickerRacc.Text, out result);
            jsAxisInfo12.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(PickerRdec.Text, out result);
            jsAxisInfo12.Add("Dec", Math.Round(result, 4));
            jsAssemblyArea.Add("PickerR", jsAxisInfo12);

            JObject jsAxisInfo13 = new JObject();
            success = double.TryParse(Beltspeed3.Text, out result);
            jsAxisInfo13.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(Beltacc3.Text, out result);
            jsAxisInfo13.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(Beltdec3.Text, out result);
            jsAxisInfo13.Add("Dec", Math.Round(result, 4));
            jsRecheckArea.Add("Belt", jsAxisInfo13);

            JObject jsAxisInfo14 = new JObject();
            success = double.TryParse(Liftspeed3.Text, out result);
            jsAxisInfo14.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(Liftacc3.Text, out result);
            jsAxisInfo14.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(Liftdec3.Text, out result);
            jsAxisInfo14.Add("Dec", Math.Round(result, 4));
            jsRecheckArea.Add("Lift", jsAxisInfo14);

            JObject jsAxisInfo15 = new JObject();
            success = double.TryParse(GantryXspeed3.Text, out result);
            jsAxisInfo15.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryXacc3.Text, out result);
            jsAxisInfo15.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryXdec3.Text, out result);
            jsAxisInfo15.Add("Dec", Math.Round(result, 4));
            jsRecheckArea.Add("GantryX", jsAxisInfo15);

            JObject jsAxisInfo16 = new JObject();
            success = double.TryParse(GantryYspeed3.Text, out result);
            jsAxisInfo16.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryYacc3.Text, out result);
            jsAxisInfo16.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryYdec3.Text, out result);
            jsAxisInfo16.Add("Dec", Math.Round(result, 4));
            jsRecheckArea.Add("GantryY", jsAxisInfo16);

            JObject jsAxisInfo17 = new JObject();
            success = double.TryParse(GantryZspeed3.Text, out result);
            jsAxisInfo17.Add("Speed", Math.Round(result, 4));
            success = double.TryParse(GantryZacc3.Text, out result);
            jsAxisInfo17.Add("Acc", Math.Round(result, 4));
            success = double.TryParse(GantryZdec3.Text, out result);
            jsAxisInfo17.Add("Dec", Math.Round(result, 4));
            jsRecheckArea.Add("GantryZ", jsAxisInfo17);

            jsObj.Add("ScanningArea", jsScanningArea);
            jsObj.Add("AssemblyArea", jsAssemblyArea);
            jsObj.Add("RecheckArea", jsRecheckArea);
            string strSrc = Convert.ToString(jsObj);//将json装换为string
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\Params.json", strSrc, System.Text.Encoding.UTF8);
        }

        private void ApplyLimit_Click(object sender, RoutedEventArgs e)
        {
            LimitJsonObject["ScanningArea"]["Belt"]["Speed"][0] = double.Parse(Beltspeed1min.Text);
            LimitJsonObject["ScanningArea"]["Belt"]["Speed"][1] = double.Parse(Beltspeed1max.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Speed"][0] = double.Parse(Liftspeed1min.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Speed"][1] = double.Parse(Liftspeed1max.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Speed"][0] = double.Parse(GantryXspeed1min.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Speed"][1] = double.Parse(GantryXspeed1max.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Speed"][0] = double.Parse(GantryYspeed1min.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Speed"][1] = double.Parse(GantryYspeed1max.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Speed"][0] = double.Parse(GantryZspeed1min.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Speed"][1] = double.Parse(GantryZspeed1max.Text);
            ShareValues.AxisSpeedMin[0] = double.Parse(Beltspeed1min.Text);
            ShareValues.AxisSpeedMax[0] = double.Parse(Beltspeed1max.Text);
            ShareValues.AxisSpeedMin[1] = double.Parse(Liftspeed1min.Text);
            ShareValues.AxisSpeedMax[1] = double.Parse(Liftspeed1max.Text);
            ShareValues.AxisSpeedMin[2] = double.Parse(GantryXspeed1min.Text);
            ShareValues.AxisSpeedMax[2] = double.Parse(GantryXspeed1max.Text);
            ShareValues.AxisSpeedMin[3] = double.Parse(GantryYspeed1min.Text);
            ShareValues.AxisSpeedMax[3] = double.Parse(GantryYspeed1max.Text);
            ShareValues.AxisSpeedMin[4] = double.Parse(GantryZspeed1min.Text);
            ShareValues.AxisSpeedMax[4] = double.Parse(GantryZspeed1max.Text);

            LimitJsonObject["ScanningArea"]["Belt"]["Acc"][0] = double.Parse(Beltacc1min.Text);
            LimitJsonObject["ScanningArea"]["Belt"]["Acc"][1] = double.Parse(Beltacc1max.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Acc"][0] = double.Parse(Liftacc1min.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Acc"][1] = double.Parse(Liftacc1max.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Acc"][0] = double.Parse(GantryXacc1min.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Acc"][1] = double.Parse(GantryXacc1max.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Acc"][0] = double.Parse(GantryYacc1min.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Acc"][1] = double.Parse(GantryYacc1max.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Acc"][0] = double.Parse(GantryZacc1min.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Acc"][1] = double.Parse(GantryZacc1max.Text);
            ShareValues.AxisAccMin[0] = double.Parse(Beltacc1min.Text);
            ShareValues.AxisAccMax[0] = double.Parse(Beltacc1max.Text);
            ShareValues.AxisAccMin[1] = double.Parse(Liftacc1min.Text);
            ShareValues.AxisAccMax[1] = double.Parse(Liftacc1max.Text);
            ShareValues.AxisAccMin[2] = double.Parse(GantryXacc1min.Text);
            ShareValues.AxisAccMax[2] = double.Parse(GantryXacc1max.Text);
            ShareValues.AxisAccMin[3] = double.Parse(GantryYacc1min.Text);
            ShareValues.AxisAccMax[3] = double.Parse(GantryYacc1max.Text);
            ShareValues.AxisAccMin[4] = double.Parse(GantryZacc1min.Text);
            ShareValues.AxisAccMax[4] = double.Parse(GantryZacc1max.Text);

            LimitJsonObject["ScanningArea"]["Belt"]["Dec"][0] = double.Parse(Beltdec1min.Text);
            LimitJsonObject["ScanningArea"]["Belt"]["Dec"][1] = double.Parse(Beltdec1max.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Dec"][0] = double.Parse(Liftdec1min.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Dec"][1] = double.Parse(Liftdec1max.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Dec"][0] = double.Parse(GantryXdec1min.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Dec"][1] = double.Parse(GantryXdec1max.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Dec"][0] = double.Parse(GantryYdec1min.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Dec"][1] = double.Parse(GantryYdec1max.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Dec"][0] = double.Parse(GantryZdec1min.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Dec"][1] = double.Parse(GantryZdec1max.Text);
            ShareValues.AxisDecMin[0] = double.Parse(Beltdec1min.Text);
            ShareValues.AxisDecMax[0] = double.Parse(Beltdec1max.Text);
            ShareValues.AxisDecMin[1] = double.Parse(Liftdec1min.Text);
            ShareValues.AxisDecMax[1] = double.Parse(Liftdec1max.Text);
            ShareValues.AxisDecMin[2] = double.Parse(GantryXdec1min.Text);
            ShareValues.AxisDecMax[2] = double.Parse(GantryXdec1max.Text);
            ShareValues.AxisDecMin[3] = double.Parse(GantryYdec1min.Text);
            ShareValues.AxisDecMax[3] = double.Parse(GantryYdec1max.Text);
            ShareValues.AxisDecMin[4] = double.Parse(GantryZdec1min.Text);
            ShareValues.AxisDecMax[4] = double.Parse(GantryZdec1max.Text);

            LimitJsonObject["ScanningArea"]["Belt"]["Axis"][0] = double.Parse(Beltaxis1min.Text);
            LimitJsonObject["ScanningArea"]["Belt"]["Axis"][1] = double.Parse(Beltaxis1max.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Axis"][0] = double.Parse(Liftaxis1min.Text);
            LimitJsonObject["ScanningArea"]["Lift"]["Axis"][1] = double.Parse(Liftaxis1max.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Axis"][0] = double.Parse(GantryXaxis1min.Text);
            LimitJsonObject["ScanningArea"]["GantryX"]["Axis"][1] = double.Parse(GantryXaxis1max.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Axis"][0] = double.Parse(GantryYaxis1min.Text);
            LimitJsonObject["ScanningArea"]["GantryY"]["Axis"][1] = double.Parse(GantryYaxis1max.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Axis"][0] = double.Parse(GantryZaxis1min.Text);
            LimitJsonObject["ScanningArea"]["GantryZ"]["Axis"][1] = double.Parse(GantryZaxis1max.Text);
            ShareValues.AxisMin[0] = double.Parse(Beltaxis1min.Text);
            ShareValues.AxisMax[0] = double.Parse(Beltaxis1max.Text);
            ShareValues.AxisMin[1] = double.Parse(Liftaxis1min.Text);
            ShareValues.AxisMax[1] = double.Parse(Liftaxis1max.Text);
            ShareValues.AxisMin[2] = double.Parse(GantryXaxis1min.Text);
            ShareValues.AxisMax[2] = double.Parse(GantryXaxis1max.Text);
            ShareValues.AxisMin[3] = double.Parse(GantryYaxis1min.Text);
            ShareValues.AxisMax[3] = double.Parse(GantryYaxis1max.Text);
            ShareValues.AxisMin[4] = double.Parse(GantryZaxis1min.Text);
            ShareValues.AxisMax[4] = double.Parse(GantryZaxis1max.Text);

            LimitJsonObject["AssemblyArea"]["Belt"]["Speed"][0] = double.Parse(Beltspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["Belt"]["Speed"][1] = double.Parse(Beltspeed2max.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Speed"][0] = double.Parse(Liftspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Speed"][1] = double.Parse(Liftspeed2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Speed"][0] = double.Parse(GantryXspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Speed"][1] = double.Parse(GantryXspeed2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Speed"][0] = double.Parse(GantryYspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Speed"][1] = double.Parse(GantryYspeed2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Speed"][0] = double.Parse(GantryZspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Speed"][1] = double.Parse(GantryZspeed2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Speed"][0] = double.Parse(PickerZspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Speed"][1] = double.Parse(PickerZspeed2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Speed"][0] = double.Parse(PickerRspeed2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Speed"][1] = double.Parse(PickerRspeed2max.Text);
            ShareValues.AxisSpeedMin[5] = double.Parse(Beltspeed2min.Text);
            ShareValues.AxisSpeedMax[5] = double.Parse(Beltspeed2max.Text);
            ShareValues.AxisSpeedMin[6] = double.Parse(Liftspeed2min.Text);
            ShareValues.AxisSpeedMax[6] = double.Parse(Liftspeed2max.Text);
            ShareValues.AxisSpeedMin[7] = double.Parse(GantryXspeed2min.Text);
            ShareValues.AxisSpeedMax[7] = double.Parse(GantryXspeed2max.Text);
            ShareValues.AxisSpeedMin[8] = double.Parse(GantryYspeed2min.Text);
            ShareValues.AxisSpeedMax[8] = double.Parse(GantryYspeed2max.Text);
            ShareValues.AxisSpeedMin[9] = double.Parse(GantryZspeed2min.Text);
            ShareValues.AxisSpeedMax[9] = double.Parse(GantryZspeed2max.Text);
            ShareValues.AxisSpeedMin[10] = double.Parse(PickerZspeed2min.Text);
            ShareValues.AxisSpeedMax[10] = double.Parse(PickerZspeed2max.Text);
            ShareValues.AxisSpeedMin[11] = double.Parse(PickerRspeed2min.Text);
            ShareValues.AxisSpeedMax[11] = double.Parse(PickerRspeed2max.Text);

            LimitJsonObject["AssemblyArea"]["Belt"]["Acc"][0] = double.Parse(Beltacc2min.Text);
            LimitJsonObject["AssemblyArea"]["Belt"]["Acc"][1] = double.Parse(Beltacc2max.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Acc"][0] = double.Parse(Liftacc2min.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Acc"][1] = double.Parse(Liftacc2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Acc"][0] = double.Parse(GantryXacc2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Acc"][1] = double.Parse(GantryXacc2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Acc"][0] = double.Parse(GantryYacc2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Acc"][1] = double.Parse(GantryYacc2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Acc"][0] = double.Parse(GantryZacc2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Acc"][1] = double.Parse(GantryZacc2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Acc"][0] = double.Parse(PickerZacc2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Acc"][1] = double.Parse(PickerZacc2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Acc"][0] = double.Parse(PickerRacc2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Acc"][1] = double.Parse(PickerRacc2max.Text);
            ShareValues.AxisAccMin[5] = double.Parse(Beltacc2min.Text);
            ShareValues.AxisAccMax[5] = double.Parse(Beltacc2max.Text);
            ShareValues.AxisAccMin[6] = double.Parse(Liftacc2min.Text);
            ShareValues.AxisAccMax[6] = double.Parse(Liftacc2max.Text);
            ShareValues.AxisAccMin[7] = double.Parse(GantryXacc2min.Text);
            ShareValues.AxisAccMax[7] = double.Parse(GantryXacc2max.Text);
            ShareValues.AxisAccMin[8] = double.Parse(GantryYacc2min.Text);
            ShareValues.AxisAccMax[8] = double.Parse(GantryYacc2max.Text);
            ShareValues.AxisAccMin[9] = double.Parse(GantryZacc2min.Text);
            ShareValues.AxisAccMax[9] = double.Parse(GantryZacc2max.Text);
            ShareValues.AxisAccMin[10] = double.Parse(PickerZacc2min.Text);
            ShareValues.AxisAccMax[10] = double.Parse(PickerZacc2max.Text);
            ShareValues.AxisAccMin[11] = double.Parse(PickerRacc2min.Text);
            ShareValues.AxisAccMax[11] = double.Parse(PickerRacc2max.Text);

            LimitJsonObject["AssemblyArea"]["Belt"]["Dec"][0] = double.Parse(Beltdec2min.Text);
            LimitJsonObject["AssemblyArea"]["Belt"]["Dec"][1] = double.Parse(Beltdec2max.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Dec"][0] = double.Parse(Liftdec2min.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Dec"][1] = double.Parse(Liftdec2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Dec"][0] = double.Parse(GantryXdec2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Dec"][1] = double.Parse(GantryXdec2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Dec"][0] = double.Parse(GantryYdec2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Dec"][1] = double.Parse(GantryYdec2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Dec"][0] = double.Parse(GantryZdec2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Dec"][1] = double.Parse(GantryZdec2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Dec"][0] = double.Parse(PickerZdec2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Dec"][1] = double.Parse(PickerZdec2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Dec"][0] = double.Parse(PickerRdec2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Dec"][1] = double.Parse(PickerRdec2max.Text);
            ShareValues.AxisDecMin[5] = double.Parse(Beltdec2min.Text);
            ShareValues.AxisDecMax[5] = double.Parse(Beltdec2max.Text);
            ShareValues.AxisDecMin[6] = double.Parse(Liftdec2min.Text);
            ShareValues.AxisDecMax[6] = double.Parse(Liftdec2max.Text);
            ShareValues.AxisDecMin[7] = double.Parse(GantryXdec2min.Text);
            ShareValues.AxisDecMax[7] = double.Parse(GantryXdec2max.Text);
            ShareValues.AxisDecMin[8] = double.Parse(GantryYdec2min.Text);
            ShareValues.AxisDecMax[8] = double.Parse(GantryYdec2max.Text);
            ShareValues.AxisDecMin[9] = double.Parse(GantryZdec2min.Text);
            ShareValues.AxisDecMax[9] = double.Parse(GantryZdec2max.Text);
            ShareValues.AxisDecMin[10] = double.Parse(PickerZdec2min.Text);
            ShareValues.AxisDecMax[10] = double.Parse(PickerZdec2max.Text);
            ShareValues.AxisDecMin[11] = double.Parse(PickerRdec2min.Text);
            ShareValues.AxisDecMax[11] = double.Parse(PickerRdec2max.Text);

            LimitJsonObject["AssemblyArea"]["Belt"]["Axis"][0] = double.Parse(Beltaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["Belt"]["Axis"][1] = double.Parse(Beltaxis2max.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Axis"][0] = double.Parse(Liftaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["Lift"]["Axis"][1] = double.Parse(Liftaxis2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Axis"][0] = double.Parse(GantryXaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryX"]["Axis"][1] = double.Parse(GantryXaxis2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Axis"][0] = double.Parse(GantryYaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryY"]["Axis"][1] = double.Parse(GantryYaxis2max.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Axis"][0] = double.Parse(GantryZaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["GantryZ"]["Axis"][1] = double.Parse(GantryZaxis2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Axis"][0] = double.Parse(PickerZaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerZ"]["Axis"][1] = double.Parse(PickerZaxis2max.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Axis"][0] = double.Parse(PickerRaxis2min.Text);
            LimitJsonObject["AssemblyArea"]["PickerR"]["Axis"][1] = double.Parse(PickerRaxis2max.Text);
            ShareValues.AxisMin[5] = double.Parse(Beltaxis2min.Text);
            ShareValues.AxisMax[5] = double.Parse(Beltaxis2max.Text);
            ShareValues.AxisMin[6] = double.Parse(Liftaxis2min.Text);
            ShareValues.AxisMax[6] = double.Parse(Liftaxis2max.Text);
            ShareValues.AxisMin[7] = double.Parse(GantryXaxis2min.Text);
            ShareValues.AxisMax[7] = double.Parse(GantryXaxis2max.Text);
            ShareValues.AxisMin[8] = double.Parse(GantryYaxis2min.Text);
            ShareValues.AxisMax[8] = double.Parse(GantryYaxis2max.Text);
            ShareValues.AxisMin[9] = double.Parse(GantryZaxis2min.Text);
            ShareValues.AxisMax[9] = double.Parse(GantryZaxis2max.Text);
            ShareValues.AxisMin[10] = double.Parse(PickerZaxis2min.Text);
            ShareValues.AxisMax[10] = double.Parse(PickerZaxis2max.Text);
            ShareValues.AxisMin[11] = double.Parse(PickerRaxis2min.Text);
            ShareValues.AxisMax[11] = double.Parse(PickerRaxis2max.Text);

            LimitJsonObject["RecheckArea"]["Belt"]["Speed"][0] = double.Parse(Beltspeed3min.Text);
            LimitJsonObject["RecheckArea"]["Belt"]["Speed"][1] = double.Parse(Beltspeed3max.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Speed"][0] = double.Parse(Liftspeed3min.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Speed"][1] = double.Parse(Liftspeed3max.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Speed"][0] = double.Parse(GantryXspeed3min.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Speed"][1] = double.Parse(GantryXspeed3max.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Speed"][0] = double.Parse(GantryYspeed3min.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Speed"][1] = double.Parse(GantryYspeed3max.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Speed"][0] = double.Parse(GantryZspeed3min.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Speed"][1] = double.Parse(GantryZspeed3max.Text);
            ShareValues.AxisSpeedMin[12] = double.Parse(Beltspeed3min.Text);
            ShareValues.AxisSpeedMax[12] = double.Parse(Beltspeed3max.Text);
            ShareValues.AxisSpeedMin[13] = double.Parse(Liftspeed3min.Text);
            ShareValues.AxisSpeedMax[13] = double.Parse(Liftspeed3max.Text);
            ShareValues.AxisSpeedMin[14] = double.Parse(GantryXspeed3min.Text);
            ShareValues.AxisSpeedMax[14] = double.Parse(GantryXspeed3max.Text);
            ShareValues.AxisSpeedMin[15] = double.Parse(GantryYspeed3min.Text);
            ShareValues.AxisSpeedMax[15] = double.Parse(GantryYspeed3max.Text);
            ShareValues.AxisSpeedMin[16] = double.Parse(GantryZspeed3min.Text);
            ShareValues.AxisSpeedMax[16] = double.Parse(GantryZspeed3max.Text);

            LimitJsonObject["RecheckArea"]["Belt"]["Acc"][0] = double.Parse(Beltacc3min.Text);
            LimitJsonObject["RecheckArea"]["Belt"]["Acc"][1] = double.Parse(Beltacc3max.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Acc"][0] = double.Parse(Liftacc3min.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Acc"][1] = double.Parse(Liftacc3max.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Acc"][0] = double.Parse(GantryXacc3min.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Acc"][1] = double.Parse(GantryXacc3max.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Acc"][0] = double.Parse(GantryYacc3min.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Acc"][1] = double.Parse(GantryYacc3max.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Acc"][0] = double.Parse(GantryZacc3min.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Acc"][1] = double.Parse(GantryZacc3max.Text);
            ShareValues.AxisAccMin[12] = double.Parse(Beltacc3min.Text);
            ShareValues.AxisAccMax[12] = double.Parse(Beltacc3max.Text);
            ShareValues.AxisAccMin[13] = double.Parse(Liftacc3min.Text);
            ShareValues.AxisAccMax[13] = double.Parse(Liftacc3max.Text);
            ShareValues.AxisAccMin[14] = double.Parse(GantryXacc3min.Text);
            ShareValues.AxisAccMax[14] = double.Parse(GantryXacc3max.Text);
            ShareValues.AxisAccMin[15] = double.Parse(GantryYacc3min.Text);
            ShareValues.AxisAccMax[15] = double.Parse(GantryYacc3max.Text);
            ShareValues.AxisAccMin[16] = double.Parse(GantryZacc3min.Text);
            ShareValues.AxisAccMax[16] = double.Parse(GantryZacc3max.Text);

            LimitJsonObject["RecheckArea"]["Belt"]["Dec"][0] = double.Parse(Beltdec3min.Text);
            LimitJsonObject["RecheckArea"]["Belt"]["Dec"][1] = double.Parse(Beltdec3max.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Dec"][0] = double.Parse(Liftdec3min.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Dec"][1] = double.Parse(Liftdec3max.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Dec"][0] = double.Parse(GantryXdec3min.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Dec"][1] = double.Parse(GantryXdec3max.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Dec"][0] = double.Parse(GantryYdec3min.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Dec"][1] = double.Parse(GantryYdec3max.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Dec"][0] = double.Parse(GantryZdec3min.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Dec"][1] = double.Parse(GantryZdec3max.Text);
            ShareValues.AxisDecMin[12] = double.Parse(Beltdec3min.Text);
            ShareValues.AxisDecMax[12] = double.Parse(Beltdec3max.Text);
            ShareValues.AxisDecMin[13] = double.Parse(Liftdec3min.Text);
            ShareValues.AxisDecMax[13] = double.Parse(Liftdec3max.Text);
            ShareValues.AxisDecMin[14] = double.Parse(GantryXdec3min.Text);
            ShareValues.AxisDecMax[14] = double.Parse(GantryXdec3max.Text);
            ShareValues.AxisDecMin[15] = double.Parse(GantryYdec3min.Text);
            ShareValues.AxisDecMax[15] = double.Parse(GantryYdec3max.Text);
            ShareValues.AxisDecMin[16] = double.Parse(GantryZdec3min.Text);
            ShareValues.AxisDecMax[16] = double.Parse(GantryZdec3max.Text);

            LimitJsonObject["RecheckArea"]["Belt"]["Axis"][0] = double.Parse(Beltaxis3min.Text);
            LimitJsonObject["RecheckArea"]["Belt"]["Axis"][1] = double.Parse(Beltaxis3max.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Axis"][0] = double.Parse(Liftaxis3min.Text);
            LimitJsonObject["RecheckArea"]["Lift"]["Axis"][1] = double.Parse(Liftaxis3max.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Axis"][0] = double.Parse(GantryXaxis3min.Text);
            LimitJsonObject["RecheckArea"]["GantryX"]["Axis"][1] = double.Parse(GantryXaxis3max.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Axis"][0] = double.Parse(GantryYaxis3min.Text);
            LimitJsonObject["RecheckArea"]["GantryY"]["Axis"][1] = double.Parse(GantryYaxis3max.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Axis"][0] = double.Parse(GantryZaxis3min.Text);
            LimitJsonObject["RecheckArea"]["GantryZ"]["Axis"][1] = double.Parse(GantryZaxis3max.Text);
            ShareValues.AxisMin[12] = double.Parse(Beltaxis3min.Text);
            ShareValues.AxisMax[12] = double.Parse(Beltaxis3max.Text);
            ShareValues.AxisMin[13] = double.Parse(Liftaxis3min.Text);
            ShareValues.AxisMax[13] = double.Parse(Liftaxis3max.Text);
            ShareValues.AxisMin[14] = double.Parse(GantryXaxis3min.Text);
            ShareValues.AxisMax[14] = double.Parse(GantryXaxis3max.Text);
            ShareValues.AxisMin[15] = double.Parse(GantryYaxis3min.Text);
            ShareValues.AxisMax[15] = double.Parse(GantryYaxis3max.Text);
            ShareValues.AxisMin[16] = double.Parse(GantryZaxis3min.Text);
            ShareValues.AxisMax[16] = double.Parse(GantryZaxis3max.Text);

            string strSrc = Convert.ToString(LimitJsonObject);//将json装换为string
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\Limit.json", strSrc, System.Text.Encoding.UTF8);
        }
        private void workstation1(Rectangle rect1)
        {
            Task task1;
            task1 = new Task(() => moveforward(rect1, 47, 96, 20));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => returnOK(rect1));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => returnOK(this.rect2));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => movebackward(this.rect3, 170, 140, 20));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => returnOK(this.rect3));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => movebackward(this.rect3, 140, 135, 50));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => returnOK(this.rect3));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => movebackward(this.rect3, 135, 130, 50));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => returnOK(this.rect3));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => movebackward(this.rect3, 130, 125, 50));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => returnOK(this.rect3));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => moveforward(this.rect3, 125, 170, 20));
            task1.Start();
            task1.Wait();
        }

        private void workstation2(Rectangle rectangle)
        {
            Task task1;
            Task task2;
            Task task3;
            //station 2
            task1 = new Task(() => moveforward(rectangle, 96, 270, 10));
            task2 = new Task(() => moveforward(this.rect4, 283, 396, 20));
            task2.Start();
            task1.Start();
            Task.WaitAll(task1, task2);
            task1 = new Task(() => returnOK(this.rect1));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 396, 486, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 486, 441, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect5));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 441, 410, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movedown(this.rect4, 74, 127, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 410, 339, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect6));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveup(this.rect4, 127, 74, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 339, 300, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 300, 240, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movedown(this.rect4, 74, 96, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 240, 250, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 250, 260, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 260, 270, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 270, 280, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 280, 290, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 290, 300, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();

            task1 = new Task(() => moveup(this.rect4, 96, 74, 20));
            task1.Start();
            task1 = new Task(() => moveforward(this.rect4, 300, 396, 20));
            task1.Start();
            Task.WaitAll(task1, task2);

            task1 = new Task(() => moveforward(this.rect4, 396, 486, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 486, 441, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect5));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 441, 410, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movedown(this.rect4, 74, 127, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 410, 339, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect6));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveup(this.rect4, 127, 96, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect4, 339, 240, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 240, 250, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 250, 260, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 260, 270, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 270, 280, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 280, 290, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect4, 290, 300, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect4));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveup(this.rect4, 96, 74, 20));
            task2 = new Task(() => movebackward(this.rect4, 300, 283, 20));
            task2.Start();
            task1.Start();
            Task.WaitAll(task1, task2);
        }

        private void workstation3(Rectangle rectangle)
        {
            Task task1;
            Task task2;
            Task task3;
            //station 3
            task1 = new Task(() => moveforward(rectangle, 270, 620, 10));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(rectangle));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movedown(this.rect7, 74, 96, 20));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect7, 584, 594, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect7, 594, 604, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect7, 604, 614, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect7, 614, 624, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect7, 624, 634, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(this.rect7, 634, 644, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => movebackward(this.rect7, 644, 584, 50));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => returnOK(this.rect7));
            task1.Start();
            task1.Wait();
            task1 = new Task(() => moveforward(rectangle, 620, 688, 10));
            task2 = new Task(() => moveup(this.rect7, 96, 74, 20));
            task2.Start();
            task1.Start();
            Task.WaitAll(task1, task2);
        }

        private int first = 1;
        private int station1Init = 0;
        private int station2Init = 0;
        private int station3Init = 0;
        private int station4Init = 0;
        private int station1Finished = 1;
        private int station2Finished = 1;
        private int station3Finished = 1;
        private int station4Finished = 1;

        //private void Station1Act()
        //{
        //    while (true)
        //    {
        //        if (station1Init == 1)
        //        {
        //            this.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                this.rect1.Visibility = Visibility.Visible;
        //            }));
        //            station1Finished = 0;
        //            workstation1(this.rect1);
        //            station1Finished = 1;
        //            if (station2Finished != 0)
        //            {
        //                this.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    this.rect1.Visibility = Visibility.Hidden;
        //                }));
        //                station2Init = 1;
        //                Thread.Sleep(10);
        //            }
        //            else
        //            {
        //                station2Init = 0;
        //                station1Init = 0;
        //            }
        //        }
        //    }
        //}

        //private void Station2Act()
        //{
        //    while (true)
        //    {
        //        if (station2Init == 1)
        //        {
        //            station1Init = 1;
        //            this.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                this.rect11.Visibility = Visibility.Visible;
        //            }));
        //            station2Init = 0;
        //            station2Finished = 0;
        //            workstation2(this.rect11);
        //            station2Finished = 1;
        //            if (station3Finished != 0)
        //            {
        //                this.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    this.rect11.Visibility = Visibility.Hidden;
        //                }));
        //                station3Init = 1;
        //                Thread.Sleep(10);
        //            }
        //            else
        //            {
        //                station3Init = 0;
        //                station2Init = 0;
        //            }
        //        }
        //    }
        //}

        //private void Station3Act()
        //{
        //    while (true)
        //    {
        //        if (station3Init == 1)
        //        {
        //            station2Init = 1;
        //            this.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                this.rect12.Visibility = Visibility.Visible;
        //            }));
        //            station3Init = 0;
        //            station3Finished = 0;
        //            workstation3(this.rect12);
        //            station3Finished = 1;
        //            this.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                this.rect12.Visibility = Visibility.Hidden;
        //            }));
        //            if (station2Finished == 1)
        //            {
        //                station3Init = 1;
        //            }
        //        }
        //    }
        //}

        private void moveforward(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft > endpos)
                    break;
            }
        }

        private void movebackward(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft -= 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft < endpos)
                    break;
            }
        }

        private void movedown(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            while (true)
            {
                mtop += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(rect.Margin.Left, mtop, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mtop > endpos)
                    break;
            }
        }

        private void moveup(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            while (true)
            {
                mtop -= 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(rect.Margin.Left, mtop, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mtop < endpos)
                    break;
            }
        }

        private int flag = 1;

        private void returnOK(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag == 1) {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag1 = 1;

        private void returnOK1(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag1 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag2 = 1;

        private void returnOK2(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag2 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag3 = 1;

        private void returnOK3(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag3 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag4 = 1;

        private void returnOK4(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag4 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag5 = 1;

        private void returnOK5(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag5 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private void returnNG(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
            }));
            Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            //station1Init = 1;
            //Task task1 = new Task(Station1Act);
            //task1.Start();
            //Task task2 = new Task(Station2Act);
            //task2.Start();
            //Task task3 = new Task(Station3Act);
            //task3.Start();
        }

        int deltatime = 0;
        private void wait() {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                deltatime = 999999;
            }

            while (true)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = deltatime - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }
        }

        private int module_Num = 12;

        private void LailiaoAct(Rectangle rect)
        {
            //pallet in
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in";
            }));
            moveforward(rect, 10, 43, 20);
            wait();
            //trigger jiansu IO
            flag = 1;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.LaiLiao_JianSu] = true;
            Task task1 = new Task(() => returnOK(rect20));
            task1.Start();
            wait();
            //send dingqi IO
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            flag1 = 1;
            Task task2 = new Task(() => returnOK1(rect21));
            task2.Start();
            wait();
            moveforward(rect, 43, 94, 20);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //scan
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "start scan pallet";
            }));
            flag2 = 1;
            Task task3 = new Task(() => returnOK2(rect22));
            task3.Start();
            Thread.Sleep(3000);
            wait();
            flag2 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "scan pallet finished";
            }));
            Thread.Sleep(1000);
            //pallet lift up
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            flag3 = 1;
            Task task4 = new Task(() => returnOK3(rect));
            task4.Start();
            Thread.Sleep(1000);
            wait();
            //laser measure
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "start laser measure";
            }));
            for(int i = 0; i < module_Num; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action5.Content = $"laser go to module{i+1} position{j+1} and trigger laser";
                    }));
                    flag2 = 1;
                    task3 = new Task(() => returnOK2(rect25));
                    task3.Start();
                    Thread.Sleep(1000);
                    wait();
                    flag2 = 0;
                }
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "laser measure finished";
            }));
            Thread.Sleep(1000);
            wait();
            flag3 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            flag1 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();

        }

        public int GenerateRandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private int current_Assembled = 0;
        private int Picker_FOAM_Count = 0;
        private int BadFoamCount = 0;
        private int has_XueWeiXinXi = 0;
        private int NG_Foam_Count = 0;
        public int currentx, currenty;

        private void  ZuzhuangAct(Rectangle rect)
        {
            Task task1, task2, task3, task4, task5, task6, task7, task8;
            double xpos, ypos;
            //move to assembly
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            moveforward(rect, 94, 112, 20);
            flag = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.LaiLiao_JianSu] = false;
            wait();
            moveforward(rect, 112, 230, 20);
            wait();
            //trigger jiansu IO
            flag = 1;
            task1 = new Task(() => returnOK(rect23));
            task1.Start();
            //send dingqi IO
            flag1 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK1(rect24));
            task2.Start();
            wait();
            moveforward(rect, 230, 266, 20);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //pallet lift up
            flag4 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            task4 = new Task(() => returnOK4(rect));
            task4.Start();
            wait();
            current_Assembled = 0;
            //如果吸嘴上有料，直接跳去CCD2精定位
            if (Picker_FOAM_Count > 0)
            {
                goto step4;
            }
        step2:
            //飞达上拍料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to feeder";
            }));
            xpos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Left));
            ypos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Top));
            task3 = new Task(() => moveup(rect26, (int)ypos, 57, 20));
            task3.Start();
            task5 = new Task(() => movebackward(rect26, (int)xpos, 229, 20));
            task5.Start();
            Task.WaitAll(task3, task5);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Thread.Sleep(1000);
            wait();
        step3:
            //吸嘴取料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "pick foams";
            }));
            flag5 = 1;
            task3 = new Task(() => returnOK5(rect26));
            task3.Start();
            Thread.Sleep(1000);
            wait();
        step4:
            //CCD2 精定位
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to CCD2";
            }));
            xpos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Left));
            ypos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Top));
            if (ypos < 116) {
                task6 = new Task(() => movedown(rect26, (int)ypos, 116, 20));
                task6.Start();
            }
            else
            {
                task6 = new Task(() => moveup(rect26, (int)ypos, 116, 20));
                task6.Start();
            }
            if (xpos < 239)
            {
                task5 = new Task(() => moveforward(rect26, (int)xpos, 239, 20));
                task5.Start();
            }
            else
            {
                task5 = new Task(() => movebackward(rect26, (int)xpos, 239, 20));
                task5.Start();
            }
            Task.WaitAll(task6, task5);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Thread.Sleep(1000);
            wait();
            if(Picker_FOAM_Count == 0)
            {
                BadFoamCount = GenerateRandomNumber(0, 2);
                Picker_FOAM_Count = 4 - BadFoamCount;
                if (BadFoamCount > 0)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action4.Content = $"{BadFoamCount} foam(s) is NG";
                    }));
                    goto step5;
                }
                else
                {
                    goto step6;
                }
            }
            else {
                goto step6;
            }
        step5:
            //如果有坏料，放到坏料盒里
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to CCD2";
            }));
            task5 = new Task(() => movebackward(rect26, 239, 182, 20));
            task5.Start();
            Task.WaitAll(task5);
            wait();
            for (int i = 0; i < BadFoamCount; ++i)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action4.Content = $"throw NG faom {i+1}";
                }));
                Thread.Sleep(1000);
                wait();
                NG_Foam_Count++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    NGnum.Content = $"{NG_Foam_Count}";
                }));
            }
        step6:
            //拍料盘
            if (has_XueWeiXinXi == 1) {
                goto step7;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to pallet";
            }));
            xpos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Left));
            ypos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Top));
            task6 = new Task(() => movedown(rect26, (int)ypos, 198, 20));
            task6.Start();
            task5 = new Task(() => moveforward(rect26, (int)xpos, 276, 20));
            task5.Start();
            Task.WaitAll(task6, task5);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            has_XueWeiXinXi = 1;
            Thread.Sleep(1000);
            wait();
        step7:
            //放料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to pallet";
            }));
            xpos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Left));
            ypos = (double)rect26.Dispatcher.Invoke(new Func<double>(() => rect26.Margin.Top));
            task6 = new Task(() => movedown(rect26, (int)ypos, 198, 20));
            task6.Start();
            task5 = new Task(() => moveforward(rect26, (int)xpos, 276, 20));
            task5.Start();
            Task.WaitAll(task6, task5);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "place faoms";
            }));
            int cnt = 0;
            while (Picker_FOAM_Count > 0)
            {
                Thread.Sleep(1000);
                wait();
                cnt++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action4.Content = $"place faom {cnt}";
                }));
                Picker_FOAM_Count--;
                current_Assembled++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    palletfaomnum.Content = $"{current_Assembled} ";
                }));
                if (current_Assembled >= module_Num)
                {
                    break;
                }
            }
            if(current_Assembled < module_Num)
            {
                flag5 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = "picker has no foams ";
                }));
                Thread.Sleep(1000);
                wait();
                goto step2;
            }
            if (Picker_FOAM_Count == 0) {
                flag5 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = "picker has no foams ";
                }));
                Thread.Sleep(1000);
                wait();
            }
            current_Assembled = 0;
            has_XueWeiXinXi = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                palletfaomnum.Content = " ";
            }));
            flag4 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            flag1 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();
        }

        private int Left_Foam_Count = 0;
        private int Fujian_OK = 0;
        private void FujianAct(Rectangle rect)
        {
            Task task1, task2, task3, task4, task5, task6, task7, task8;
            double xpos, ypos;
            //move to next
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            moveforward(rect, 266, 303, 20);
            flag = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.ZuZhuang_JianSu] = false;
            wait();
            moveforward(rect, 303, 410, 20);
            wait();
            //trigger jiansu IO
            flag = 1;
            task1 = new Task(() => returnOK(rect28));
            task1.Start();
            //send dingqi IO
            flag1 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK1(rect27));
            task2.Start();
            wait();
            moveforward(rect, 410, 448, 20);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //pallet lift up
            flag4 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            task4 = new Task(() => returnOK4(rect));
            task4.Start();
            wait();
            Left_Foam_Count = module_Num;
        step2:
            //撕膜
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "start to tear film";
            }));
            for (int i = 0; i < module_Num; ++i)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = $"robot go to module{i + 1} and tear film";
                }));
                flag2 = 1;
                task3 = new Task(() => returnOK2(rect29));
                task3.Start();
                Thread.Sleep(1000);
                wait();
                Left_Foam_Count--;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Leftfoam.Content = $"{Left_Foam_Count}";
                }));
                flag2 = 0;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "tear film finished";
            }));
            Thread.Sleep(1000);
            wait();
        step3:
            //CCD3复检
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Fujian_OK = GenerateRandomNumber(0, 1);
            Thread.Sleep(1000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Leftfoam.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            flag1 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();
        }

        private void RejectAct(Rectangle rect)
        {
            Task task1, task2, task3, task4, task5, task6, task7, task8;
            double xpos, ypos;
            //move to next
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            moveforward(rect, 448, 480, 20);
            flag = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] = false;
            wait();
            moveforward(rect, 480, 620, 20);
            wait();
            //trigger jiansu IO
            flag = 1;
            task1 = new Task(() => returnOK(rect31));
            task1.Start();
            //send dingqi IO
            flag1 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK1(rect30));
            task2.Start();
            wait();
            moveforward(rect, 620, 653, 20);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
        step2:
            if(Fujian_OK == 0)
            {
                //NG顶升
                flag4 = 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "NG pallet lift up";
                }));
                task4 = new Task(() => returnOK4(rect));
                task4.Start();
                Thread.Sleep(1000);
                wait();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "NG pallet remove";
                }));
                Thread.Sleep(3000);
                flag1 = 0;
                flag = 0;
            }
            else
            {
                flag1 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action2.Content = "send cylinder IO off";
                }));
                Thread.Sleep(1000);
                wait();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet go out";
                }));
                moveforward(rect, 653, 689, 20);
                flag = 0;
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                wait();
                moveforward(rect, 689, 702, 20);
                wait();
            }

        }
        public static void CopyProperties(object source, object destination)
        {
            var sourceProps = source.GetType().GetProperties();
            foreach (var prop in sourceProps)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(destination, prop.GetValue(source));
                }
            }
        }
        private void Station1Act()
        {
            while (true)
            {
                if (station1Init == 1)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect10.Visibility = Visibility.Visible;
                    }));
                    station1Finished = 0;
                    LailiaoAct(this.rect10);
                    station1Finished = 1;
                    if (station2Finished != 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //this.rect10.Visibility = Visibility.Hidden;
                            CopyProperties(rect10, rect32);
                        }));
                        station2Init = 1;
                        Thread.Sleep(10);
                    }
                    else
                    {
                        station2Init = 0;
                        station1Init = 0;
                    }
                }
            }
        }

        private void Station2Act()
        {
            while (true)
            {
                if (station2Init == 1)
                {
                    station1Init = 1;
                    //this.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    this.rect32.Visibility = Visibility.Visible;
                    //}));
                    station2Init = 0;
                    station2Finished = 0;
                    ZuzhuangAct(this.rect32);
                    station2Finished = 1;
                    if (station3Finished != 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //this.rect32.Visibility = Visibility.Hidden;
                            CopyProperties(rect32, rect33);
                        }));
                        station3Init = 1;
                        Thread.Sleep(10);
                    }
                    else
                    {
                        station3Init = 0;
                        station2Init = 0;
                    }
                }
            }
        }

        private void Station3Act()
        {
            while (true)
            {
                if (station3Init == 1)
                {
                    station2Init = 1;
                    //this.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    this.rect33.Visibility = Visibility.Visible;
                    //}));
                    station3Init = 0;
                    station3Finished = 0;
                    FujianAct(this.rect33);
                    station3Finished = 1;
                    if (station4Finished != 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (station2Finished == 0)
                            {
                                CopyProperties(rect33, rect34);
                                this.rect33.Visibility = Visibility.Hidden;
                            }
                            else {
                                this.rect33.Visibility = Visibility.Visible;
                                CopyProperties(rect33, rect34);
                            }
                        }));
                        station4Init = 1;
                        Thread.Sleep(10);
                    }
                    else
                    {
                        station4Init = 0;
                        station3Init = 0;
                    }
                }
            }
        }

        private void Station4Act()
        {
            while (true)
            {
                if (station4Init == 1)
                {
                    //station3Init = 1;
                    //this.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    this.rect34.Visibility = Visibility.Visible;
                    //}));
                    station4Init = 0;
                    station4Finished = 0;
                    RejectAct(this.rect34);
                    station4Finished = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect34.Visibility = Visibility.Hidden;
                    }));
                }
            }
        }

        private void wholeprocess() {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.rect10.Visibility = Visibility.Visible;
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = " ";
            }));
            while (true)
            {
                LailiaoAct(this.rect10);
                ZuzhuangAct(this.rect10);
                FujianAct(this.rect10);
                RejectAct(this.rect10);

            }
        }

        private void start1_Click(object sender, RoutedEventArgs e)
        {
            flag = 0;
            flag1 = 0;
            flag2 = 0;
            flag3 = 0;
            flag4 = 0;
            NG_Foam_Count = 0;
            station1Init = 1;
            Task task1 = new Task(Station1Act);
            task1.Start();
            Task task2 = new Task(Station2Act);
            task2.Start();
            Task task3 = new Task(Station3Act);
            task3.Start();
            Task task4 = new Task(Station4Act);
            task4.Start();

            //Task task1 = new Task(wholeprocess);
            //task1.Start();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            deltatime = 999999;
        }

        private void resume_Click(object sender, RoutedEventArgs e)
        {
            deltatime = 0;
        }
    }
}
