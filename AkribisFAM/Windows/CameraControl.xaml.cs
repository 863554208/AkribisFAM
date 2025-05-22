using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.CommunicationProtocol.CamerCalibProcess;
using AkribisFAM.Properties;
using AkribisFAM.WorkStation;
using HslCommunication;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.WorkStation.Reject;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// CameraControl.xaml 的交互逻辑
    /// </summary>
    public partial class CameraControl : UserControl
    {
        private bool Calibstatus_Click;
        public CameraControl()
        {
            InitializeComponent();
            Calibstatus_Click = true;
            try
            {
                string folder = Directory.GetCurrentDirectory(); //获取应用程序的当前工作目录。 
                string path = folder + "\\NozzleCalib1.json";
                string content = File.ReadAllText(path);
                Reject.Current.TrainPointlist = JsonConvert.DeserializeObject<List<TrainPoint>>(content);
            }
            catch
            {

            }

        }

        // 按钮点击事件处理
        //private void OnSelectImageClick(object sender, RoutedEventArgs e)
        //{
        //    // 创建文件选择对话框实例
        //    OpenFileDialog openFileDialog = new OpenFileDialog();

        //    // 设置初始目录为默认的图片文件夹（例如，项目目录中的 Images 文件夹）
        //    openFileDialog.InitialDirectory = @"C:\Users\Public\Pictures"; // 设置为你想要的默认目录

        //    // 过滤器，用于显示特定类型的文件（例如，仅显示图片文件）
        //    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

        //    // 如果用户选择了文件并点击“打开”
        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        // 获取选中的文件路径
        //        string filePath = openFileDialog.FileName;

        //        // 将选中的文件显示到 Image 控件中
        //        imageDisplay.Source = new BitmapImage(new Uri(filePath));
        //    }
        //}

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Loadbtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif)|*.png;*.jpeg;*.jpg;*.gif"; // 设置文件过滤器

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ImageSource imageSource = new BitmapImage(new Uri(filePath));
                Imageview.Source = imageSource;
            }
        }

        private void SaveImage(string filename)
        {
            // 这里假设你有一个BitmapSource类型的图片变量叫myBitmapSource，你需要将其保存到文件
            BitmapEncoder encoder = null;
            switch (Path.GetExtension(filename).ToLower())
            {
                case ".png": encoder = new PngBitmapEncoder(); break;
                case ".jpg": encoder = new JpegBitmapEncoder(); break;
                case ".jpeg": encoder = new JpegBitmapEncoder(); break;
                case ".gif": encoder = new GifBitmapEncoder(); break;
            }
            if (encoder != null)
            {
                BitmapFrame frame = BitmapFrame.Create(Imageview.Source as BitmapImage); // myBitmapSource是你的图片源，例如一个RenderTargetBitmap对象
                encoder.Frames.Add(frame);
                using (FileStream stream = File.Create(filename))
                {
                    encoder.Save(stream); // 保存图片到文件
                }
            }
        }

        private void Savebtn_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个SaveFileDialog实例
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPG Image|*.jpg|GIF Image|*.gif"; // 设置文件类型过滤器
            saveFileDialog.FileName = "Image"; // 默认文件名
            saveFileDialog.DefaultExt = ".png"; // 默认文件扩展名

            // 显示保存对话框
            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true) // 如果用户点击了OK按钮
            {
                string filename = saveFileDialog.FileName; // 获取保存的文件名和路径
                SaveImage(filename); // 调用保存图片的方法
            }
        }

        private void Capturebtn_Click(object sender, RoutedEventArgs e)
        {

        }

        
        private async void NozzleCalib_Click(object sender, RoutedEventArgs e)
        {
            int nozzlenum = NozzleCalibNum.SelectedIndex;
            if (nozzlenum < 0 || nozzlenum >=4) {
                return;
            }
            if (Calibstatus_Click)
            {
                Calibstatus_Click = false;
                try
                {
                    await Reject.Current.TrainNozzles(nozzlenum);   
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the Nozzle calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            }
        }

        private async void Points11Calib_Click(object sender, RoutedEventArgs e)
        {
            int nozzlenum = Points11CalibNum.SelectedIndex;
            if (nozzlenum < 0 || nozzlenum >= 4)
            {
                return;
            }
            if (Calibstatus_Click)
            {
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.Point11Calibprocess((NozzleNumber)nozzlenum);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the Points11 calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            }
        }

        private async void JointCalib_Click(object sender, RoutedEventArgs e)
        {
            if (Calibstatus_Click)
            {
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.CombineCalibrationprocess();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the joint calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            }
        }

        private async void Points9Calib_Click(object sender, RoutedEventArgs e)
        {
            int calibnum = Points9CalibNum.SelectedIndex;
            if (calibnum < 0 || calibnum >= 2)
            {
                return;
            }
            if (Calibstatus_Click)
            {
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.Point9Calibprocess((MovingCameraCalibposition)calibnum);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the Points9 calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            }
        }

        private void Train_Click(object sender, RoutedEventArgs e)
        {
            int nozzlenum = NozzleCalibNum.SelectedIndex;
            if (nozzlenum < 0 || nozzlenum >= 4)
            {
                return;
            }
            try {
                Reject.Current.TrainPointlist[nozzlenum].x = double.Parse(Xpos.Text);
                Reject.Current.TrainPointlist[nozzlenum].y = double.Parse(Ypos.Text);
                Reject.Current.TrainPointlist[nozzlenum].z = double.Parse(Zpos.Text);
                Reject.Current.TrainPointlist[nozzlenum].r = double.Parse(Rpos.Text);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                string content = JsonConvert.SerializeObject(Reject.Current.TrainPointlist, settings);
                string path = Directory.GetCurrentDirectory() + "\\NozzleCalib1.json";
                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed! :" + ex.Message);
            }
        }

        private void Train2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Reject.Current.TrainPointlist[4].x =  double.Parse(Xpos.Text);
                Reject.Current.TrainPointlist[4].y =  double.Parse(Ypos.Text);
                Reject.Current.TrainPointlist[4].z =  double.Parse(Zpos.Text);
                Reject.Current.TrainPointlist[4].r =  double.Parse(Rpos.Text);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                string content = JsonConvert.SerializeObject(Reject.Current.TrainPointlist, settings);
                string path = Directory.GetCurrentDirectory() + "\\NozzleCalib1.json";
                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed! :" + ex.Message);
            }
        }

        private void Save9points_Click(object sender, RoutedEventArgs e)
        {
            int calibnum = Points9CalibNum.SelectedIndex;
            if (calibnum == 0) {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
                string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
                JObject obj = JObject.Parse(json);
                var MoveCameraCalibmoveAxisNozzle = obj["MoveCameraCalibMoveAxisCalibposition"]?[MovingCameraCalibposition.FeedDischarging.ToString()] as JArray;//获取轴Nozzle 数组 
                
                double XstepV = double.Parse(Xstep.Text);
                double YstepV = double.Parse(Ystep.Text);
                double RstepV = double.Parse(Rstep.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][0][0] = double.Parse(Xpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][0][1] = double.Parse(Ypos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][0][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][0][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][1][0] = double.Parse(Xpos.Text)+ XstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][1][1] = double.Parse(Ypos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][1][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][1][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][2][0] = double.Parse(Xpos.Text) + XstepV*2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][2][1] = double.Parse(Ypos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][2][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][2][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][3][0] = double.Parse(Xpos.Text) + XstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][3][1] = double.Parse(Ypos.Text) + YstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][3][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][3][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][4][0] = double.Parse(Xpos.Text) + XstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][4][1] = double.Parse(Ypos.Text) + YstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][4][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][4][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][5][0] = double.Parse(Xpos.Text) + XstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][5][1] = double.Parse(Ypos.Text) + YstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][5][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][5][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][6][0] = double.Parse(Xpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][6][1] = double.Parse(Ypos.Text) + YstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][6][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][6][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][7][0] = double.Parse(Xpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][7][1] = double.Parse(Ypos.Text) + YstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][7][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][7][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][8][0] = double.Parse(Xpos.Text) + XstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][8][1] = double.Parse(Ypos.Text) + YstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][8][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["FeedDischarging"][8][3] = double.Parse(Rpos.Text);
                File.WriteAllText(filePath, obj.ToString());
            }
            if (calibnum == 1)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
                string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
                JObject obj = JObject.Parse(json);
                var MoveCameraCalibmoveAxisNozzle = obj["MoveCameraCalibMoveAxisCalibposition"]?[MovingCameraCalibposition.FeedDischarging.ToString()] as JArray;//获取轴Nozzle 数组 

                double XstepV = double.Parse(Xstep.Text);
                double YstepV = double.Parse(Ystep.Text);
                double RstepV = double.Parse(Rstep.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][0][0] = double.Parse(Xpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][0][1] = double.Parse(Ypos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][0][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][0][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][1][0] = double.Parse(Xpos.Text) + XstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][1][1] = double.Parse(Ypos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][1][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][1][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][2][0] = double.Parse(Xpos.Text) + XstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][2][1] = double.Parse(Ypos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][2][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][2][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][3][0] = double.Parse(Xpos.Text) + XstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][3][1] = double.Parse(Ypos.Text) + YstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][3][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][3][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][4][0] = double.Parse(Xpos.Text) + XstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][4][1] = double.Parse(Ypos.Text) + YstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][4][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][4][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][5][0] = double.Parse(Xpos.Text) + XstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][5][1] = double.Parse(Ypos.Text) + YstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][5][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][5][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][6][0] = double.Parse(Xpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][6][1] = double.Parse(Ypos.Text) + YstepV * 2;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][6][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][6][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][7][0] = double.Parse(Xpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][7][1] = double.Parse(Ypos.Text) + YstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][7][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][7][3] = double.Parse(Rpos.Text);

                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][8][0] = double.Parse(Xpos.Text) + XstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][8][1] = double.Parse(Ypos.Text) + YstepV;
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][8][2] = double.Parse(Zpos.Text);
                obj["MoveCameraCalibMoveAxisCalibposition"]["Vehicles"][8][3] = double.Parse(Rpos.Text);
                File.WriteAllText(filePath, obj.ToString());
            }
        }

        private void Save11points_Click(object sender, RoutedEventArgs e)
        {
            int nozzlenum = Points11CalibNum.SelectedIndex;
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            double XstepV = double.Parse(Xstep.Text);
            double YstepV = double.Parse(Ystep.Text);
            double RstepV = double.Parse(Rstep.Text);

        }
    }
}
