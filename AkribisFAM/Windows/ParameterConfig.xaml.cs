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
using System.Runtime.CompilerServices;
using LiveCharts;
using System.Threading;
using MaterialDesignThemes.Wpf;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Xml.Linq;
using AkribisFAM.Helper;
using System.Windows.Threading;
using System.Runtime.ConstrainedExecution;


namespace AkribisFAM.Windows
{
    /// <summary>
    /// ParameterConfig.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterConfig : UserControl
    {
        private string[] axisarray = new string[] {
        "LSX",
        "LSY",
        "FSX",
        "FSY",
        "BL5",
        "BR5",
        "BL1",
        "BL2",
        "BL3",
        "BL4",
        "BR1",
        "BR2",
        "BR3",
        "BR4",
        "PICK1_Z",
        "PICK1_T",
        "PICK2_Z",
        "PICK2_T",
        "PICK3_Z",
        "PICK3_T",
        "PICK4_Z",
        "PICK4_T",
        "PRX",
        "PRY",
        "PRZ"
        };

        List<string> posFilePre = new List<string>();
        List<string> posFileName = new List<string>();

        private const string MatrixPointPrefix = "矩阵点:";
        TeachingWindow teachingWindow;

        private StationPoints stationPoints = new StationPoints();
        public StationPoints StationPoints
        {
            get { return stationPoints; }
            set { stationPoints = value; }
        }

        public ParameterConfig()
        {
            InitializeComponent();
            ReadAxisParamJson();

            //Add by yxw
            posFilePre.Add("Station_points1.json");
            posFilePre.Add("Station_points2.json");
            posFilePre.Add("Station_points3.json");
            posFilePre.Add("Station_points4.json");
            posFilePre.Add("Station_points5.json");

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
            FileHelper.LoadConfig(posFileName[0], out stationPoints);
            //20250523 点位从json文件里读取 【史彦洋】 Start

            LoadPoints();
            //20250523 点位从json文件里读取 【史彦洋】 Start
            InitTabs(stationPoints);


            //END ADD
        }
        private static void Clear()
        {

            GlobalManager.Current.laserPoints.Clear();
            GlobalManager.Current.feedar1Points.Clear();
            GlobalManager.Current.feedar2Points.Clear();
            GlobalManager.Current.pickFoam1Points.Clear();
            GlobalManager.Current.pickFoam2Points.Clear();
            GlobalManager.Current.lowerCCDPoints.Clear();
            GlobalManager.Current.dropBadFoamPoints.Clear();
            GlobalManager.Current.snapPalletePoints.Clear();
            GlobalManager.Current.placeFoamPoints.Clear();
            GlobalManager.Current.recheckPoints.Clear();
            GlobalManager.Current.tearingPoints.Clear();

            //GlobalManager.Current.BarcodeQueue.Clear();
        }
        public static void LoadPoints()
        {
            Clear();
            List<string> posFilePre = new List<string>();
            List<string> posFileName = new List<string>();

            posFilePre.Add("Station_points1.json");
            posFilePre.Add("Station_points2.json");
            posFilePre.Add("Station_points3.json");
            posFilePre.Add("Station_points4.json");
            posFilePre.Add("Station_points5.json");

            for (int i = 0; i < posFilePre.Count; i++)
            {
                string jsonFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, posFilePre[i]);
                posFileName.Add(jsonFile);
                if (string.IsNullOrEmpty(jsonFile) || !File.Exists(jsonFile))
                {
                }
            }

            FileHelper.LoadConfig(posFileName[0], out GlobalManager.Current.stationPoints);
            foreach (var Node in GlobalManager.Current.stationPoints.LaiLiaoPointList)
            {
                if (Node.name != null && Node.name.Equals("laserpoint1_shift_X"))
                {
                    GlobalManager.Current.laserpoint1_shift_X = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("laserpoint1_shift_Y"))
                {
                    GlobalManager.Current.laserpoint1_shift_Y = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("laserpoint2_shift_X"))
                {
                    GlobalManager.Current.laserpoint2_shift_X = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("laserpoint2_shift_Y"))
                {
                    GlobalManager.Current.laserpoint2_shift_Y = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("laserpoint3_shift_X"))
                {
                    GlobalManager.Current.laserpoint3_shift_X = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("laserpoint3_shift_Y"))
                {
                    GlobalManager.Current.laserpoint3_shift_Y = Convert.ToInt32(Node.general);
                }


                if (Node.name != null && Node.name.Equals("Laser Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.laserPoints.Add(temp);
                    }
                }
            }

            foreach (var Node in GlobalManager.Current.stationPoints.ZuZhuangPointList)
            {
                if (Node.name != null && Node.name.Equals("NozzleGap_X"))
                {
                    GlobalManager.Current.NozzleGap_X = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("PalleteGap_X"))
                {
                    GlobalManager.Current.PalleteGap_X = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("PalleteGap_Y"))
                {
                    GlobalManager.Current.PalleteGap_Y = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TotalRow"))
                {
                    GlobalManager.Current.TotalRow = Convert.ToInt32(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TotalColumn"))
                {
                    GlobalManager.Current.TotalColumn = Convert.ToInt32(Node.general);
                }

                if (Node.name != null && Node.name.Equals("Snap Feedar1 Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.feedar1Points.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Snap Feedar2 Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.feedar2Points.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Feedar1 PickFoam Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.pickFoam1Points.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Feedar2 PickFoam Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.pickFoam2Points.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Snap LowerCCD Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.lowerCCDPoints.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("DropBadFoam Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.dropBadFoamPoints.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("SnapPallete Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.snapPalletePoints.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("PlaceFoam Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.placeFoamPoints.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Pickers_ZPickPos"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            Z = pointList.childPos[2]
                        };
                        GlobalManager.Current.pickerZPickPoints.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Pickers_ZCam2Pos"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            Z = pointList.childPos[2]
                        };
                        GlobalManager.Current.pickerZCam2Points.Add(temp);
                    }
                }

                if (Node.name != null && Node.name.Equals("Pickers_ZSafePos"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            Z = pointList.childPos[2]
                        };
                        GlobalManager.Current.pickerZSafePoints.Add(temp);
                    }
                }
                if (Node.name != null && Node.name.Equals("Pickers_LoadCell"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.pickerLoadCellPoints.Add(temp);
                    }
                }
            }

            foreach (var Node in GlobalManager.Current.stationPoints.FuJianPointList)
            {
                if (Node.name != null && Node.name.Equals("Tearing Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.tearingPoints.Add(temp);
                    }
                }
                if (Node.name != null && Node.name.Equals("Recheck Points"))
                {
                    foreach (var pointList in Node.childList)
                    {
                        SinglePoint temp = new SinglePoint()
                        {
                            X = pointList.childPos[0],
                            Y = pointList.childPos[1],
                            Z = pointList.childPos[2],
                            R = pointList.childPos[3]
                        };
                        GlobalManager.Current.recheckPoints.Add(temp);
                    }
                }
                if (Node.name != null && Node.name.Equals("Recycle Point"))
                {
                    GlobalManager.Current.RecheckRecylePos.X = Node.X;
                    GlobalManager.Current.RecheckRecylePos.Y = Node.Y;
                    GlobalManager.Current.RecheckRecylePos.Z = Node.Z;
                    GlobalManager.Current.RecheckRecylePos.R = Node.R;
                }
                if (Node.name != null && Node.name.Equals("Zliftup Point"))
                {
                    GlobalManager.Current.SafeZPos.X = Node.X;
                    GlobalManager.Current.SafeZPos.Y = Node.Y;
                    GlobalManager.Current.SafeZPos.Z = Node.Z;
                    GlobalManager.Current.SafeZPos.R = Node.R;
                }
                if (Node.name != null && Node.name.Equals("StartPoint"))
                {
                    GlobalManager.Current.StartPoint.X = Node.X;
                    GlobalManager.Current.StartPoint.Y = Node.Y;
                    GlobalManager.Current.StartPoint.Z = Node.Z;
                    GlobalManager.Current.StartPoint.R = Node.R;
                }
                if (Node.name != null && Node.name.Equals("TearX"))
                {
                    GlobalManager.Current.TearX = Convert.ToDouble(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TearY"))
                {
                    GlobalManager.Current.TearY = Convert.ToDouble(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TearZ"))
                {
                    GlobalManager.Current.TearZ = Convert.ToDouble(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TearXvel"))
                {
                    GlobalManager.Current.TearXvel = Convert.ToDouble(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TearYvel"))
                {
                    GlobalManager.Current.TearYvel = Convert.ToDouble(Node.general);
                }
                if (Node.name != null && Node.name.Equals("TearZvel"))
                {
                    GlobalManager.Current.TearZvel = Convert.ToDouble(Node.general);
                }
            }
            var c = GlobalManager.Current.laserPoints;
            var a = GlobalManager.Current.feedar1Points;
            var b = GlobalManager.Current.feedar2Points;
            var d = GlobalManager.Current.pickFoam1Points;
            var e = GlobalManager.Current.lowerCCDPoints;
            var f = GlobalManager.Current.dropBadFoamPoints;
            var g = GlobalManager.Current.snapPalletePoints;
            var h = GlobalManager.Current.placeFoamPoints;
            var ii = GlobalManager.Current.tearingPoints;
            var j = GlobalManager.Current.recheckPoints;

        }

        //Add By YXW
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

            AddTabIfHasData("Materials", points.LaiLiaoPointList);
            AddTabIfHasData("Assembly", points.ZuZhuangPointList);
            AddTabIfHasData("ReCheck", points.FuJianPointList);

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
                    var tbID = new TextBlock
                    {
                        Text = "ID:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    rowPanel.Children.Add(tbID);
                    addTextBlockClicked(tbID, pt.axisMap, rowPanel);   //点击ID弹出示教


                    // 添加可编辑的 ID 输入框
                    var idTextBox = new TextBox
                    {
                        Text = pt.name,
                        Width = 150,
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

                    rowPanel.Children.Add(CreateLabeledTextBox("X", 0, out xBox, newText =>
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
                    //panel.Children.Add(new TextBlock
                    //{
                    //    Text = $"{MatrixPointPrefix}: {pt.name} ({pt.col}col × {pt.row}row)",
                    //    FontWeight = FontWeights.Bold,
                    //    Tag = pt.row, //把行数存进 Tag
                    //    Margin = new Thickness(0, 8, 0, 4)
                    //});

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
                        ToolTip = "Add the top left, bottom left, bottom right points to fillData",
                        Margin = new Thickness(8, 0, 0, 0),
                        Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton"),
                    };
                    Grid.SetColumn(ButtonAutoData, 3);
                    rowGrid.Children.Add(ButtonAutoData);

                    // 添加整行到主 panel
                    panel.Children.Add(rowGrid);

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
                                Tag = "MatrixRow",
                                Orientation = Orientation.Vertical,
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
                            addTextBlockClicked(tbID, pt.axisMap, pointPanel);

                            TextBox xBox, yBox, zBox, rBox;

                            //回写，用于保存文件
                            //use to save file
                            pointPanel.Children.Add(CreateLabeledTextBox("X", pos[0], out xBox, newText =>
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

                    //if (pt.childList.Count != pt.row * pt.col) return;

                    //var topLeft = pt.childList[0].childPos;
                    //var topRight = pt.childList[pt.col - 1].childPos;
                    //var bottomLeft = pt.childList[(pt.row - 1) * pt.col].childPos;

                    //double[] vecX = new double[4];
                    //double[] vecY = new double[4];
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    vecX[i] = (topRight[i] - topLeft[i]) / (pt.col - 1);
                    //    vecY[i] = (bottomLeft[i] - topLeft[i]) / (pt.row - 1);
                    //}

                    //int idx = 0;
                    //for (int r = 0; r < pt.row; r++)
                    //{
                    //    for (int c = 0; c < pt.col; c++)
                    //    {
                    //        var pos = pt.childList[idx++].childPos;

                    //        pos[0] = topLeft[0] + vecX[0] * c + vecY[0] * r;
                    //        pos[1] = topLeft[1] + vecX[1] * c + vecY[1] * r;
                    //        pos[2] = topLeft[2];
                    //        pos[3] = topLeft[3];

                    //        var boxes = matrixInputs[r][c];
                    //        boxes[0].Text = pos[0].ToString("0.###");
                    //        boxes[1].Text = pos[1].ToString("0.###");
                    //        boxes[2].Text = pos[2].ToString("0.###");
                    //        boxes[3].Text = pos[3].ToString("0.###");
                    //    }
                    //}

                }
                else
                {
                    //var rowPanel = new StackPanel
                    //{
                    //    Tag = "SinglePoint",
                    //    Orientation = Orientation.Horizontal,
                    //    Margin = new Thickness(0, 4, 0, 4)
                    //};

                    //// general 输入框 + 回写
                    //rowPanel.Children.Add(CreateLabeledTextBox(pt.name, pt.general, newText =>
                    //{
                    //    if (double.TryParse(newText, out double val)) pt.general = val;
                    //}));

                    //panel.Children.Add(rowPanel);


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
                        Width = 150,
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
        private FrameworkElement CreateLabeledTextBox(string label, double initialValue, out TextBox tb, Action<string> onTextChanged = null)
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
                Visibility = Visibility.Collapsed,
                ToolTip = "Teaching point",
                Style = (Style)Application.Current.FindResource("MaterialDesignFloatingActionButton"),
                Width = 30,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 5),
                Content = new PackIcon { Kind = PackIconKind.DebugStepInto, Width = 18, Height = 18 }
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

        private void addTextBlockClicked(TextBlock textBlock, List<int> axisIndex, StackPanel backSP)
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

        private bool SaveAllTabsData(string jsonFilePath)
        {

            return FileHelper.SaveToJson(jsonFilePath, stationPoints);
        }

        private void SavePosParam_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = CboxNowType.SelectedIndex;
            string jsonFile = posFileName[selectedIndex];
            if (SaveAllTabsData(jsonFile))
            {
                GlobalManager.Current.laserPoints.Clear();
                GlobalManager.Current.feedar1Points.Clear();
                GlobalManager.Current.feedar2Points.Clear();
                GlobalManager.Current.pickFoam1Points.Clear();
                GlobalManager.Current.pickFoam2Points.Clear();
                GlobalManager.Current.lowerCCDPoints.Clear();
                GlobalManager.Current.dropBadFoamPoints.Clear();
                GlobalManager.Current.snapPalletePoints.Clear();
                GlobalManager.Current.placeFoamPoints.Clear();
                GlobalManager.Current.recheckPoints.Clear();
                GlobalManager.Current.tearingPoints.Clear();
                ParameterConfig.LoadPoints();

                MessageBox.Show("保存点位成功" + posFilePre[selectedIndex]);
            }
            else
                MessageBox.Show("保存点位失败" + posFilePre[selectedIndex]);
        }


        //添加double输入限制-------------------------------------------------------------
        private void RegisterFloatInputHandlers()
        {
            //foreach (var tabObj in AxisGrid)
            //{
            //    if (tabObj is TabItem tabItem)
            //    {
            RegisterHandlersInContainer(AxisGrid);
            //    }
            //}
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

        //END ADD-------------------------------------------------------------


        private void DoubleText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.Handled =!( new Regex(@"^\d*\.?\d*$").IsMatch(e.Text));
            Regex regex = new Regex("[^0-9\\-\\.]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        /// <summary>
        /// 从指定json文件加载配置信息字典
        /// </summary>
        /// <param name="_config"></param>
        /// <returns></returns>
        public int LoadConfig(string jsonfile)
        {
            if (string.IsNullOrEmpty(jsonfile))
            {
                return 1;
            }
            try
            {
                string content = File.ReadAllText(jsonfile);
                GlobalManager.Current.axisparams = JsonConvert.DeserializeObject<AxisParams>(content);
                if (GlobalManager.Current.axisparams == null)
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 将配置信息字典保存至指定json文件
        /// </summary>
        /// <param name="_config"></param>
        /// <returns></returns>
        public bool SaveToJson(string _config)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                string content = JsonConvert.SerializeObject(GlobalManager.Current.axisparams, settings);
                File.WriteAllText(_config, content);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in GlobalManager.Current.axisparams.AxisSpeedDict.Keys.ToList())
            {
                string speedname = key + "_Speed";
                TextBox tbspeed = (TextBox)FindObject(speedname);
                GlobalManager.Current.axisparams.AxisSpeedDict[key] = (int)(double.Parse(tbspeed.Text));
            }
            foreach (var key in GlobalManager.Current.axisparams.AxisAccDict.Keys.ToList())
            {
                string accname = key + "_Acc";
                TextBox tbacc = (TextBox)FindObject(accname);
                GlobalManager.Current.axisparams.AxisAccDict[key] = (int)(double.Parse(tbacc.Text));
            }
            foreach (var key in GlobalManager.Current.axisparams.AxisDecDict.Keys.ToList())
            {
                string decname = key + "_Dec";
                TextBox tbdec = (TextBox)FindObject(decname);
                GlobalManager.Current.axisparams.AxisDecDict[key] = (int)(double.Parse(tbdec.Text));
            }
            string path = Directory.GetCurrentDirectory() + "\\AxisParams.json";
            SaveToJson(path);
        }

        private Object FindObject(string name)
        {
            Object obj = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
            return obj;
        }

        private void ReadAxisParamJson()
        {
            try
            {
                string folder = Directory.GetCurrentDirectory();
                string path = folder + "\\AxisParams.json";

                int ret = LoadConfig(path);
                if (ret != 0)
                {
                    GlobalManager.Current.axisparams.AxisSpeedDict = new Dictionary<string, double>();
                    GlobalManager.Current.axisparams.AxisAccDict = new Dictionary<string, double>();
                    GlobalManager.Current.axisparams.AxisDecDict = new Dictionary<string, double>();
                    for (int i = 0; i < 25; ++i)
                    {
                        GlobalManager.Current.axisparams.AxisSpeedDict.Add(axisarray[i], 100);
                        GlobalManager.Current.axisparams.AxisAccDict.Add(axisarray[i], 100);
                        GlobalManager.Current.axisparams.AxisDecDict.Add(axisarray[i], 100);
                    }
                }
                foreach (var item in GlobalManager.Current.axisparams.AxisSpeedDict)
                {
                    string speedname = item.Key + "_Speed";
                    TextBox tbspeed = (TextBox)FindObject(speedname);
                    tbspeed.Text = ((double)item.Value).ToString();
                }
                foreach (var item in GlobalManager.Current.axisparams.AxisAccDict)
                {
                    string accname = item.Key + "_Acc";
                    TextBox tbacc = (TextBox)FindObject(accname);
                    tbacc.Text = ((double)item.Value).ToString();
                }
                foreach (var item in GlobalManager.Current.axisparams.AxisDecDict)
                {
                    string decname = item.Key + "_Dec";
                    TextBox tbdec = (TextBox)FindObject(decname);
                    tbdec.Text = ((double)item.Value).ToString();
                }
            }
            catch
            {

            }
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
                case "Materials":
                    return stationPoints.LaiLiaoPointList;
                case "Assembly":
                    return stationPoints.ZuZhuangPointList;
                case "ReCheck":
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
            pt.axisMap = dlg.AxexIndexList;   //将轴映射保存
            //点分类
            if (data == 0)
            {
                // 单独点，使用 pt 的 X/Y/Z/R
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

                // 添加可编辑的 ID 输入框
                var idTextBox = new TextBox
                {
                    Text = pt.name,
                    Width = 150,
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

                rowPanel.Children.Add(CreateLabeledTextBox("X", 0, out xBox, newText =>
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
                //mainPanel.Children.Add(new TextBlock
                //{
                //    Text = $"{MatrixPointPrefix}: NewMatrix ({col}col × {row}row)",
                //    FontWeight = FontWeights.Bold,
                //    Tag = row, //把行数存进 Tag
                //    Margin = new Thickness(0, 8, 0, 4)
                //});


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

                mainPanel.Children.Add(rowGrid);


                var ButtonAutoData = new Button
                {
                    Content = "FillData",
                    ToolTip = "Add top left, bottom left, bottom right points to fillData",
                    Margin = new Thickness(8, 0, 0, 0),
                    Style = (Style)Application.Current.FindResource("MaterialDesignRaisedButton"),
                };
                Grid.SetColumn(ButtonAutoData, 3);
                rowGrid.Children.Add(ButtonAutoData);

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

                        pointPanel.Children.Add(CreateLabeledTextBox("X", 0, out xBox, newText => { if (double.TryParse(newText, out double val)) pos[0] = val; }));
                        pointPanel.Children.Add(CreateLabeledTextBox("Y", 0, out yBox, newText => { if (double.TryParse(newText, out double val)) pos[1] = val; }));
                        pointPanel.Children.Add(CreateLabeledTextBox("Z", 0, out zBox, newText => { if (double.TryParse(newText, out double val)) pos[2] = val; }));
                        pointPanel.Children.Add(CreateLabeledTextBox("R", 0, out rBox, newText => { if (double.TryParse(newText, out double val)) pos[3] = val; }));

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
                //var rowPanel = new StackPanel
                //{
                //    Tag = "SinglePoint",
                //    Orientation = Orientation.Horizontal,
                //    Margin = new Thickness(0, 4, 10, 4)
                //};
                //pt.name = $"New General";

                //// general 输入框 + 回写
                //rowPanel.Children.Add(CreateLabeledTextBox(pt.name, 0, newText =>
                //{
                //    if (double.TryParse(newText, out double val)) pt.general = val;
                //}));

                //mainPanel.Children.Add(rowPanel);
                //AddStationData(selectIndex, pt);

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
                    Width = 150,
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
    }
}
