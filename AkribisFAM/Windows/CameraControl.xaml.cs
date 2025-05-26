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
using AAMotion;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.CommunicationProtocol.CamerCalibProcess;
using AkribisFAM.Helper;
using AkribisFAM.WorkStation;
using MaterialDesignThemes.Wpf;
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
        List<string> posFilePre = new List<string>();
        List<string> posFileName = new List<string>();

        private const string MatrixPointPrefix = "Camera矩阵点:";
        TeachingWindow teachingWindow;

        private StationPoints stationPoints = new StationPoints();
        public StationPoints StationPoints
        {
            get { return stationPoints; }
            set { stationPoints = value; }
        }

        public CameraControl()
        {
            InitializeComponent();
            Calibstatus_Click = true;

            //Add by yxw
            posFilePre.Add("Camera_points1.json");
            posFilePre.Add("Camera_points2.json");
            posFilePre.Add("Camera_points3.json");
            posFilePre.Add("Camera_points4.json");
            posFilePre.Add("Camera_points5.json");

            for (int z = 0; z < posFilePre.Count; z++)
            {
                string nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(posFilePre[z]);
                CboxNowType.Items.Add(nameWithoutExtension);
            }
            CboxNowType.SelectedIndex = 0;
            CboxNowType.SelectionChanged += CboxNowType_SelectionChanged;

            // 读取数据并生成 UI
            for (int i = 0; i < posFilePre.Count; i++)
            {
                string jsonFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, posFilePre[i]);
                posFileName.Add(jsonFile);
                if (string.IsNullOrEmpty(jsonFile) || !File.Exists(jsonFile))
                {
                    InitAxisJsonPos(jsonFile);
                }
            }
            stationPoints = new StationPoints();
            FileHelper.LoadConfig(posFileName[0], out stationPoints);   //默认加载第一套参数
            InitTabs(stationPoints);
            //END ADD

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

        private void Cam3Calibbtn_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif)|*.png;*.jpeg;*.jpg;*.gif"; // 设置文件过滤器

            //if (openFileDialog.ShowDialog() == true)
            //{
            //    string filePath = openFileDialog.FileName;
            //    ImageSource imageSource = new BitmapImage(new Uri(filePath));
            //    Imageview.Source = imageSource;
            //}
            CamerCalibProcess.Instance.ReCheckCalibration();
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

        private void UniversalCalibbtn_Click(object sender, RoutedEventArgs e)
        {
            //// 创建一个SaveFileDialog实例
            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.Filter = "PNG Image|*.png|JPG Image|*.jpg|GIF Image|*.gif"; // 设置文件类型过滤器
            //saveFileDialog.FileName = "Image"; // 默认文件名
            //saveFileDialog.DefaultExt = ".png"; // 默认文件扩展名

            //// 显示保存对话框
            //Nullable<bool> result = saveFileDialog.ShowDialog();
            //if (result == true) // 如果用户点击了OK按钮
            //{
            //    string filename = saveFileDialog.FileName; // 获取保存的文件名和路径
            //    SaveImage(filename); // 调用保存图片的方法
            //}
            MessageBoxResult result = MessageBox.Show("Universal Calibration?", "Confirming", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                try
                {
                    CamerCalibProcess.Instance.AllCalibrationFinished();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Universal Calibration Failed!" + ex.Message);
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {

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
            //if (Calibstatus_Click)
            //{
                FileHelper.LoadConfig(posFileName[2], out CamerCalibProcess.Instance.CalibrationPoints);
                if (CamerCalibProcess.Instance.CalibrationPoints.ZuZhuangPointList.Count != 5)
                {
                    MessageBox.Show("Point Number is incorrect!");
                    return;
                }
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.TrainNozzles(nozzlenum);
                    MessageBox.Show("Calibration Finished!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the Nozzle calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            //}
        }

        private async void Points11Calib_Click(object sender, RoutedEventArgs e)
        {
            int nozzlenum = Points11CalibNum.SelectedIndex;
            if (nozzlenum < 1 || nozzlenum >= 4)
            {
                return;
            }
            //if (Calibstatus_Click)
            //{
                FileHelper.LoadConfig(posFileName[0], out CamerCalibProcess.Instance.CalibrationPoints);
                if (CamerCalibProcess.Instance.CalibrationPoints.ZuZhuangPointList.Count != 33) {
                    MessageBox.Show("Point Number is incorrect!");
                    return;
                }
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.Point11Calibprocess((NozzleNumber)nozzlenum);
                    MessageBox.Show("Calibration Finished!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the Points11 calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            //}
        }

        private async void JointCalib_Click(object sender, RoutedEventArgs e)
        {
            //if (Calibstatus_Click)
            //{
                FileHelper.LoadConfig(posFileName[3], out CamerCalibProcess.Instance.CalibrationPoints);
                if (CamerCalibProcess.Instance.CalibrationPoints.ZuZhuangPointList.Count != 17)
                {
                    MessageBox.Show("Point Number is incorrect!");
                    return;
                }
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.CombineCalibrationprocess();
                    MessageBox.Show("Calibration Finished!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the joint calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            //}
        }

        private async void Points9Calib_Click(object sender, RoutedEventArgs e)
        {
            int calibnum = Points9CalibNum.SelectedIndex;
            if (calibnum < 0 || calibnum >= 2)
            {
                return;
            }
            //if (Calibstatus_Click)
            //{
                FileHelper.LoadConfig(posFileName[1], out CamerCalibProcess.Instance.CalibrationPoints);
                if (CamerCalibProcess.Instance.CalibrationPoints.ZuZhuangPointList.Count != 18)
                {
                    MessageBox.Show("Point Number is incorrect!");
                    return;
                }
                Calibstatus_Click = false;
                try
                {
                    await CamerCalibProcess.Instance.Point9Calibprocess((MovingCameraCalibposition)calibnum);
                    MessageBox.Show("Calibration Finished!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during the Points9 calibration process:" + ex.Message);
                }
                Calibstatus_Click = true;
            
            //}
        }
        /// ======================================================================================================
        /// ======================================================================================================
        private void InitAxisJsonPos(string file)
        {
            StationPoints newStats = new StationPoints
            {
                LaiLiaoPointList = new List<Point>
                {
                    new Point
                    {
                        name = "LL1",
                        type = 1,
                        col = 3,
                        row = 1,
                        general = 500,
                        axisMap = new List<int>{555,11,12345,99},
                        childList = new List<ChildPoint>
                        {
                        new ChildPoint { childName = new List<string>{ "LL1-1" }, childPos = new List<double>{ 10, 20, 30, 0 } },
                        new ChildPoint { childName = new List<string>{ "LL1-2" }, childPos = new List<double>{ 11, 21, 31, 1 } },
                        new ChildPoint { childName = new List<string>{ "LL1-3" }, childPos = new List<double>{ 12, 22, 32, 2 } }
                    }
                },
                new Point
                {
                    name = "LL2",
                    type = 1,
                    col = 1,
                    row = 1,
                    axisMap = new List<int>{1,2,5,6},
                    childList = new List<ChildPoint>
                    {
                        new ChildPoint { childName = new List<string>{ "LL2-1" }, childPos = new List<double>{ 15, 25, 35, 5 } }
                        }
                    }
                },
                ZuZhuangPointList = new List<Point>
                {
                    new Point
                    {
                        name = "ZZ1",
                        type = 0,
                        X = 100,
                        Y = 200,
                        Z = 300,
                        R = 10,
                        axisMap = new List<int>{1,2,5,6},
                    }
                },
                FuJianPointList = new List<Point>
                {
                    new Point
                    {
                        name = "FJ1",
                        type = 1,
                        col = 2,
                        row = 1,
                        axisMap = new List<int>{1,2,5,6},
                    childList = new List<ChildPoint>
                    {
                        new ChildPoint { childName = new List<string>{ "FJ1-1" }, childPos = new List<double>{ 50, 60, 70, 15 } },
                        new ChildPoint { childName = new List<string>{ "FJ1-2" }, childPos = new List<double>{ 51, 61, 71, 16 } }
                    }
                        },
                        new Point
                        {
                            name = "FJ2",
                            type = 1,
                            col = 2,
                            row = 1,
                            axisMap = new List<int>{1,2,5,6},
                            childList = new List<ChildPoint>
                            {
                                new ChildPoint { childName = new List<string>{ "FJ2-1" }, childPos = new List<double>{ 55, 65, 75, 20 } },
                                new ChildPoint { childName = new List<string>{ "FJ2-2" }, childPos = new List<double>{ 56, 66, 76, 21 } }
                            }
                        }
                 },
                RejectPointList = new List<Point>
                {
                    new Point
                    {
                        name = "RJ1",
                        type = 1,
                        col = 3,
                        row = 1,
                        axisMap = new List<int>{1,2,5,6},
                    childList = new List<ChildPoint>
                    {
                        new ChildPoint { childName = new List<string>{ "RJ1-1" }, childPos = new List<double>{ 80, 90, 100, 25 } },
                        new ChildPoint { childName = new List<string>{ "RJ1-2" }, childPos = new List<double>{ 81, 91, 101, 26 } },
                        new ChildPoint { childName = new List<string>{ "RJ1-3" }, childPos = new List<double>{ 82, 92, 102, 27 } }
                            }
                        }
                    }
            };

            // 保存到文件
            bool saveOk = FileHelper.SaveToJson(file, newStats);
            //MessageBox.Show(saveOk ? "The Json point file cannot be found, and the system automatically generates default point data" : "The automatic generation of the Json point file failed");
        }

        private void InitTabs(StationPoints points)
        {
            if (points == null)
            {
                return;
            }
            PosTabControl.Items.Clear();

            AddTabIfHasData("Para 1", points.LaiLiaoPointList);
            AddTabIfHasData("Para 2", points.ZuZhuangPointList);
            AddTabIfHasData("Para 3", points.FuJianPointList);

        }

        private void AddTabIfHasData(string header, List<Point> pointList)
        {
            if (pointList == null || pointList.Count == 0)
                return;

            var tabItem = new TabItem
            {
                Header = header,
                Content = GenerateContentForPoints(pointList)
            };

            PosTabControl.Items.Add(tabItem);
        }

        private void EnsureChildDataValid(ChildPoint child)
        {
            int maxCount = Math.Max(
                child.childName?.Count ?? 0,
                child.childPos?.Count ?? 0
            );

            if (child.childName == null)
                child.childName = new List<string>();
            while (child.childName.Count < maxCount)
                child.childName.Add($"ChildPoint{child.childName.Count + 1}");

            if (child.childPos == null)
                child.childPos = new List<double>();
            while (child.childPos.Count < maxCount)
                child.childPos.Add(0);
        }


        private UIElement GenerateContentForPoints(List<Point> pointList)
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };

            foreach (var pt in pointList)
            {
                if (pt.type == 0)
                {
                    // 单独点，使用 pt 的 X/Y/Z/R
                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Tag = "SinglePoint", Margin = new Thickness(0, 2, 0, 2) };


                    // 添加 ID 标签
                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    });

                    // 添加可编辑的 ID 输入框
                    var idTextBox = new TextBox
                    {
                        Text = pt.name,
                        Width = 100,
                        Margin = new Thickness(0, 0, 15, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    // 注册 TextChanged 事件，将用户输入回写到 pt.name
                    idTextBox.TextChanged += (s, edc) =>
                    {
                        pt.name = idTextBox.Text;
                    };
                    rowPanel.Children.Add(idTextBox);

                    TextBox xBox, yBox, zBox, rBox;

                    rowPanel.Children.Add(CreateLabeledTextBox("X", pt.X,out xBox, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.X = val;
                    }));

                    rowPanel.Children.Add(CreateLabeledTextBox("Y", pt.Y, out yBox, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.Y = val;
                    }));

                    rowPanel.Children.Add(CreateLabeledTextBox("Z", pt.Z, out zBox, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.Z = val;
                    }));

                    rowPanel.Children.Add(CreateLabeledTextBox("R", pt.R, out rBox, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.R = val;
                    }));

                    rowPanel.Children.Add(GreateButton(pt.axisMap, rowPanel));

                    panel.Children.Add(rowPanel);

                }
                else if (pt.type == 1)
                {
                    // 矩阵点，使用 pt.childList 里的每一个 ChildPoint
                    int totalPoints = pt.row * pt.col;

                    if (totalPoints > 200)
                    {
                        panel.Children.Add(new TextBlock
                        {
                            Text = $"{MatrixPointPrefix} {pt.name} 超过最大限制（200 个点），跳过。",
                            Foreground = new SolidColorBrush(Colors.Red)
                        });
                        continue;
                    }

                    // 初始化并填满 childList
                    if (pt.childList == null)
                        pt.childList = new List<ChildPoint>();

                    while (pt.childList.Count < totalPoints)
                    {
                        pt.childList.Add(new ChildPoint
                        {
                            childName = new List<string> { $"Point{pt.childList.Count + 1}" },
                            childPos = new List<double> { 0, 0, 0, 0 }
                        });
                    }

                    // 校验每个子点的内容
                    foreach (var child in pt.childList)
                    {
                        EnsureChildDataValid(child);
                    }

                    // 绘制 UI
                    var rowGrid = new Grid
                    {
                        Margin = new Thickness(0, 8, 0, 4),
                        Tag = "MatrixHeader"  // 关键标记
                    };

                    // 定义三列：标签、输入框、说明文本
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // "ID:"
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) }); // 输入框宽度
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row

                    // ID: 标签
                    var idLabel = new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    };
                    Grid.SetColumn(idLabel, 0);
                    rowGrid.Children.Add(idLabel);

                    // 可编辑的 ID 输入框
                    var matrixIdTextBox = new TextBox
                    {
                        Text = pt.name,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    matrixIdTextBox.TextChanged += (s, e) => pt.name = matrixIdTextBox.Text;
                    Grid.SetColumn(matrixIdTextBox, 1);
                    rowGrid.Children.Add(matrixIdTextBox);

                    // 显示 col × row 信息
                    var matrixInfoText = new TextBlock
                    {
                        Text = $"({pt.col}col × {pt.row}row)",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0)
                    };
                    Grid.SetColumn(matrixInfoText, 2);
                    rowGrid.Children.Add(matrixInfoText);

                    var ButtonAutoData = new Button
                    {
                        Content = "FillData",
                        Margin = new Thickness(8, 0, 0, 0),
                        Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton"),
                    };
                    Grid.SetColumn(ButtonAutoData, 3);

                    rowGrid.Children.Add(ButtonAutoData);

                    // 添加整行到主 panel
                    panel.Children.Add(rowGrid);


                    //添加偏移行
                    var rowOfferGrid = new Grid
                    {
                        Margin = new Thickness(0, 8, 0, 4),
                        Tag = "MatrixHeader"  // 关键标记
                    };

                    // 定义4列：标签、输入框  offer10,offer11
                    rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                    rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                    // Offer10 Label
                    var offer10Label = new TextBlock
                    {
                        Text = "Offer10:",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 6, 0)
                    };
                    Grid.SetColumn(offer10Label, 0);
                    rowOfferGrid.Children.Add(offer10Label);

                    // Offer10 TextBox
                    var offer10Box = new TextBox
                    {
                        Width = 90,
                        Margin = new Thickness(0, 0, 10, 0),
                        Text = pt.offer10.ToString() // 初始化值
                    };
                    offer10Box.TextChanged += (s, e) =>
                    {
                        if (double.TryParse(offer10Box.Text, out double val))
                            pt.offer10 = val;
                    };
                    Grid.SetColumn(offer10Box, 1);
                    rowOfferGrid.Children.Add(offer10Box);

                    // Offer11 Label
                    var offer11Label = new TextBlock
                    {
                        Text = "Offer11:",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(20, 0, 6, 0)
                    };
                    Grid.SetColumn(offer11Label, 2);
                    rowOfferGrid.Children.Add(offer11Label);

                    // Offer11 TextBox
                    var offer11Box = new TextBox
                    {
                        Width = 90,
                        Text = pt.offer11.ToString()
                    };
                    offer11Box.TextChanged += (s, e) =>
                    {
                        if (double.TryParse(offer11Box.Text, out double val))
                            pt.offer11 = val;
                    };
                    Grid.SetColumn(offer11Box, 3);
                    rowOfferGrid.Children.Add(offer11Box);

                    panel.Children.Add(rowOfferGrid);


                    List<List<TextBox[]>> matrixInputs = new List<List<TextBox[]>>(); // 每个点 4 个 TextBox

                    int childIndex = 0;
                    for (int r = 0; r < pt.row; r++)
                    {
                        var rowPanel = new StackPanel
                        {
                            Tag = "MatrixRow",
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 4, 0, 4)
                        };

                        var inputRow = new List<TextBox[]>();

                        for (int c = 0; c < pt.col; c++)
                        {
                            var child = pt.childList[childIndex++];
                            string displayName = child.childName[0];
                            var pos = child.childPos;

                            var pointPanel = new StackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(4),
                                Width = 120,
                                Background = new SolidColorBrush(Colors.LightGray),
                            };

                            pointPanel.Children.Add(new TextBlock
                            {
                                Text = $"ID: {displayName}",
                                Margin = new Thickness(0, 0, 0, 6)
                            });

                            TextBox xBox, yBox, zBox, rBox;

                            //回写，用于保存文件
                            pointPanel.Children.Add(CreateLabeledTextBox("X", pos[0],out xBox, newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[0] = val;
                            }));

                            pointPanel.Children.Add(CreateLabeledTextBox("Y", pos[1], out yBox, newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[1] = val;
                            }));

                            pointPanel.Children.Add(CreateLabeledTextBox("Z", pos[2], out zBox, newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[2] = val;
                            }));

                            pointPanel.Children.Add(CreateLabeledTextBox("R", pos[3], out rBox, newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[3] = val;
                            }));

                            pointPanel.Children.Add(GreateButton(pt.axisMap, pointPanel));

                            rowPanel.Children.Add(pointPanel);

                            inputRow.Add(new[] { xBox, yBox, zBox, rBox });

                        }

                        matrixInputs.Add(inputRow);

                        panel.Children.Add(rowPanel);


                    }
                    ButtonAutoData.Click += (s, e) =>
                    {
                        if (pt.childList.Count != pt.row * pt.col) return;

                        var topLeft = pt.childList[0].childPos;
                        var topRight = pt.childList[pt.col - 1].childPos;
                        var bottomLeft = pt.childList[(pt.row - 1) * pt.col].childPos;

                        double[] vecX = new double[4];
                        double[] vecY = new double[4];
                        for (int i = 0; i < 4; i++)
                        {
                            vecX[i] = (topRight[i] - topLeft[i]) / (pt.col - 1);
                            vecY[i] = (bottomLeft[i] - topLeft[i]) / (pt.row - 1);
                        }

                        int idx = 0;
                        for (int r = 0; r < pt.row; r++)
                        {
                            for (int c = 0; c < pt.col; c++)
                            {
                                var pos = pt.childList[idx++].childPos;

                                pos[0] = topLeft[0] + vecX[0] * c + vecY[0] * r;
                                pos[1] = topLeft[1] + vecX[1] * c + vecY[1] * r;
                                pos[2] = topLeft[2];
                                pos[3] = topLeft[3];

                                var boxes = matrixInputs[r][c];
                                boxes[0].Text = pos[0].ToString("0.###");
                                boxes[1].Text = pos[1].ToString("0.###");
                                boxes[2].Text = pos[2].ToString("0.###");
                                boxes[3].Text = pos[3].ToString("0.###");
                            }
                        }
                    };
                }
                else
                {
                    var rowPanel = new StackPanel
                    {
                        Tag = "SinglePoint",
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 4, 10, 4)
                    };

                    //pt.name = "New";

                    // 添加 ID 标签
                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    });

                    // 添加 ID 输入框（回写 pt.name）
                    var idTextBox = new TextBox
                    {
                        Text = pt.name,
                        Width = 100,
                        Margin = new Thickness(0, 0, 15, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    idTextBox.TextChanged += (s, ede) => pt.name = idTextBox.Text;
                    rowPanel.Children.Add(idTextBox);

                    // 添加 General 标签
                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = "Data:",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    });

                    // 添加 General 输入框（回写 pt.general）
                    var genTextBox = new TextBox
                    {
                        Text = pt.general.ToString(),
                        Width = 90,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    genTextBox.TextChanged += (s, edc) =>
                    {
                        if (double.TryParse(genTextBox.Text, out double val))
                            pt.general = val;
                    };
                    rowPanel.Children.Add(genTextBox);

                    // 添加到主容器
                    panel.Children.Add(rowPanel);
                }
            }

            return new ScrollViewer
            {
                Content = panel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }



        private FrameworkElement CreateLabeledTextBox(string label, double initialValue,out TextBox tb, Action<string> onTextChanged = null)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 10, 2),
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(new TextBlock
            {
                Text = label + ":",
                //Width = 25,
                VerticalAlignment = VerticalAlignment.Center
            });

            var tba = new TextBox
            {
                Width = 90,
                Text = initialValue.ToString()
            };

            tba.PreviewTextInput += FloatTextBox_PreviewTextInput;
            tba.PreviewKeyDown += FloatTextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(tba, FloatTextBox_Pasting);

            // 禁用输入法
            InputMethod.SetIsInputMethodEnabled(tba, false);

            tb = tba;

            // 如果有绑定回调，就在文本变更时触发
            if (onTextChanged != null)
            {
                tba.TextChanged += (s, e) =>
                {
                    onTextChanged(tba.Text);
                };
            }

            panel.Children.Add(tba);

            return panel;
        }

        private Button GreateButton(List<int> axisIndex, StackPanel backSP)
        {
            // 添加 Teaching Point 按钮
            var teachBtn = new Button
            {
                ToolTip = "Teaching point",
                Style = (Style)Application.Current.FindResource("MaterialDesignFloatingActionButton"),
                Width = 25,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 5),
                Content = new PackIcon { Kind = PackIconKind.DebugStepInto, Width = 17, Height = 17 }
            };

            teachBtn.Click += (s, e) =>
            {
                //将X,Y,Z,R轴的对应映射下标传入
                //List<int> ints= new List<int>();
                // 如果已有弹窗存在并还在显示，就关闭它
                if (teachingWindow != null && teachingWindow.IsLoaded)
                {
                    teachingWindow.Close();
                }

                // 创建新的窗口
                teachingWindow = new TeachingWindow(axisIndex);
                teachingWindow.TeachingDataReady += (x, y, z, r) =>
                {
                    Console.WriteLine($"X={x}, Y={y}, Z={z}, R={r}");
                    // 遍历 backSP 中的 TextBox，按顺序赋值 X/Y/Z/R
                    var textBoxes = FindTextBoxes(backSP);
                    if (textBoxes.Count >= 4)
                    {
                        textBoxes[0].Text = x.ToString("F3");
                        textBoxes[1].Text = y.ToString("F3");
                        textBoxes[2].Text = z.ToString("F3");
                        textBoxes[3].Text = r.ToString("F3");
                    }
                };

                teachingWindow.Show();
            };
            return teachBtn;
        }

        private List<TextBox> FindTextBoxes(DependencyObject parent)
        {
            var result = new List<TextBox>();
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox tb)
                {
                    result.Add(tb);
                }
                else
                {
                    result.AddRange(FindTextBoxes(child));
                }
            }
            return result;
        }
        private void RegisterHandlersInContainer(object container)
        {
            if (container is DependencyObject depObj)
            {
                foreach (var child in FindVisualChildren<TextBox>(depObj))
                {
                    child.PreviewTextInput += FloatTextBox_PreviewTextInput;
                    child.PreviewKeyDown += FloatTextBox_PreviewKeyDown;
                    DataObject.AddPastingHandler(child, FloatTextBox_Pasting);

                    // 禁用输入法
                    InputMethod.SetIsInputMethodEnabled(child, false);
                }
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                string newText = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                       .Insert(tb.SelectionStart, e.Text);

                e.Handled = !IsValidFloatInput(newText);
            }
        }

        private void FloatTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidFloatInput(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void FloatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 禁用中文输入法（保险起见）
            InputMethod.Current.ImeState = InputMethodState.Off;
        }

        private bool IsValidFloatInput(string input)
        {
            // 支持合法浮点数格式：123、0.5、.5、123.
            return Regex.IsMatch(input, @"^-?(\d+(\.\d*)?|\.\d+)?$");
        }

        private void CboxNowType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                int selectedIndex = comboBox.SelectedIndex;
                FileHelper.LoadConfig(posFileName[selectedIndex], out stationPoints);   //默认加载第一套参数
                InitTabs(stationPoints);
            }
        }

        private void deletePosParam_Click(object sender, RoutedEventArgs e)
        {
            int listIndex = PosTabControl.SelectedIndex;
            if (PosTabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is ScrollViewer scrollViewer &&
                    scrollViewer.Content is StackPanel mainPanel &&
                    mainPanel.Children.Count > 0)
                {
                    int lastIndex = mainPanel.Children.Count - 1;

                    if (lastIndex < 0)
                    {
                        MessageBox.Show("No deletable points", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (mainPanel.Children[lastIndex] is StackPanel lastPanel &&
                        lastPanel.Tag as string == "SinglePoint")
                    {
                        mainPanel.Children.RemoveAt(lastIndex);
                        ReMoveStationData(listIndex);
                        MessageBox.Show("Single point deleted", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // 查找最后一个矩阵头的位置（Tag == "MatrixHeader"）
                        int matrixHeaderIndex = -1;
                        for (int i = lastIndex; i >= 0; i--)
                        {
                            if (mainPanel.Children[i] is FrameworkElement fe &&
                                fe.Tag as string == "MatrixHeader")
                            {
                                matrixHeaderIndex = i;
                                break;
                            }
                        }

                        if (matrixHeaderIndex >= 0)
                        {
                            // 从矩阵头开始，删除它和后面所有 "MatrixRow"
                            int removeCount = 0;
                            int currentIndex = matrixHeaderIndex;

                            while (currentIndex < mainPanel.Children.Count)
                            {
                                var child = mainPanel.Children[currentIndex] as FrameworkElement;
                                string tag = child?.Tag as string;

                                if (tag == "MatrixHeader" || tag == "MatrixRow")
                                {
                                    mainPanel.Children.RemoveAt(currentIndex);
                                    removeCount++;
                                    // 删除后元素会自动往前移，不要 ++ currentIndex
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (removeCount > 0)
                            {
                                ReMoveStationData(listIndex);
                                MessageBox.Show("Matrix point deleted", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("No matrix rows found to delete", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Unidentified point elements cannot be deleted", "Err", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No deletable points", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a point page first", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<Point> GetPointListByTab(string header)
        {
            if (stationPoints == null)
                return null;

            switch (header)
            {
                case "Para 1":
                    return stationPoints.LaiLiaoPointList;
                case "Para 2":
                    return stationPoints.ZuZhuangPointList;
                case "Para 3":
                    return stationPoints.FuJianPointList;
                default:
                    return null;
            }
        }

        private void ReMoveStationData(int listIndex)
        {
            if (listIndex == 0)
            {
                if (stationPoints.LaiLiaoPointList.Count > 0)
                {
                    stationPoints.LaiLiaoPointList.RemoveAt(stationPoints.LaiLiaoPointList.Count - 1);
                }
            }
            else if (listIndex == 1)
            {
                if (stationPoints.ZuZhuangPointList.Count > 0)
                {
                    stationPoints.ZuZhuangPointList.RemoveAt(stationPoints.ZuZhuangPointList.Count - 1);
                }
            }
            else if (listIndex == 2)
            {
                if (stationPoints.FuJianPointList.Count > 0)
                {
                    stationPoints.FuJianPointList.RemoveAt(stationPoints.FuJianPointList.Count - 1);
                }
            }
        }

        private void AddStationData(int listIndex, Point point)
        {
            if (listIndex == 0)
            {
                stationPoints.LaiLiaoPointList.Add(point);
            }
            else if (listIndex == 1)
            {
                stationPoints.ZuZhuangPointList.Add(point);
            }
            else if (listIndex == 2)
            {
                stationPoints.FuJianPointList.Add(point);
            }
        }

        private void AddPosParam_Click(object sender, RoutedEventArgs e)
        {
            ///TODO 1.选中类型   1.5（设置行列）   2.添加UI     3.维护缓存
            var dlg = new SelectPointType();
            dlg.ShowDialog();
            var data = dlg.SelectedType;
            var row = dlg.SelectedRow;
            var col = dlg.SelectedCol;
            //依次单点，矩阵点，通用点

            int selectIndex = PosTabControl.SelectedIndex;

            if (!(PosTabControl.SelectedItem is TabItem selectedTab &&
                  selectedTab.Content is ScrollViewer scrollViewer &&
                  scrollViewer.Content is StackPanel mainPanel))
            {
                System.Windows.MessageBox.Show("请先选择一个页面", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 获取对应缓存列表
            var pointList = GetPointListByTab(selectedTab.Header.ToString());
            if (pointList == null) return;

            //var pt = pointList[pointList.Count-1];
            Point pt = new Point();
            pt.type = data;
            pt.row = row;
            pt.col = col;
            //点分类
            if (data == 0)
            {
                // 单独点，使用 pt 的 X/Y/Z/R
                var rowPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Tag = "SinglePoint", Margin = new Thickness(0, 2, 0, 2) };

                // 添加 ID 标签
                rowPanel.Children.Add(new TextBlock
                {
                    Text = "ID:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });

                // 添加可编辑的 ID 输入框
                var idTextBox = new TextBox
                {
                    Text = pt.name,
                    Width = 100,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // 注册 TextChanged 事件，将用户输入回写到 pt.name
                idTextBox.TextChanged += (s, edc) =>
                {
                    pt.name = idTextBox.Text;
                };
                rowPanel.Children.Add(idTextBox);

                TextBox xBox, yBox, zBox, rBox;

                rowPanel.Children.Add(CreateLabeledTextBox("X", 0,out xBox, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.X = val;
                }));

                rowPanel.Children.Add(CreateLabeledTextBox("Y", 0, out yBox, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.Y = val;
                }));

                rowPanel.Children.Add(CreateLabeledTextBox("Z", 0, out zBox, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.Z = val;
                }));

                rowPanel.Children.Add(CreateLabeledTextBox("R", 0, out rBox, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.R = val;
                }));

                rowPanel.Children.Add(GreateButton(pt.axisMap, rowPanel));

                mainPanel.Children.Add(rowPanel);

                AddStationData(selectIndex, pt);

            }
            else if (data == 1)
            {
                // 矩阵点，使用 pt.childList 里的每一个 ChildPoint
                int totalPoints = row * col;

                if (totalPoints > 200)
                {
                    mainPanel.Children.Add(new TextBlock
                    {
                        Text = $"{MatrixPointPrefix} New 超过最大限制（200 个点），跳过。",
                        Foreground = new SolidColorBrush(Colors.Red)
                    });
                    return;
                }

                // 初始化并填满 NewchildList   
                if (pt.childList == null)
                    pt.childList = new List<ChildPoint>();

                while (pt.childList.Count < totalPoints)
                {
                    pt.childList.Add(new ChildPoint
                    {
                        childName = new List<string> { $"NewPoint{pt.childList.Count + 1}" },
                        childPos = new List<double> { 0, 0, 0, 0 }
                    });
                }

                // 校验每个子点的内容
                //foreach (var child in pt.childList)
                //{
                //    EnsureChildDataValid(child);
                //}

                // 绘制 UI
                var rowGrid = new Grid
                {
                    Margin = new Thickness(0, 8, 0, 4),
                    Tag = row // 可选：将行数存储在 Tag 中
                };

                // 定义三列：标签、输入框、说明文本
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // "ID:"
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) }); // 输入框宽度
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row

                // ID: 标签
                var idLabel = new TextBlock
                {
                    Text = "ID:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                Grid.SetColumn(idLabel, 0);
                rowGrid.Children.Add(idLabel);

                // 可编辑的 ID 输入框
                var matrixIdTextBox = new TextBox
                {
                    Text = pt.name,
                    VerticalAlignment = VerticalAlignment.Center
                };
                matrixIdTextBox.TextChanged += (s, ede) => pt.name = matrixIdTextBox.Text;
                Grid.SetColumn(matrixIdTextBox, 1);
                rowGrid.Children.Add(matrixIdTextBox);

                // 显示 col × row 信息
                var matrixInfoText = new TextBlock
                {
                    Text = $"({col}col × {row}row)",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(matrixInfoText, 2);
                rowGrid.Children.Add(matrixInfoText);


                var ButtonAutoData = new Button
                {
                    Content = "FillData",
                    Margin = new Thickness(8, 0, 0, 0),
                    Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton"),
                };
                Grid.SetColumn(ButtonAutoData, 3);
                rowGrid.Children.Add(ButtonAutoData);



                //添加偏移行
                var rowOfferGrid = new Grid
                {
                    Margin = new Thickness(0, 8, 0, 4),
                    Tag = "MatrixHeader"  // 关键标记
                };

                // 定义4列：标签、输入框  offer10,offer11
                rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                rowOfferGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                // Offer10 Label
                var offer10Label = new TextBlock
                {
                    Text = "Offer10:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0)
                };
                Grid.SetColumn(offer10Label, 0);
                rowOfferGrid.Children.Add(offer10Label);

                // Offer10 TextBox
                var offer10Box = new TextBox
                {
                    Width = 90,
                    Margin = new Thickness(0, 0, 10, 0),
                    Text = pt.offer10.ToString() // 初始化值
                };
                offer10Box.TextChanged += (s, eoffer) =>
                {
                    if (double.TryParse(offer10Box.Text, out double val))
                        pt.offer10 = val;
                };
                Grid.SetColumn(offer10Box, 1);
                rowOfferGrid.Children.Add(offer10Box);

                // Offer11 Label
                var offer11Label = new TextBlock
                {
                    Text = "Offer11:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 6, 0)
                };
                Grid.SetColumn(offer11Label, 2);
                rowOfferGrid.Children.Add(offer11Label);

                // Offer11 TextBox
                var offer11Box = new TextBox
                {
                    Width = 90,
                    Text = pt.offer11.ToString()
                };
                offer11Box.TextChanged += (s, eoffer) =>
                {
                    if (double.TryParse(offer11Box.Text, out double val))
                        pt.offer11 = val;
                };
                Grid.SetColumn(offer11Box, 3);
                rowOfferGrid.Children.Add(offer11Box);
                

                mainPanel.Children.Add(rowGrid);
                mainPanel.Children.Add(rowOfferGrid);



                List<List<TextBox[]>> matrixInputs = new List<List<TextBox[]>>(); // 每个点 4 个 TextBox

                int childIndex = 0;
                for (int r = 0; r < row; r++)
                {
                    var rowPanel = new StackPanel
                    {
                        Tag = "MatrixRow",
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new Thickness(0, 4, 0, 4)
                    };

                    var inputRow = new List<TextBox[]>();

                    for (int c = 0; c < col; c++)
                    {
                        var child = pt.childList[childIndex++];
                        string displayName = child.childName[0];
                        var pos = child.childPos;

                        var pointPanel = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Vertical,
                            Margin = new Thickness(4),
                            Width = 120,
                            Background = new SolidColorBrush(Colors.LightGray),
                        };

                        pointPanel.Children.Add(new TextBlock
                        {
                            Text = $"ID: {displayName}",
                            Margin = new Thickness(0, 0, 0, 6)
                        });
                        //回写，用于保存文件
                        TextBox xBox, yBox, zBox, rBox;

                        pointPanel.Children.Add(CreateLabeledTextBox("X", pos[0], out xBox, newText => { if (double.TryParse(newText, out double val)) pos[0] = val; }));
                        pointPanel.Children.Add(CreateLabeledTextBox("Y", pos[1], out yBox, newText => { if (double.TryParse(newText, out double val)) pos[1] = val; }));
                        pointPanel.Children.Add(CreateLabeledTextBox("Z", pos[2], out zBox, newText => { if (double.TryParse(newText, out double val)) pos[2] = val; }));
                        pointPanel.Children.Add(CreateLabeledTextBox("R", pos[3], out rBox, newText => { if (double.TryParse(newText, out double val)) pos[3] = val; }));

                        pointPanel.Children.Add(GreateButton(pt.axisMap, pointPanel));

                        rowPanel.Children.Add(pointPanel);

                        inputRow.Add(new[] { xBox, yBox, zBox, rBox });

                    }

                    matrixInputs.Add(inputRow);

                    mainPanel.Children.Add(rowPanel);
                }

                ButtonAutoData.Click += (s, ebtn) =>
                {
                    if (pt.childList.Count != pt.row * pt.col) return;

                    var topLeft = pt.childList[0].childPos;
                    var topRight = pt.childList[pt.col - 1].childPos;
                    var bottomLeft = pt.childList[(pt.row - 1) * pt.col].childPos;

                    double[] vecX = new double[4];
                    double[] vecY = new double[4];
                    for (int i = 0; i < 4; i++)
                    {
                        vecX[i] = (topRight[i] - topLeft[i]) / (pt.col - 1);
                        vecY[i] = (bottomLeft[i] - topLeft[i]) / (pt.row - 1);
                    }

                    int idx = 0;
                    for (int r = 0; r < pt.row; r++)
                    {
                        for (int c = 0; c < pt.col; c++)
                        {
                            var pos = pt.childList[idx++].childPos;

                            pos[0] = topLeft[0] + vecX[0] * c + vecY[0] * r;
                            pos[1] = topLeft[1] + vecX[1] * c + vecY[1] * r;
                            pos[2] = topLeft[2];
                            pos[3] = topLeft[3];

                            var boxes = matrixInputs[r][c];
                            boxes[0].Text = pos[0].ToString("0.###");
                            boxes[1].Text = pos[1].ToString("0.###");
                            boxes[2].Text = pos[2].ToString("0.###");
                            boxes[3].Text = pos[3].ToString("0.###");
                        }
                    }
                };

                AddStationData(selectIndex, pt);
            }
            else if (data == 2)
            {
                var rowPanel = new StackPanel
                {
                    Tag = "SinglePoint",
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 10, 4)
                };

                //pt.name = "New";

                // 添加 ID 标签
                rowPanel.Children.Add(new TextBlock
                {
                    Text = "ID:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                });

                // 添加 ID 输入框（回写 pt.name）
                var idTextBox = new TextBox
                {
                    Text = pt.name,
                    Width = 100,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                idTextBox.TextChanged += (s, ede) => pt.name = idTextBox.Text;
                rowPanel.Children.Add(idTextBox);

                // 添加 General 标签
                rowPanel.Children.Add(new TextBlock
                {
                    Text = "Data:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                });

                // 添加 General 输入框（回写 pt.general）
                var genTextBox = new TextBox
                {
                    Width = 90,
                    VerticalAlignment = VerticalAlignment.Center
                };
                genTextBox.TextChanged += (s, edc) =>
                {
                    if (double.TryParse(genTextBox.Text, out double val))
                        pt.general = val;
                };
                rowPanel.Children.Add(genTextBox);

                // 添加到主容器
                mainPanel.Children.Add(rowPanel);
                AddStationData(selectIndex, pt);
            }


        }

        private bool SaveAllTabsData(string jsonFilePath)
        {

            return FileHelper.SaveToJson(jsonFilePath, stationPoints);
        }

        private void SavePosParam_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = CboxNowType.SelectedIndex;
            string jsonFile = posFileName[selectedIndex];
            if (SaveAllTabsData(jsonFile))
                MessageBox.Show("保存相机点位成功" + posFilePre[selectedIndex]);
            else
                MessageBox.Show("保存相机点位失败" + posFilePre[selectedIndex]);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox?.IsChecked == true)
            {
                if (TeachingPage != null)
                {
                    TeachingPage.Visibility = Visibility;
                    // 恢复右列宽度
                    CameraGrid.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Star);
                }
            }
            else
            {
                if (TeachingPage != null)
                {
                    TeachingPage.Visibility = Visibility.Collapsed;
                    // 右列宽度设为 0
                    CameraGrid.ColumnDefinitions[1].Width = new GridLength(0);
                }
            }
        }

    }
}
