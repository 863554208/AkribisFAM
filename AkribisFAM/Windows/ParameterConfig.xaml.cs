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
using WpfApp1;

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

        public ParameterConfig()
        {
            InitializeComponent();
        }

        private void DoubleText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(new Regex(@"^\d*\.?\d*$").IsMatch(e.Text));
        }

        private void Speed_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if(textbox.Text == ""){
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.AXISMINSPEED)
                {
                    textbox.Text = ShareValues.AXISMINSPEED.ToString();
                }
                else if (result > ShareValues.AXISMAXSPEED)
                {
                    textbox.Text = ShareValues.AXISMAXSPEED.ToString();
                }
            }
        }

        private void PickerSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.PICKERAXISMINSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISMINSPEED.ToString();
                }
                else if (result > ShareValues.PICKERAXISMAXSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISMAXSPEED.ToString();
                }
            }
        }

        private void PickerRSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.PICKERAXISRMINSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISRMINSPEED.ToString();
                }
                else if (result > ShareValues.PICKERAXISRMAXSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISRMAXSPEED.ToString();
                }
            }
        }

        private void ACC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.AXISMINACC)
                {
                    textbox.Text = ShareValues.AXISMINACC.ToString();
                }
                else if (result > ShareValues.AXISMAXACC)
                {
                    textbox.Text = ShareValues.AXISMAXACC.ToString();
                }
            }
        }

        private void PickerACC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.PICKERAXISMINSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISMINSPEED.ToString();
                }
                else if (result > ShareValues.PICKERAXISMAXSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISMAXSPEED.ToString();
                }
            }
        }

        private void PickerRACC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.PICKERAXISRMINSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISRMINSPEED.ToString();
                }
                else if (result > ShareValues.PICKERAXISRMAXSPEED)
                {
                    textbox.Text = ShareValues.PICKERAXISRMAXSPEED.ToString();
                }
            }
        }

        private void DEC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.AXISMINDEC)
                {
                    textbox.Text = ShareValues.AXISMINDEC.ToString();
                }
                else if (result > ShareValues.AXISMAXDEC)
                {
                    textbox.Text = ShareValues.AXISMAXDEC.ToString();
                }
            }
        }

        private void PickerDEC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.PICKERAXISMINDEC)
                {
                    textbox.Text = ShareValues.PICKERAXISMINDEC.ToString();
                }
                else if (result > ShareValues.PICKERAXISMAXDEC)
                {
                    textbox.Text = ShareValues.PICKERAXISMAXDEC.ToString();
                }
            }
        }

        private void PickerRDEC_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "0";
            }
            double result;
            bool success = double.TryParse(textbox.Text, out result);
            if (success)
            {
                if (result < ShareValues.PICKERAXISRMINDEC)
                {
                    textbox.Text = ShareValues.PICKERAXISRMINDEC.ToString();
                }
                else if (result > ShareValues.PICKERAXISRMAXDEC)
                {
                    textbox.Text = ShareValues.PICKERAXISRMAXDEC.ToString();
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

        }
    }
}
