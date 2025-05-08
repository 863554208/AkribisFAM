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
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;
using System.Diagnostics.Eventing.Reader;

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
        public ParameterConfigViewModel ViewModel { get; }

        public ParameterConfig()
        {
            InitializeComponent();
            ReadLimitJson();

            ViewModel = new ParameterConfigViewModel();
            this.DataContext = ViewModel;

        }

        private void Current_OnZuZhuangExecuted_4()
        {
            throw new NotImplementedException();
        }

        private void UpdateRectangleColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateRectangleColorToGreen());
        }

        private void UpdateLaserRectangleColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLaserRectangleColorToGreen());
        }

        private async void UpdateRectanglePosition()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            //Dispatcher.Invoke(() => ViewModel.UpdateRectanglePosition(378,700));

            Thickness rect1Thickness = new Thickness();

            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                rect1Thickness.Left = this.rect1.Margin.Left;
                rect1Thickness.Top = this.rect1.Margin.Top;
                rect1Thickness.Right = this.rect1.Margin.Right;
                rect1Thickness.Bottom = this.rect1.Margin.Bottom;
            });

            while (rect1Thickness.Left <= 350)
            {
                // 更新 UI 元素时，需确保在 UI 线程上执行
                await this.Dispatcher.InvokeAsync(() =>
                {
                    rect1Thickness.Left += 5;
                    this.rect1.Margin = rect1Thickness;
                });

                // 延时以缓解 UI 刷新压力
                await Task.Delay(10);
            }
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

        public async void UpdateMovement()
        {
            Thickness rect1Thickness = new Thickness();

            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                rect1Thickness.Left = this.rect1.Margin.Left;
                rect1Thickness.Top = this.rect1.Margin.Top;
                rect1Thickness.Right = this.rect1.Margin.Right;
                rect1Thickness.Bottom = this.rect1.Margin.Bottom;
            });

            // 在异步上下文中更新位置
            while (rect1Thickness.Left <= 96)
            {
                // 更新 UI 元素时，需确保在 UI 线程上执行
                await this.Dispatcher.InvokeAsync(() =>
                {
                    rect1Thickness.Left += 5;
                    this.rect1.Margin = rect1Thickness;
                });

                // 延时以缓解 UI 刷新压力
                await Task.Delay(10);
            }
        }

        public async void UpdateCCDMovement_1()
        {
            GlobalManager.Current.Feedar1Captured = false;
            Thickness CCD1Thickness = new Thickness();
            double totalDistance = 0;
            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                CCD1Thickness.Left = this.CCD1.Margin.Left;
                CCD1Thickness.Top = this.CCD1.Margin.Top;
                CCD1Thickness.Right = this.CCD1.Margin.Right;
                CCD1Thickness.Bottom = this.CCD1.Margin.Bottom;
            });


            // 在异步上下文中更新位置
            while (true)
            {
                // 等待一段时间
                await Task.Delay(10);

                // 检查是否达到总平移距离大于50
                if (totalDistance == 40)
                {
                    break;
                }

                // 更新 UI 元素时，需确保在 UI 线程上执行
                await this.Dispatcher.InvokeAsync(() =>
                {
                    // 平移CCD1
                    CCD1Thickness.Left += 2;
                    totalDistance += 2;
                    this.CCD1.Margin = CCD1Thickness;
                });
            }

            GlobalManager.Current.Feedar1Captured = true;
        }

        public async void UpdateCCDMovement_2()
        {
            GlobalManager.Current.CCD1InPosition = false;
            Thickness CCD1Thickness = new Thickness();
            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                CCD1Thickness.Left = this.CCD1.Margin.Left;
                CCD1Thickness.Top = this.CCD1.Margin.Top;
                CCD1Thickness.Right = this.CCD1.Margin.Right;
                CCD1Thickness.Bottom = this.CCD1.Margin.Bottom;
            });

            Console.WriteLine("sad: " + Math.Abs(CCD1Thickness.Left));
            Console.WriteLine("sad_top: " + Math.Abs(CCD1Thickness.Top));
            while (true)
            {
                await Task.Delay(10);

                if ((CCD1Thickness.Left==290) && CCD1Thickness.Top==220) 
                {
                    break;
                }

                await this.Dispatcher.InvokeAsync(() =>
                {                    
                    if (CCD1Thickness.Left <= 287)
                    {
                        CCD1Thickness.Left += 2;
                    }
                    else if (CCD1Thickness.Left >= 293)
                    {
                        CCD1Thickness.Left -= 2;
                    }
                    else
                    {
                        CCD1Thickness.Left = 290;
                    }

                    if (CCD1Thickness.Top <= 217)
                    {
                        CCD1Thickness.Top += 2;

                    }
                    else if (CCD1Thickness.Top >= 223)
                    {
                        CCD1Thickness.Top -= 2;

                    }
                    else
                    {
                        CCD1Thickness.Top = 220;
                    }
                    this.CCD1.Margin = CCD1Thickness;
                });
            }

                GlobalManager.Current.CCD1InPosition = true;
        }


        //把吸嘴移动到CCD2上方进行拍照
        public async void UpdateCCDMovement_3()
        {
            GlobalManager.Current.CCD2Captured = false;
            Thickness CCD1Thickness = new Thickness();
            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                CCD1Thickness.Left = this.CCD1.Margin.Left;
                CCD1Thickness.Top = this.CCD1.Margin.Top;
                CCD1Thickness.Right = this.CCD1.Margin.Right;
                CCD1Thickness.Bottom = this.CCD1.Margin.Bottom;
            });

            while (true)
            {
                await Task.Delay(10);

                if ((CCD1Thickness.Left == 375) && CCD1Thickness.Top == 180)
                {
                    break;
                }

                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (CCD1Thickness.Left <= 373)
                    {
                        CCD1Thickness.Left += 2;
                    }
                    else if (CCD1Thickness.Left >= 378)
                    {
                        CCD1Thickness.Left -= 2;
                    }
                    else
                    {
                        CCD1Thickness.Left = 375;
                    }

                    if (CCD1Thickness.Top <= 177)
                    {
                        CCD1Thickness.Top += 2;

                    }
                    else if (CCD1Thickness.Top >= 183)
                    {
                        CCD1Thickness.Top -= 2;

                    }
                    else
                    {
                        CCD1Thickness.Top = 180;
                    }
                    this.CCD1.Margin = CCD1Thickness;
                });
            }

            GlobalManager.Current.CCD2Captured = true;
        }

        //把吸嘴移动到料盘上面
        public async void UpdateCCDMovement_4()
        {
            GlobalManager.Current.MoveToLiaopan = false;
            Thickness CCD1Thickness = new Thickness();
            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                CCD1Thickness.Left = this.CCD1.Margin.Left;
                CCD1Thickness.Top = this.CCD1.Margin.Top;
                CCD1Thickness.Right = this.CCD1.Margin.Right;
                CCD1Thickness.Bottom = this.CCD1.Margin.Bottom;
            });

            while (true)
            {
                await Task.Delay(10);

                if ((CCD1Thickness.Left == 360) && CCD1Thickness.Top == 120)
                {
                    break;
                }

                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (CCD1Thickness.Left <= 358)
                    {
                        CCD1Thickness.Left += 2;
                    }
                    else if (CCD1Thickness.Left >= 363)
                    {
                        CCD1Thickness.Left -= 2;
                    }
                    else
                    {
                        CCD1Thickness.Left = 360;
                    }

                    if (CCD1Thickness.Top <= 117)
                    {
                        CCD1Thickness.Top += 2;

                    }
                    else if (CCD1Thickness.Top >= 123)
                    {
                        CCD1Thickness.Top -= 2;

                    }
                    else
                    {
                        CCD1Thickness.Top = 120;
                    }
                    this.CCD1.Margin = CCD1Thickness;
                });
            }

            GlobalManager.Current.MoveToLiaopan = true;
        }

        //对料盘进行飞拍
        public async void UpdateCCDMovement_5()
        {
            GlobalManager.Current.GrabLiaoPan = false;
            GlobalManager.Current.has_XueWeiXinXi = false;
            Thickness CCD1Thickness = new Thickness();
            double totalDistance = 0;
            // 使用 Dispatcher 确保在 UI 线程上获取初始 Margin
            await this.Dispatcher.InvokeAsync(() =>
            {
                CCD1Thickness.Left = this.CCD1.Margin.Left;
                CCD1Thickness.Top = this.CCD1.Margin.Top;
                CCD1Thickness.Right = this.CCD1.Margin.Right;
                CCD1Thickness.Bottom = this.CCD1.Margin.Bottom;
            });


            // 在异步上下文中更新位置
            while (true)
            {
                // 等待一段时间
                await Task.Delay(10);

                // 检查是否达到总平移距离大于50
                if (totalDistance == 40)
                {
                    break;
                }

                // 更新 UI 元素时，需确保在 UI 线程上执行
                await this.Dispatcher.InvokeAsync(() =>
                {
                    // 平移CCD1
                    CCD1Thickness.Left += 2;
                    totalDistance += 2;
                    this.CCD1.Margin = CCD1Thickness;
                });
            }

            GlobalManager.Current.has_XueWeiXinXi = true;
            GlobalManager.Current.GrabLiaoPan = true;
        }


        private void updateMargin(int x)
        {
            this.Dispatcher.Invoke(() =>
            {
                rect1.Margin = new Thickness(x, rect1.Margin.Top, 0, 0);
            });
        }
        private void start_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.Current.lailiao_ChuFaJinBan = true;
            UpdateMovement();

        }

    }
}
