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
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace AkribisFAM.Windows
{
    public enum PointsType
    {
        NULL,
        Laser,
        Pallet,
        Feeder,
        Detect,
        Recheck
    }
    //scanning gantry
    public struct ScanningPoint
    {
        public double x;
        public double y;
        public double z;
    }

    //assmebly gantry
    public struct AssmeblyPoint
    {
        public double x;
        public double y;
        public double r;
    }

    /// <summary>
    /// SetPoint.xaml 的交互逻辑
    /// </summary>
    public partial class SetPoint : Window
    {
        public int pointsnum = 1;
        public int moduleID = 1;

        public List<ScanningPoint> scanningpointlist;
        public List<AssmeblyPoint> assmeblypointlist;

        //points type
        public PointsType type = PointsType.NULL;

        public SetPoint()
        {
            InitializeComponent();
            scanningpointlist = new List<ScanningPoint>();
            assmeblypointlist = new List<AssmeblyPoint>();
        }

        public void SetComponents()
        {
            if(type == PointsType.Laser)
            {
                SetLaserComponents();
            }
            else if(type == PointsType.Feeder)
            {
                SetAssemblyComponents();
            }
        }

        //四行二*pointsnum列
        private void SetLaserComponents()
        {
            Grid grid = new Grid();
            this.Width = 150 * pointsnum;
            this.Height = 200;
            this.ResizeMode = ResizeMode.NoResize;
            this.Content = grid;
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition()); // 将行定义添加到Grid中
            }
            for (int j = 0; j < 2 * pointsnum; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition()); // 将列定义添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++) {
                // 创建label X
                Label labelx = new Label();
                labelx.Content = $"X : ";
                labelx.Width = 50; // 设置宽度
                labelx.Height = 25; // 设置高度
                labelx.Margin = new Thickness(5); // 设置外边距
                Grid.SetRow(labelx, 0); // 设置行位置
                Grid.SetColumn(labelx, i * 2); // 设置列位置
                if (grid != null) grid.Children.Add(labelx); // 添加到Grid中
            }

            for(int i = 0; i < pointsnum; i++)
            {
                // 创建textbox X
                TextBox textboxx = new TextBox();
                textboxx.Name = $"textboxX{i}";
                textboxx.Text = $"{scanningpointlist[i].x}";
                textboxx.Width = 50; // 设置宽度
                textboxx.Height = 25; // 设置高度
                textboxx.Margin = new Thickness(5); // 设置外边距
                textboxx.MaxLines = 1;
                textboxx.TextWrapping = TextWrapping.NoWrap;
                textboxx.TextChanged += textboxX_TextChanged; // 绑定事件处理器
                Grid.SetRow(textboxx, 0); // 设置的行位置
                Grid.SetColumn(textboxx, i * 2 + 1); // 设置的列位置
                if (grid != null) grid.Children.Add(textboxx); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建label Y
                Label labely = new Label();
                labely.Content = $"Y : ";
                labely.Width = 50; // 设置宽度
                labely.Height = 25; // 设置高度
                labely.Margin = new Thickness(5); // 设置外边距
                Grid.SetRow(labely, 1); // 设置行位置
                Grid.SetColumn(labely, i * 2); // 设置列位置
                if (grid != null) grid.Children.Add(labely); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建textbox Y
                TextBox textboxy = new TextBox();
                textboxy.Name = $"textboxY{i}";
                textboxy.Text = $"{scanningpointlist[i].y}";
                textboxy.Width = 50; // 设置宽度
                textboxy.Height = 25; // 设置高度
                textboxy.Margin = new Thickness(5); // 设置外边距
                textboxy.MaxLines = 1;
                textboxy.TextWrapping = TextWrapping.NoWrap;
                textboxy.TextChanged += textboxX_TextChanged; // 绑定事件处理器
                Grid.SetRow(textboxy, 1); // 设置的行位置
                Grid.SetColumn(textboxy, i * 2 + 1); // 设置的列位置
                if (grid != null) grid.Children.Add(textboxy); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建label Z
                Label labelz = new Label();
                labelz.Content = $"Z : ";
                labelz.Width = 50; // 设置宽度
                labelz.Height = 25; // 设置高度
                labelz.Margin = new Thickness(5); // 设置外边距
                Grid.SetRow(labelz, 2); // 设置行位置
                Grid.SetColumn(labelz, i * 2); // 设置列位置
                if (grid != null) grid.Children.Add(labelz); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建textbox Z
                TextBox textboxz = new TextBox();
                textboxz.Name = $"textboxZ{i}";
                textboxz.Text = $"{scanningpointlist[i].z}";
                textboxz.Width = 50; // 设置宽度
                textboxz.Height = 25; // 设置高度
                textboxz.Margin = new Thickness(5); // 设置外边距
                textboxz.MaxLines = 1;
                textboxz.TextWrapping = TextWrapping.NoWrap;
                textboxz.TextChanged += textboxX_TextChanged; // 绑定事件处理器
                Grid.SetRow(textboxz, 2); // 设置的行位置
                Grid.SetColumn(textboxz, i * 2 + 1); // 设置的列位置
                if (grid != null) grid.Children.Add(textboxz); // 添加到Grid中
            }

            // 创建button
            Button myButton = new Button();
            myButton.Content = $"Apply"; // 设置按钮内容
            myButton.Width = 50; // 设置按钮宽度
            myButton.Height = 25; // 设置按钮高度
            myButton.Margin = new Thickness(5); // 设置按钮外边距
            myButton.Click += Button_Click; // 绑定点击事件处理器
            Grid.SetRow(myButton, 3); // 设置按钮的行位置（如果需要）
            Grid.SetColumn(myButton, pointsnum*2-1); // 设置按钮的列位置（如果需要）
            if (grid != null) grid.Children.Add(myButton); // 将按钮添加到Grid中


        }
        
        //四行二*pointsnum列
        private void SetAssemblyComponents()
        {
            Grid grid = new Grid();
            this.Width = 150 * pointsnum;
            this.Height = 200;
            this.ResizeMode = ResizeMode.NoResize;
            this.Content = grid;
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.Children.Clear();
            for (int i = 0; i < 4; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition()); // 将行定义添加到Grid中
            }
            for (int j = 0; j < 2 * pointsnum; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition()); // 将列定义添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建label X
                Label labelx = new Label();
                labelx.Content = $"X : ";
                labelx.Width = 50; // 设置宽度
                labelx.Height = 25; // 设置高度
                labelx.Margin = new Thickness(5); // 设置外边距
                Grid.SetRow(labelx, 0); // 设置行位置
                Grid.SetColumn(labelx, i * 2); // 设置列位置
                if (grid != null) grid.Children.Add(labelx); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建textbox X
                TextBox textboxx = new TextBox();
                textboxx.Name = $"textboxX{i}";
                textboxx.Text = $"{scanningpointlist[i].x}";
                textboxx.Width = 50; // 设置宽度
                textboxx.Height = 25; // 设置高度
                textboxx.Margin = new Thickness(5); // 设置外边距
                textboxx.MaxLines = 1;
                textboxx.TextWrapping = TextWrapping.NoWrap;
                textboxx.TextChanged += textboxX_TextChanged; // 绑定事件处理器
                Grid.SetRow(textboxx, 0); // 设置的行位置
                Grid.SetColumn(textboxx, i * 2 + 1); // 设置的列位置
                if (grid != null) grid.Children.Add(textboxx); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建label Y
                Label labely = new Label();
                labely.Content = $"Y : ";
                labely.Width = 50; // 设置宽度
                labely.Height = 25; // 设置高度
                labely.Margin = new Thickness(5); // 设置外边距
                Grid.SetRow(labely, 1); // 设置行位置
                Grid.SetColumn(labely, i * 2); // 设置列位置
                if (grid != null) grid.Children.Add(labely); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建textbox Y
                TextBox textboxy = new TextBox();
                textboxy.Name = $"textboxY{i}";
                textboxy.Text = $"{scanningpointlist[i].y}";
                textboxy.Width = 50; // 设置宽度
                textboxy.Height = 25; // 设置高度
                textboxy.Margin = new Thickness(5); // 设置外边距
                textboxy.MaxLines = 1;
                textboxy.TextWrapping = TextWrapping.NoWrap;
                textboxy.TextChanged += textboxX_TextChanged; // 绑定事件处理器
                Grid.SetRow(textboxy, 1); // 设置的行位置
                Grid.SetColumn(textboxy, i * 2 + 1); // 设置的列位置
                if (grid != null) grid.Children.Add(textboxy); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建label R
                Label labelz = new Label();
                labelz.Content = $"R : ";
                labelz.Width = 50; // 设置宽度
                labelz.Height = 25; // 设置高度
                labelz.Margin = new Thickness(5); // 设置外边距
                Grid.SetRow(labelz, 2); // 设置行位置
                Grid.SetColumn(labelz, i * 2); // 设置列位置
                if (grid != null) grid.Children.Add(labelz); // 添加到Grid中
            }

            for (int i = 0; i < pointsnum; i++)
            {
                // 创建textbox R
                TextBox textboxz = new TextBox();
                textboxz.Name = $"textboxR{i}";
                textboxz.Text = $"{scanningpointlist[i].z}";
                textboxz.Width = 50; // 设置宽度
                textboxz.Height = 25; // 设置高度
                textboxz.Margin = new Thickness(5); // 设置外边距
                textboxz.MaxLines = 1;
                textboxz.TextWrapping = TextWrapping.NoWrap;
                textboxz.TextChanged += textboxX_TextChanged; // 绑定事件处理器
                Grid.SetRow(textboxz, 2); // 设置的行位置
                Grid.SetColumn(textboxz, i * 2 + 1); // 设置的列位置
                if (grid != null) grid.Children.Add(textboxz); // 添加到Grid中
            }

            // 创建button
            Button myButton = new Button();
            myButton.Content = $"Apply"; // 设置按钮内容
            myButton.Width = 50; // 设置按钮宽度
            myButton.Height = 25; // 设置按钮高度
            myButton.Margin = new Thickness(5); // 设置按钮外边距
            myButton.Click += Button_Click; // 绑定点击事件处理器
            Grid.SetRow(myButton, 3); // 设置按钮的行位置（如果需要）
            Grid.SetColumn(myButton, pointsnum * 2 - 1); // 设置按钮的列位置（如果需要）
            if (grid != null) grid.Children.Add(myButton); // 将按钮添加到Grid中


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textboxX_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            double result;
            bool success = double.TryParse(textBox.Text, out result);
            if (success)
            {
                if(type == PointsType.Laser)
                {
                    if (textBox.Name.Substring(7, 1) == "X")
                    {
                        if (result < ShareValues.AxisMin[2])
                        {
                            textBox.Text = ShareValues.AxisMin[2].ToString();
                        }
                        else if (result > ShareValues.AxisMax[2])
                        {
                            textBox.Text = ShareValues.AxisMax[2].ToString();
                        }
                    }
                    else if(textBox.Name.Substring(7, 1) == "Y")
                    {
                        if (result < ShareValues.AxisMin[3])
                        {
                            textBox.Text = ShareValues.AxisMin[3].ToString();
                        }
                        else if (result > ShareValues.AxisMax[3])
                        {
                            textBox.Text = ShareValues.AxisMax[3].ToString();
                        }
                    }
                    else if (textBox.Name.Substring(7, 1) == "Z")
                    {
                        if (result < ShareValues.AxisMin[4])
                        {
                            textBox.Text = ShareValues.AxisMin[4].ToString();
                        }
                        else if (result > ShareValues.AxisMax[4])
                        {
                            textBox.Text = ShareValues.AxisMax[4].ToString();
                        }
                    }
                }
                else if(type == PointsType.Feeder)
                {
                    if (textBox.Name.Substring(7, 1) == "X")
                    {
                        if (result < ShareValues.AxisMin[7])
                        {
                            textBox.Text = ShareValues.AxisMin[7].ToString();
                        }
                        else if (result > ShareValues.AxisMax[7])
                        {
                            textBox.Text = ShareValues.AxisMax[7].ToString();
                        }
                    }
                    else if (textBox.Name.Substring(7, 1) == "Y")
                    {
                        if (result < ShareValues.AxisMin[8])
                        {
                            textBox.Text = ShareValues.AxisMin[8].ToString();
                        }
                        else if (result > ShareValues.AxisMax[8])
                        {
                            textBox.Text = ShareValues.AxisMax[8].ToString();
                        }
                    }
                    else if (textBox.Name.Substring(7, 1) == "R")
                    {
                        if (result < ShareValues.AxisMin[9])
                        {
                            textBox.Text = ShareValues.AxisMin[9].ToString();
                        }
                        else if (result > ShareValues.AxisMax[9])
                        {
                            textBox.Text = ShareValues.AxisMax[9].ToString();
                        }
                    }
                }

                for (int i = 0; i < pointsnum; i++)
                {
                    if (textBox.Name == $"textboxX{i}")
                    {
                        ScanningPoint pt = scanningpointlist[i];
                        pt.x = result;
                        scanningpointlist[i] = pt;
                    }
                    else if (textBox.Name == $"textboxY{i}")
                    {
                        ScanningPoint pt = scanningpointlist[i];
                        pt.y = result;
                        scanningpointlist[i] = pt;
                    }
                    else if (textBox.Name == $"textboxZ{i}")
                    {
                        ScanningPoint pt = scanningpointlist[i];
                        pt.z = result;
                        scanningpointlist[i] = pt;
                    }
                }
            }
        }
    }
}
