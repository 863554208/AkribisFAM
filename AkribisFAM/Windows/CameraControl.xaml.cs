using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// CameraControl.xaml 的交互逻辑
    /// </summary>
    public partial class CameraControl : UserControl
    {
        public CameraControl()
        {
            InitializeComponent();
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
    }
}
