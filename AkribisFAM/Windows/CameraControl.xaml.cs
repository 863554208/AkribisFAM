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
using AkribisFAM.Manager;
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
    /// function
    /// 1.camera calibration
    ///     11 points for nozzle2,3,4
    ///     9 points for feeder and conveyor(pallet)
    ///     joint calibration calibrates motion from pallet to feeder on nozzle1
    ///     nozzle train for nozzle1,2,3,4 if change the foam
    ///     cam3 calibration
    ///     universal calibration after all calibration
    /// </summary>
    public partial class CameraControl : UserControl
    {
        private bool Calibstatus_Click;
        List<string> posFilePre = new List<string>();
        List<string> posFileName = new List<string>();

        private const string MatrixPointPrefix = "Camera Matrix Points:";
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
            posFilePre.Add("Camera_points1.json");//11 points calibration points, 1-11 for nozzle2, 12-22 for nozzle3, 23-33 for nozzle4
            posFilePre.Add("Camera_points2.json");//9 points calibration points, 1-9 for feeder, 10-18 for conveyor
            posFilePre.Add("Camera_points3.json");//nozzle train points, 5 is fly photograph start point, 1-4 is photograph position for nozzle1,2,3,4
            posFilePre.Add("Camera_points4.json");//joint calibration points, 1-11 for nozzle1 11 points calibration, 12-18 move points in calibration process
            posFilePre.Add("Camera_points5.json");//reserve

            for (int z = 0; z < posFilePre.Count; z++)
            {
                string nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(posFilePre[z]);
                CboxNowType.Items.Add(nameWithoutExtension);
            }
            CboxNowType.SelectedIndex = 0;
            CboxNowType.SelectionChanged += CboxNowType_SelectionChanged;

            // 读取数据并生成 UI
            //read json and generate UI
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
            FileHelper.LoadConfig(posFileName[0], out stationPoints);   //默认加载第一套参数; load the first json file by default
            InitTabs(stationPoints);
            //END ADD

        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Cam3Calibbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => CamerCalibProcess.Instance.ReCheckCalibration());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Universal Calibration Failed!" + ex.Message);
            }
        }

        private async void UniversalCalibbtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Universal Calibration?", "Confirming", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                try
                {
                    await Task.Run(() => CamerCalibProcess.Instance.AllCalibrationFinished()); 
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

        //train nozzle
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
                if (CamerCalibProcess.Instance.CalibrationPoints.ZuZhuangPointList.Count != 18)
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

            AddTabIfHasData("Station 1", points.LaiLiaoPointList);
            AddTabIfHasData("Station 2", points.ZuZhuangPointList);
            AddTabIfHasData("Station 3", points.FuJianPointList);

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
                    // single point uses X/Y/Z/R
                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Tag = "SinglePoint", Margin = new Thickness(0, 2, 0, 2) };


                    // 添加 ID 标签
                    var tbID = new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    rowPanel.Children.Add(tbID);

                    addTextBlockClicked(tbID,pt.axisMap, rowPanel);   //点击ID弹出示教


                    // 添加可编辑的 ID 输入框
                    // add textbox
                    var idTextBox = new TextBox
                    {
                        Text = pt.name,
                        Width = 160,
                        Margin = new Thickness(0, 0, 15, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    // 注册 TextChanged 事件，将用户输入回写到 pt.name
                    // register TextChanged event, set user change
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

                    //点击ID，显示按钮
                    var btnTea = GreateButton(pt.axisMap, rowPanel);
                    rowPanel.Children.Add(btnTea);

                    panel.Children.Add(rowPanel);

                }
                else if (pt.type == 1)
                {
                    // 矩阵点，使用 pt.childList 里的每一个 ChildPoint
                    // matrix points
                    int totalPoints = pt.row * pt.col;

                    if (totalPoints > 200)
                    {
                        panel.Children.Add(new TextBlock
                        {
                            Text = $"{MatrixPointPrefix} {pt.name} Exceeding the maximum limit（200 points），Skip!",//over 200 points limitation
                            Foreground = new SolidColorBrush(Colors.Red)
                        });
                        continue;
                    }

                    // 初始化并填满 childList
                    // init ChildPoint list
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
                    //Verify the content of childList
                    foreach (var child in pt.childList)
                    {
                        EnsureChildDataValid(child);
                    }

                    // 绘制 UI
                    // draw UI
                    var rowGrid = new Grid
                    {
                        Margin = new Thickness(0, 8, 0, 4),
                        Tag = "MatrixHeader"  
                    };

                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // "ID:"
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // 输入框宽度
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row

                    var idLabel = new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    };
                    Grid.SetColumn(idLabel, 0);
                    rowGrid.Children.Add(idLabel);

                    var matrixIdTextBox = new TextBox
                    {
                        Text = pt.name,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    matrixIdTextBox.TextChanged += (s, e) => pt.name = matrixIdTextBox.Text;
                    Grid.SetColumn(matrixIdTextBox, 1);
                    rowGrid.Children.Add(matrixIdTextBox);

                    //  col × row 
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
                        ToolTip = "Add the top left, top right and bottom left points to fillData",
                        Margin = new Thickness(8, 0, 0, 0),
                        Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton"),
                    };
                    Grid.SetColumn(ButtonAutoData, 3);

                    rowGrid.Children.Add(ButtonAutoData);

                    panel.Children.Add(rowGrid);


                    var rowOfferGrid = new Grid
                    {
                        Margin = new Thickness(0, 8, 0, 4),
                        Tag = "MatrixRow"  // 关键标记
                    };

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
                        Tag = "MatrixRow",
                        Width = 90,
                        Margin = new Thickness(0, 0, 10, 0),
                        Text = pt.offer10.ToString()
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
                        Tag = "MatrixRow",
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


                    List<List<TextBox[]>> matrixInputs = new List<List<TextBox[]>>(); // each point has 4 TextBoxes

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

                            // 默认背景颜色
                            Brush background = new SolidColorBrush(Colors.LightGray);
                            // 是否需要设置角点高亮（至少 2行2列）
                            bool highlightCorners = pt.row >= 2 && pt.col >= 2;
                            // 三个角点统一高亮为 #E0E0E0
                            bool isCorner = highlightCorners && (
                                (r == 0 && c == 0) ||                            // 左上角 C
                                (r == pt.row - 1 && c == 0) ||                   // 左下角 A
                                (r == pt.row - 1 && c == pt.col - 1)             // 右下角 B
                            );

                            if (isCorner)
                            {
                                background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B2828282"));
                            }

                            var pointPanel = new StackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(4),
                                Width = 120,
                                Background = background,
                                Tag = "MatrixRow",
                            };

                            var tbID = new TextBlock
                            {
                                Text = $"ID: {displayName}",
                                Margin = new Thickness(0, 0, 0, 6)
                            };
                            pointPanel.Children.Add(tbID);
                            addTextBlockClicked(tbID, pt.axisMap, pointPanel);   //点击ID弹出示教

                            TextBox xBox, yBox, zBox, rBox;

                            //回写，用于保存文件
                            //use to save file
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

                        // 三个角的坐标（行列）
                        int ax = 0, ay = pt.row - 1;        // 左下角 A → (2, 0)
                        int dx = pt.col - 1, dy = pt.row - 1; // 右下角 D → (2, 2)
                        int cx = 0, cy = 0;                 // 左上角 C → (0, 0)

                        var A = pt.childList[ay * pt.col + ax].childPos; // 左下
                        var D = pt.childList[dy * pt.col + dx].childPos; // 右下
                        var C = pt.childList[cy * pt.col + cx].childPos; // 左上

                        // vecX: A -> D 水平方向向量
                        // vecY: A -> C 垂直方向向量
                        double[] vecX = new double[2];
                        double[] vecY = new double[2];

                        vecX[0] = (D[0] - A[0]) / (pt.col - 1); // X 水平方向单步增量
                        vecX[1] = (D[1] - A[1]) / (pt.col - 1); // Y 水平方向单步增量

                        vecY[0] = (C[0] - A[0]) / (pt.row - 1); // X 垂直方向单步增量
                        vecY[1] = (C[1] - A[1]) / (pt.row - 1); // Y 垂直方向单步增量

                        Console.WriteLine($"vecX: ({vecX[0]:0.####}, {vecX[1]:0.####})");
                        Console.WriteLine($"vecY: ({vecY[0]:0.####}, {vecY[1]:0.####})");

                        Console.WriteLine($"A (左下): {A[0]}, {A[1]}, {A[2]}, {A[3]}");
                        Console.WriteLine($"D (右下): {D[0]}, {D[1]}, {D[2]}, {D[3]}");
                        Console.WriteLine($"C (左上): {C[0]}, {C[1]}, {C[2]}, {C[3]}");

                        for (int r = 0; r < pt.row; r++)
                        {
                            for (int c = 0; c < pt.col; c++)
                            {
                                int index = r * pt.col + c; // 保持顺序一致：从上到下、从左到右

                                // 跳过已设置的三个角点
                                if ((r == ay && c == ax) || (r == dy && c == dx) || (r == cy && c == cx))
                                {
                                    Console.WriteLine($"跳过基准点[{r},{c}]");
                                    continue;
                                }

                                var pos = pt.childList[index].childPos;

                                // 用 (pt.row - 1 - r) 来反转行，保证点[0,*]是左上角行
                                pos[0] = A[0] + vecX[0] * c + vecY[0] * (pt.row - 1 - r); // X
                                pos[1] = A[1] + vecX[1] * c + vecY[1] * (pt.row - 1 - r); // Y
                                pos[2] = A[2]; // Z 保持不变
                                pos[3] = A[3]; // R 保持不变

                                var boxes = matrixInputs[r][c];
                                boxes[0].Text = pos[0].ToString("F3");
                                boxes[1].Text = pos[1].ToString("F3");
                                boxes[2].Text = pos[2].ToString("F3");
                                boxes[3].Text = pos[3].ToString("F3");

                                Console.WriteLine($"点[{r},{c}]: X={pos[0]:0.###}, Y={pos[1]:0.###}, Z={pos[2]}, R={pos[3]}");
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
                    var tbID = new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    };
                    rowPanel.Children.Add(tbID);


                    // 添加 ID 输入框（回写 pt.name）
                    var idTextBox = new TextBox
                    {
                        Text = pt.name,
                        Width = 150,
                        Margin = new Thickness(0, 0, 15, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    idTextBox.TextChanged += (s, ede) => pt.name = idTextBox.Text;
                    rowPanel.Children.Add(idTextBox);

                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = "Data:",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    });

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
            //limit input
            InputMethod.SetIsInputMethodEnabled(tba, false);

            tb = tba;

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
                Visibility= Visibility.Collapsed,
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

                    if (backSP.Tag.ToString() == "SinglePoint")
                    {
                        if (textBoxes.Count >= 5)
                        {
                            textBoxes[1].Text = x.ToString("F3");
                            textBoxes[2].Text = y.ToString("F3");
                            textBoxes[3].Text = z.ToString("F3");
                            textBoxes[4].Text = r.ToString("F3");
                        }
                    }
                    else
                    {
                        if (textBoxes.Count >= 4)
                        {
                            textBoxes[0].Text = x.ToString("F3");
                            textBoxes[1].Text = y.ToString("F3");
                            textBoxes[2].Text = z.ToString("F3");
                            textBoxes[3].Text = r.ToString("F3");
                        }
                    }
                };

                teachingWindow.Show();
            };
            return teachBtn;
        }

        private void addTextBlockClicked(TextBlock textBlock,List<int> axisIndex, StackPanel backSP)
        {
            textBlock.MouseLeftButtonDown += (s, e) =>
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

                    if (backSP.Tag.ToString() == "SinglePoint")
                    {
                        if (textBoxes.Count >= 5)
                        {
                            textBoxes[1].Text = x.ToString("F3");
                            textBoxes[2].Text = y.ToString("F3");
                            textBoxes[3].Text = z.ToString("F3");
                            textBoxes[4].Text = r.ToString("F3");
                        }
                    }
                    else
                    {
                        if (textBoxes.Count >= 4)
                        {
                            textBoxes[0].Text = x.ToString("F3");
                            textBoxes[1].Text = y.ToString("F3");
                            textBoxes[2].Text = z.ToString("F3");
                            textBoxes[3].Text = r.ToString("F3");
                        }
                    }
                };

                teachingWindow.Show();
            };
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

        //limit input
        private void FloatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputMethod.Current.ImeState = InputMethodState.Off;
        }

        private bool IsValidFloatInput(string input)
        {
            return Regex.IsMatch(input, @"^-?(\d+(\.\d*)?|\.\d+)?$");
        }

        private void CboxNowType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                int selectedIndex = comboBox.SelectedIndex;
                FileHelper.LoadConfig(posFileName[selectedIndex], out stationPoints);  
                InitTabs(stationPoints);
            }
        }

        //delete position point
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
                            int currentIndex = matrixHeaderIndex;

                            while (currentIndex < mainPanel.Children.Count)
                            {
                                var child = mainPanel.Children[currentIndex] as FrameworkElement;
                                string tag = child?.Tag as string;

                                if (tag == "MatrixHeader" || tag == "MatrixRow")
                                {
                                    mainPanel.Children.RemoveAt(currentIndex);
                                }
                                else
                                {
                                    break;
                                }
                            }

                                ReMoveStationData(listIndex);
                                MessageBox.Show("Matrix point deleted", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
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


            //ReMoveStationData(PosTabControl.SelectedIndex);

            //int selectedIndex = CboxNowType.SelectedIndex;
            //FileHelper.SaveToJson(posFileName[selectedIndex],stationPoints);
            //FileHelper.LoadConfig(posFileName[selectedIndex],out stationPoints);   
            //InitTabs(stationPoints);
        }

        private List<Point> GetPointListByTab(string header)
        {
            if (stationPoints == null)
                return null;

            switch (header)
            {
                case "Station 1":
                    return stationPoints.LaiLiaoPointList;
                case "Station 2":
                    return stationPoints.ZuZhuangPointList;
                case "Station 3":
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

        //add position point
        private void AddPosParam_Click(object sender, RoutedEventArgs e)
        {

            var dlg = new SelectPointType();
            dlg.ShowDialog();
            var data = dlg.SelectedType;
            var row = dlg.SelectedRow;
            var col = dlg.SelectedCol;

            int selectIndex = PosTabControl.SelectedIndex;

            if (!(PosTabControl.SelectedItem is TabItem selectedTab &&
                  selectedTab.Content is ScrollViewer scrollViewer &&
                  scrollViewer.Content is StackPanel mainPanel))
            {
                System.Windows.MessageBox.Show("Please select a page first", "Tips", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var pointList = GetPointListByTab(selectedTab.Header.ToString());
            if (pointList == null) return;

            //var pt = pointList[pointList.Count-1];
            Point pt = new Point();
            pt.type = data;
            pt.row = row;
            pt.col = col;
            pt.axisMap = dlg.AxexIndexList;   //将轴映射保存

            if (data == 0)
            {
                var rowPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Tag = "SinglePoint", Margin = new Thickness(0, 2, 0, 2) };

                // 添加 ID 标签
                var tbID = new TextBlock
                {
                    Text = "ID:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                rowPanel.Children.Add(tbID);
                addTextBlockClicked(tbID, pt.axisMap, rowPanel);   //点击ID弹出示教


                var idTextBox = new TextBox
                {
                    Text = pt.name,
                    Width = 150,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

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
                int totalPoints = row * col;

                if (totalPoints > 200)
                {
                    mainPanel.Children.Add(new TextBlock
                    {
                        Text = $"{MatrixPointPrefix} New Exceeding the maximum limit（200 points），Skip!",
                        Foreground = new SolidColorBrush(Colors.Red)
                    });
                    return;
                }

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

                var rowGrid = new Grid
                {
                    Margin = new Thickness(0, 8, 0, 4),
                    Tag = "MatrixHeader"  // 关键标记
                };

                // 定义三列：标签、输入框、说明文本
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // "ID:"
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // 输入框宽度
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // col×row

                var idLabel = new TextBlock
                {
                    Text = "ID:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                Grid.SetColumn(idLabel, 0);
                rowGrid.Children.Add(idLabel);

                var matrixIdTextBox = new TextBox
                {
                    Text = pt.name,
                    VerticalAlignment = VerticalAlignment.Center
                };
                matrixIdTextBox.TextChanged += (s, ede) => pt.name = matrixIdTextBox.Text;
                Grid.SetColumn(matrixIdTextBox, 1);
                rowGrid.Children.Add(matrixIdTextBox);

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
                    ToolTip = "Add the top left, top right and bottom left points to fillData",
                    Margin = new Thickness(8, 0, 0, 0),
                    Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton"),
                };
                Grid.SetColumn(ButtonAutoData, 3);
                rowGrid.Children.Add(ButtonAutoData);



                var rowOfferGrid = new Grid
                {
                    Margin = new Thickness(0, 8, 0, 4),
                    Tag = "MatrixRow"  // 关键标记
                };

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
                    Text = pt.offer10.ToString() 
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



                List<List<TextBox[]>> matrixInputs = new List<List<TextBox[]>>(); 

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

                        // 默认背景颜色
                        Brush background = new SolidColorBrush(Colors.LightGray);
                        // 是否需要设置角点高亮（至少 2行2列）
                        bool highlightCorners = pt.row >= 2 && pt.col >= 2;
                        // 三个角点统一高亮为 #E0E0E0
                        bool isCorner = highlightCorners && (
                            (r == 0 && c == 0) ||                            // 左上角 C
                            (r == pt.row - 1 && c == 0) ||                   // 左下角 A
                            (r == pt.row - 1 && c == pt.col - 1)             // 右下角 B
                        );

                        if (isCorner)
                        {
                            background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B2828282"));
                        }

                        var pointPanel = new StackPanel
                        {
                            Tag = "MatrixRow",
                            Orientation = System.Windows.Controls.Orientation.Vertical,
                            Margin = new Thickness(4),
                            Width = 120,
                            Background = background,
                        };

                        var tbID = new TextBlock
                        {
                            Text = $"ID: {displayName}",
                            Margin = new Thickness(0, 0, 0, 6)
                        };
                        pointPanel.Children.Add(tbID);
                        addTextBlockClicked(tbID, pt.axisMap, pointPanel);   //点击ID弹出示教


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

                    // 三个角的坐标（行列）
                    int ax = 0, ay = pt.row - 1;        // 左下角 A → (2, 0)
                    int dx = pt.col - 1, dy = pt.row - 1; // 右下角 D → (2, 2)
                    int cx = 0, cy = 0;                 // 左上角 C → (0, 0)

                    var A = pt.childList[ay * pt.col + ax].childPos; // 左下
                    var D = pt.childList[dy * pt.col + dx].childPos; // 右下
                    var C = pt.childList[cy * pt.col + cx].childPos; // 左上

                    // vecX: A -> D 水平方向向量
                    // vecY: A -> C 垂直方向向量
                    double[] vecX = new double[2];
                    double[] vecY = new double[2];

                    vecX[0] = (D[0] - A[0]) / (pt.col - 1); // X 水平方向单步增量
                    vecX[1] = (D[1] - A[1]) / (pt.col - 1); // Y 水平方向单步增量

                    vecY[0] = (C[0] - A[0]) / (pt.row - 1); // X 垂直方向单步增量
                    vecY[1] = (C[1] - A[1]) / (pt.row - 1); // Y 垂直方向单步增量

                    Console.WriteLine($"vecX: ({vecX[0]:0.####}, {vecX[1]:0.####})");
                    Console.WriteLine($"vecY: ({vecY[0]:0.####}, {vecY[1]:0.####})");

                    Console.WriteLine($"A (左下): {A[0]}, {A[1]}, {A[2]}, {A[3]}");
                    Console.WriteLine($"D (右下): {D[0]}, {D[1]}, {D[2]}, {D[3]}");
                    Console.WriteLine($"C (左上): {C[0]}, {C[1]}, {C[2]}, {C[3]}");

                    for (int r = 0; r < pt.row; r++)
                    {
                        for (int c = 0; c < pt.col; c++)
                        {
                            int index = r * pt.col + c; // 保持顺序一致：从上到下、从左到右

                            // 跳过已设置的三个角点
                            if ((r == ay && c == ax) || (r == dy && c == dx) || (r == cy && c == cx))
                            {
                                Console.WriteLine($"跳过基准点[{r},{c}]");
                                continue;
                            }

                            var pos = pt.childList[index].childPos;

                            // 用 (pt.row - 1 - r) 来反转行，保证点[0,*]是左上角行
                            pos[0] = A[0] + vecX[0] * c + vecY[0] * (pt.row - 1 - r); // X
                            pos[1] = A[1] + vecX[1] * c + vecY[1] * (pt.row - 1 - r); // Y
                            pos[2] = A[2]; // Z 保持不变
                            pos[3] = A[3]; // R 保持不变

                            var boxes = matrixInputs[r][c];
                            boxes[0].Text = pos[0].ToString("F3");
                            boxes[1].Text = pos[1].ToString("F3");
                            boxes[2].Text = pos[2].ToString("F3");
                            boxes[3].Text = pos[3].ToString("F3");

                            Console.WriteLine($"点[{r},{c}]: X={pos[0]:0.###}, Y={pos[1]:0.###}, Z={pos[2]}, R={pos[3]}");
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


                // 添加 ID 标签
                var tbID = new TextBlock
                {
                    Text = "ID:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                rowPanel.Children.Add(tbID);

                var idTextBox = new TextBox
                {
                    Text = pt.name,
                    Width = 150,
                    Margin = new Thickness(0, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                idTextBox.TextChanged += (s, ede) => pt.name = idTextBox.Text;
                rowPanel.Children.Add(idTextBox);

                rowPanel.Children.Add(new TextBlock
                {
                    Text = "Data:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                });

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
                MessageBox.Show("Save success" + posFilePre[selectedIndex]);
            else
                MessageBox.Show("Save failed" + posFilePre[selectedIndex]);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox?.IsChecked == true)
            {
                if (TeachingPage != null)
                {
                    TeachingPage.Visibility = Visibility;
                    CameraGrid.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Star);
                }
            }
            else
            {
                if (TeachingPage != null)
                {
                    TeachingPage.Visibility = Visibility.Collapsed;
                    CameraGrid.ColumnDefinitions[1].Width = new GridLength(0);
                }
            }
        }

    }
}
