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
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;
using Microsoft.Win32;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class MainContent : UserControl
    {
        public ColorChangeViewModel ViewModel { get; }

        public MainContent()
        {
            InitializeComponent();

            ViewModel = new ColorChangeViewModel();
            this.DataContext = ViewModel;
    
            ZuZhuang.Current.OnTriggerStep1 += UpdateTest1ColorToGreen;
            ZuZhuang.Current.OnStopStep1 += UpdateTest1ColorToTransparent;

            ZuZhuang.Current.OnTriggerStep2 += UpdateTest2ColorToGreen;
            ZuZhuang.Current.OnStopStep2 += UpdateTest2ColorToTransparent;

            ZuZhuang.Current.OnTriggerStep3 += UpdateTest3ColorToGreen;
            ZuZhuang.Current.OnStopStep3 += UpdateTest3ColorToTransparent;

            ZuZhuang.Current.OnTriggerStep4 += UpdateTest4ColorToGreen;
            ZuZhuang.Current.OnStopStep4 += UpdateTest4ColorToTransparent;

            LaiLiao.Current.OnTriggerStep1 += Updatelailiao1ColorToGreen;
            LaiLiao.Current.OnStopStep1 += Updatelailiao1ColorToTransparent;
            LaiLiao.Current.OnTriggerStep2 += Updatelailiao2ColorToGreen;
            LaiLiao.Current.OnStopStep2 += Updatelailiao2ColorToTransparent;
            LaiLiao.Current.OnTriggerStep3 += Updatelailiao3ColorToGreen;
            LaiLiao.Current.OnStopStep3 += Updatelailiao3ColorToTransparent;


            FuJian.Current.OnTriggerStep1 += UpdateFuJian1ColorToGreen;
            FuJian.Current.OnStopStep1 += UpdateFuJian1ColorToTransparent;
            FuJian.Current.OnTriggerStep2 += UpdateFuJian2ColorToGreen;
            FuJian.Current.OnStopStep2 += UpdateFuJian2ColorToTransparent;
            FuJian.Current.OnTriggerStep3 += UpdateFuJian3ColorToGreen;
            FuJian.Current.OnStopStep3 += UpdateFuJian3ColorToTransparent;

        }

        private void UpdateFuJian1ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateFuJian_1ColorToGreen());
        }

        private void UpdateFuJian1ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateFuJian_1ColorToTransparent());
        }

        private void UpdateFuJian2ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateFuJian_2ColorToGreen());
        }

        private void UpdateFuJian2ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateFuJian_2ColorToTransparent());
        }

        private void UpdateFuJian3ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateFuJian_3ColorToGreen());
        }

        private void UpdateFuJian3ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateFuJian_3ColorToTransparent());
        }




        private void Updatelailiao1ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLailiao1ColorToGreen());
        }

        private void Updatelailiao1ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLailiao1ColorToTransparent());
        }

        private void Updatelailiao2ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLailiao2ColorToGreen());
        }

        private void Updatelailiao2ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLailiao2ColorToTransparent());
        }

        private void Updatelailiao3ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLailiao3ColorToGreen());
        }

        private void Updatelailiao3ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateLailiao3ColorToTransparent());
        }


        private void UpdateTest1ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest1ColorToGreen());
        }

        private void UpdateTest1ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest1ColorToTransparent());
        }

        private void UpdateTest2ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest2ColorToGreen());
        }

        private void UpdateTest2ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest2ColorToTransparent());
        }

        private void UpdateTest3ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest3ColorToGreen());
        }

        private void UpdateTest3ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest3ColorToTransparent());
        }

        private void UpdateTest4ColorToGreen()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest4ColorToGreen());
        }

        private void UpdateTest4ColorToTransparent()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateTest4ColorToTransparent());
        }

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
        //        cameraDisplay1.Source = new BitmapImage(new Uri(filePath));
        //        cameraDisplay2.Source = new BitmapImage(new Uri(filePath));
        //    }
        //}

        private void Time3Set_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(test3_time.Text, out int value))
            {
                GlobalManager.Current.step3_time = value;
            }
            else
            {
                MessageBox.Show("请输入有效的整数！");
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            
            if(GlobalManager.Current.IsPass == true)
            {
                GlobalManager.Current.IsPass = false;
                this.Change_Block.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                GlobalManager.Current.IsPass = true;
                this.Change_Block.Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void SendIO_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.Current.IO_test1 = true;
        }
        private void StopZuZhuang_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.Current.Zuzhuang_state[3] = 1;
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 0;
            GlobalManager.Current.Zuzhuang_delta[GlobalManager.Current.current_Zuzhuang_step] = 0;
        }
    }
}
