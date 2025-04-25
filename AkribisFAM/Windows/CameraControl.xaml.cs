using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
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
        private void OnSelectImageClick(object sender, RoutedEventArgs e)
        {
            // 创建文件选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置初始目录为默认的图片文件夹（例如，项目目录中的 Images 文件夹）
            openFileDialog.InitialDirectory = @"C:\Users\Public\Pictures"; // 设置为你想要的默认目录

            // 过滤器，用于显示特定类型的文件（例如，仅显示图片文件）
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            // 如果用户选择了文件并点击“打开”
            if (openFileDialog.ShowDialog() == true)
            {
                // 获取选中的文件路径
                string filePath = openFileDialog.FileName;

                // 将选中的文件显示到 Image 控件中
                imageDisplay.Source = new BitmapImage(new Uri(filePath));
            }
        }


    }
}
