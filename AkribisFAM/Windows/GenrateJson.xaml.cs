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
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace WpfApp1
{
    /// <summary>
    /// GenrateJson.xaml 的交互逻辑
    /// </summary>
    public partial class GenrateJson : Window
    {
        SetPoint setpointwindow;
        public JObject jsonObject;
        ScanningPoint deltaPoint;
        public string jsonname;
        public PointsType jsontype;

        public GenrateJson()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void Addmodulepoints_Click(object sender, RoutedEventArgs e)
        {
            int pointsnum;
            setpointwindow = new SetPoint();
            bool success = int.TryParse(Pointsnumtext.Text, out pointsnum);
            if (success)
            {
                if (pointsnum > 0 && pointsnum <= ShareValues.MAXPOINTSNUM)
                {
                    for (int k = 0; k < pointsnum; k++)
                    {
                        ScanningPoint spoint = new ScanningPoint();
                        spoint.x = 0.0;
                        spoint.y = 0.0;
                        spoint.z = 0.0;
                        setpointwindow.scanningpointlist.Add(spoint);
                    }
                    setpointwindow.pointsnum = pointsnum;
                    setpointwindow.type = jsontype;
                    setpointwindow.SetComponents();
                    setpointwindow.ShowDialog();
                }
            }
        }

        //A-B
        private ScanningPoint SubScanningPoint(ScanningPoint A, ScanningPoint B)
        {
            ScanningPoint C;
            C.x = A.x - B.x;
            C.y = A.y - B.y;
            C.z = A.z - B.z;
            return C;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            JObject jsObj = new JObject();
            int PalletID;
            bool success = int.TryParse(PalletIDtext.Text, out PalletID);
            if (success)
            {
                jsObj.Add("PalletID", PalletID);
            }
            else
            {
                return;
            }
            int PalletW;
            success = int.TryParse(PalletWtext.Text, out PalletW);
            if (success)
            {
                jsObj.Add("PalletW", PalletW);
            }
            else
            {
                return;
            }
            int PalletH;
            success = int.TryParse(PalletHtext.Text, out PalletH);
            if (success)
            {
                jsObj.Add("PalletH", PalletH);
            }
            else
            {
                return;
            }
            int PointsNum;
            success = int.TryParse(Pointsnumtext.Text, out PointsNum);
            if (success)
            {
                ScanningPoint Point1;
                jsObj.Add("PointsNum", PointsNum);
                try {
                    Point1.x = double.Parse(X1text.Text);
                    Point1.y = double.Parse(Y1text.Text);
                    Point1.z = double.Parse(Z1text.Text);
                    double X2 = double.Parse(X2text.Text);
                    double Y2 = double.Parse(Y2text.Text);
                    double Z2 = double.Parse(Z2text.Text);
                    double X3 = double.Parse(X3text.Text);
                    double Y3 = double.Parse(Y3text.Text);
                    double Z3 = double.Parse(Z3text.Text);
                    double deltaXW, deltaYW, deltaZW;
                    if (PalletW > 1)
                    {
                        deltaXW = (X2 - Point1.x) / (PalletW - 1);
                        deltaYW = (Y2 - Point1.y) / (PalletW - 1);
                        deltaZW = (Z2 - Point1.z) / (PalletW - 1);
                    }
                    else
                    {
                        deltaXW = 0;
                        deltaYW = 0;
                        deltaZW = 0;
                    }
                    double deltaXH, deltaYH, deltaZH;
                    if (PalletH > 1)
                    {
                        deltaXH = (X3 - Point1.x) / (PalletH - 1);
                        deltaYH = (Y3 - Point1.y) / (PalletH - 1);
                        deltaZH = (Z3 - Point1.z) / (PalletH - 1);
                    }
                    else
                    {
                        deltaXH = 0;
                        deltaYH = 0;
                        deltaZH = 0;
                    }

                    for (int i = 0; i < PalletH; i++)
                    {
                        for (int j = 0; j < PalletW; j++)
                        {
                            JObject jsModule = new JObject();
                            for (int k = 0; k < PointsNum; k++)
                            {
                                deltaPoint = SubScanningPoint(setpointwindow.scanningpointlist[k], Point1);
                                JObject jsPt = new JObject();
                                double X, Y, Z;
                                X = Point1.x + j * deltaXW + i * deltaXH + deltaPoint.x;
                                jsPt.Add("X", Math.Round(X, 3));
                                Y = Point1.y + j * deltaYW + i * deltaYH + deltaPoint.y;
                                jsPt.Add("Y", Math.Round(Y, 3));
                                Z = Point1.z + j * deltaZW + i * deltaZH + deltaPoint.z;
                                if(jsontype == PointsType.Feeder)
                                {
                                    jsPt.Add("R", Math.Round(Z, 3));
                                }
                                else
                                {
                                    jsPt.Add("Z", Math.Round(Z, 3));
                                }
                                jsModule.Add($"Point{k + 1}", jsPt);
                            }
                            jsObj.Add($"module{i * PalletW + j + 1}", jsModule);
                        }
                    }
                    string strSrc = Convert.ToString(jsObj);//将json装换为string
                    File.WriteAllText(Directory.GetCurrentDirectory() + "\\" + jsonname + ".json", strSrc, System.Text.Encoding.UTF8);//将内容写进json文件
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Parameters are not complete!", "Warning");
                    //参数不全
                }
            }
            else
            {
                return;
            }
        }

        private void DoubleText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(new Regex(@"^\d*\.?\d*$").IsMatch(e.Text));
        }

        private void IntText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Doubletext_TextChanged(object sender, TextChangedEventArgs e)
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
                if(jsontype == PointsType.Laser)
                {
                    if (textbox.Name.Substring(0, 1) == "X")
                    {
                        if (result < ShareValues.STATION1_AXISXMIN)
                        {
                            textbox.Text = ShareValues.STATION1_AXISXMIN.ToString();
                        }
                        else if (result > ShareValues.STATION1_AXISXMAX)
                        {
                            textbox.Text = ShareValues.STATION1_AXISXMAX.ToString();
                        }
                    }
                    else if(textbox.Name.Substring(0, 1) == "Y")
                    {
                        if (result < ShareValues.STATION1_AXISYMIN)
                        {
                            textbox.Text = ShareValues.STATION1_AXISYMIN.ToString();
                        }
                        else if (result > ShareValues.STATION1_AXISYMAX)
                        {
                            textbox.Text = ShareValues.STATION1_AXISYMAX.ToString();
                        }
                    }
                    else if (textbox.Name.Substring(0, 1) == "Z")
                    {
                        if (result < ShareValues.STATION1_AXISZMIN)
                        {
                            textbox.Text = ShareValues.STATION1_AXISZMIN.ToString();
                        }
                        else if (result > ShareValues.STATION1_AXISZMAX)
                        {
                            textbox.Text = ShareValues.STATION1_AXISZMAX.ToString();
                        }
                    }
                }
                else if(jsontype == PointsType.Feeder)
                {
                    if (textbox.Name.Substring(0, 1) == "X")
                    {
                        if (result < ShareValues.STATION2_AXISXMIN)
                        {
                            textbox.Text = ShareValues.STATION2_AXISXMIN.ToString();
                        }
                        else if (result > ShareValues.STATION2_AXISXMAX)
                        {
                            textbox.Text = ShareValues.STATION2_AXISXMAX.ToString();
                        }
                    }
                    else if (textbox.Name.Substring(0, 1) == "Y")
                    {
                        if (result < ShareValues.STATION2_AXISYMIN)
                        {
                            textbox.Text = ShareValues.STATION2_AXISYMIN.ToString();
                        }
                        else if (result > ShareValues.STATION2_AXISYMAX)
                        {
                            textbox.Text = ShareValues.STATION2_AXISYMAX.ToString();
                        }
                    }
                    else if (textbox.Name.Substring(0,1) == "Z")
                    {
                        if (result < ShareValues.STATION2_AXISRMIN)
                        {
                            textbox.Text = ShareValues.STATION2_AXISRMIN.ToString();
                        }
                        else if (result > ShareValues.STATION2_AXISRMAX)
                        {
                            textbox.Text = ShareValues.STATION2_AXISRMAX.ToString();
                        }
                    }
                }

            }
        }

        private void Inttext_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (textbox.Text == "")
            {
                textbox.Text = "1";
            }
            int result;
            bool success = int.TryParse(textbox.Text, out result);
            if (success)
            {
                if(textbox.Name == "PalletIDtext")
                {
                    if (result < 1)
                    {
                        textbox.Text = "1";
                    }
                    else if (result > ShareValues.MAXPALLETNUM)
                    {
                        textbox.Text = ShareValues.MAXPALLETNUM.ToString();
                    }
                }
                else if(textbox.Name == "PalletWtext")
                {
                    if (result < 1)
                    {
                        textbox.Text = "1";
                    }
                    else if (result > ShareValues.MAXMODULENUMX)
                    {
                        textbox.Text = ShareValues.MAXMODULENUMX.ToString();
                    }
                }
                else if (textbox.Name == "PalletHtext")
                {
                    if (result < 1)
                    {
                        textbox.Text = "1";
                    }
                    else if (result > ShareValues.MAXMODULENUMY)
                    {
                        textbox.Text = ShareValues.MAXMODULENUMY.ToString();
                    }
                }
                else if (textbox.Name == "Pointsnumtext")
                {
                    if (result < 1)
                    {
                        textbox.Text = "1";
                    }
                    else if (result > ShareValues.MAXPOINTSNUM)
                    {
                        textbox.Text = ShareValues.MAXPOINTSNUM.ToString();
                    }
                }
            }
        }
    }
}
