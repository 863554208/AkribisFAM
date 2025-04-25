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
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace WpfApp1
{
    /// <summary>
    /// PalletPointsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PalletPointsWindow : Window
    {
        private int moduleW = 200;
        private int moduleH = 200;
        public int modulenumX = 1;
        public int modulenumY = 1;
        public int pointsnum = 1;
        public JObject jsonObject;
        public string jsonpath;
        public PointsType jsontype;
        Grid grid = new Grid();

        public PalletPointsWindow()
        {
            InitializeComponent();

        }

        public void setWindow()
        {
            this.Width = modulenumX * moduleW;
            this.Height = modulenumY * moduleH + 40;
            this.ResizeMode = ResizeMode.NoResize;
            this.Content = grid;
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.Children.Clear();
            for (int i = 0; i < modulenumY; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition()); // 将行定义添加到Grid中
            }
            for (int j = 0; j < modulenumX; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition()); // 将列定义添加到Grid中
            }
            AddButton(modulenumY, modulenumX);
        }

        private void AddButton(int modulenumY, int modulenumX)
        {
            if(jsontype == PointsType.Laser)
            {
                for (int i = 0; i < modulenumY; i++)
                {
                    for (int j = 0; j < modulenumX; j++)
                    {
                        // 创建按钮实例并设置属性
                        Button myButton = new Button();
                        myButton.Name = $"module{i * modulenumX + j + 1}";
                        string pointsinfo = "";
                        for (int k = 0; k < pointsnum; k++)
                        {
                            pointsinfo += $"\r\nX={jsonObject["module" + $"{i * modulenumX + j + 1}"]["Point" + $"{k + 1}"]["X"].ToString()}";
                            pointsinfo += $" Y={jsonObject["module" + $"{i * modulenumX + j + 1}"]["Point" + $"{k + 1}"]["Y"].ToString()}";
                            pointsinfo += $" Z={jsonObject["module" + $"{i * modulenumX + j + 1}"]["Point" + $"{k + 1}"]["Z"].ToString()}";
                        }
                        myButton.Content = $"Module {i * modulenumX + j + 1}" + pointsinfo; // 设置按钮内容
                        myButton.Width = 180; // 设置按钮宽度
                        myButton.Height = 180; // 设置按钮高度
                        myButton.Margin = new Thickness(5); // 设置按钮外边距
                        myButton.Background = Brushes.LightBlue; // 设置按钮背景色为浅蓝色
                        myButton.Foreground = Brushes.White; // 设置按钮前景色为白色（文字颜色）
                        myButton.Click += Button_Click; // 绑定点击事件处理器
                        Grid.SetRow(myButton, i); // 设置按钮的行位置（如果需要）
                        Grid.SetColumn(myButton, j); // 设置按钮的列位置（如果需要）
                        if (grid != null) grid.Children.Add(myButton); // 将按钮添加到Grid中
                    }
                }
            }
            else if (jsontype == PointsType.Feeder)
            {
                for (int i = 0; i < modulenumY; i++)
                {
                    for (int j = 0; j < modulenumX; j++)
                    {
                        // 创建按钮实例并设置属性
                        Button myButton = new Button();
                        myButton.Name = $"module{i * modulenumX + j + 1}";
                        string pointsinfo = "";
                        for (int k = 0; k < pointsnum; k++)
                        {
                            pointsinfo += $"\r\nX={jsonObject["module" + $"{i * modulenumX + j + 1}"]["Point" + $"{k + 1}"]["X"].ToString()}";
                            pointsinfo += $" Y={jsonObject["module" + $"{i * modulenumX + j + 1}"]["Point" + $"{k + 1}"]["Y"].ToString()}";
                            pointsinfo += $" R={jsonObject["module" + $"{i * modulenumX + j + 1}"]["Point" + $"{k + 1}"]["R"].ToString()}";
                        }
                        myButton.Content = $"Module {i * modulenumX + j + 1}" + pointsinfo; // 设置按钮内容
                        myButton.Width = 180; // 设置按钮宽度
                        myButton.Height = 180; // 设置按钮高度
                        myButton.Margin = new Thickness(5); // 设置按钮外边距
                        myButton.Background = Brushes.LightBlue; // 设置按钮背景色为浅蓝色
                        myButton.Foreground = Brushes.White; // 设置按钮前景色为白色（文字颜色）
                        myButton.Click += Button_Click; // 绑定点击事件处理器
                        Grid.SetRow(myButton, i); // 设置按钮的行位置（如果需要）
                        Grid.SetColumn(myButton, j); // 设置按钮的列位置（如果需要）
                        if (grid != null) grid.Children.Add(myButton); // 将按钮添加到Grid中
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetPoint setpointwindow = new SetPoint();
            Button Button = sender as Button;
            int moduleID = 0;
            for (int i = 0; i < modulenumY; i++)
            {
                for (int j = 0; j < modulenumX; j++)
                {
                    if(Button.Name == $"module{i * modulenumX + j + 1}")
                    {
                        moduleID = i * modulenumX + j + 1;
                    }
                }
            }
            for (int k = 0; k < pointsnum; k++)
            {
                ScanningPoint spoint = new ScanningPoint();
                spoint.x = double.Parse(jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["X"].ToString());
                spoint.y = double.Parse(jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["Y"].ToString());
                if(jsontype == PointsType.Feeder)
                {
                    spoint.z = double.Parse(jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["R"].ToString());
                }
                else
                {
                    spoint.z = double.Parse(jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["Z"].ToString());
                }
                setpointwindow.scanningpointlist.Add(spoint);
            }
            setpointwindow.moduleID = moduleID;
            setpointwindow.pointsnum = pointsnum;
            setpointwindow.type = jsontype;
            setpointwindow.SetComponents();
            setpointwindow.ShowDialog();
            for (int k = 0; k < pointsnum; k++)
            {
                jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["X"] = Math.Round(setpointwindow.scanningpointlist[k].x, 3);
                jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["Y"] = Math.Round(setpointwindow.scanningpointlist[k].y, 3);
                if (jsontype == PointsType.Feeder)
                    jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["R"] = Math.Round(setpointwindow.scanningpointlist[k].z, 3);
                else
                    jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["Z"] = Math.Round(setpointwindow.scanningpointlist[k].z, 3);
            }
            string pointsinfo = "";
            for (int k = 0; k < pointsnum; k++)
            {
                pointsinfo += $"\r\nX={jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["X"].ToString()}";
                pointsinfo += $" Y={jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["Y"].ToString()}";
                if (jsontype == PointsType.Feeder)
                    pointsinfo += $" R={jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["R"].ToString()}";
                else
                    pointsinfo += $" Z={jsonObject["module" + $"{moduleID}"]["Point" + $"{k + 1}"]["Z"].ToString()}";
            }
            Button.Content = $"Module {moduleID}" + pointsinfo; // 设置按钮内容
            string strSrc = Convert.ToString(jsonObject);//将json装换为string
            File.WriteAllText(jsonpath, strSrc, System.Text.Encoding.UTF8);//将内容写进json文件
        }
    }
}
