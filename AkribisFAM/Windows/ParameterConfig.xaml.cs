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

        public static void LoadPoints()
        {
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


                if (Node.name!=null && Node.name.Equals("Laser Points"))
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
                if(Node.name !=null && Node.name.Equals("NozzleGap_X"))
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
                else if(pt.type == 1)
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
                        ToolTip = "Add the top left, top right and bottom left points to fillData",
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

                            var pointPanel = new StackPanel
                            {
                                Tag = "MatrixRow",
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(4),
                                Width = 120,
                                Background = new SolidColorBrush(Colors.LightGray),
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

            return new ScrollViewer { Content = panel,VerticalScrollBarVisibility = ScrollBarVisibility.Auto,HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
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

        private Button GreateButton(List<int> axisIndex,StackPanel backSP)
        {
            // 添加 Teaching Point 按钮
            var teachBtn = new Button
            {
                Visibility= Visibility.Collapsed,
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
                MessageBox.Show("保存点位成功"+ posFilePre[selectedIndex]);
            else
                MessageBox.Show("保存点位失败"+ posFilePre[selectedIndex]);
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
                    tbspeed.Text = ((double)item.Value ).ToString();
                }
                foreach (var item in GlobalManager.Current.axisparams.AxisAccDict)
                {
                    string accname = item.Key + "_Acc";
                    TextBox tbacc = (TextBox)FindObject(accname);
                    tbacc.Text = ((double)item.Value ).ToString();
                }
                foreach (var item in GlobalManager.Current.axisparams.AxisDecDict)
                {
                    string decname = item.Key + "_Dec";
                    TextBox tbdec = (TextBox)FindObject(decname);
                    tbdec.Text = ((double)item.Value ).ToString();
                }
            }
            catch { 

            }
        }

        private int first = 1;
        private int station1Init = 0;
        private int station2Init = 0;
        private int station3Init = 0;
        private int station4Init = 0;
        private int station1Finished = 1;
        private int station2Finished = 1;
        private int station3Finished = 1;
        private int station4Finished = 1;

        private void moveforward(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft > endpos)
                    break;
            }
        }

        private void move2forward(Rectangle rect, Rectangle rect1, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                    rect1.Margin = new Thickness(mleft, rect1.Margin.Top, rect1.Margin.Right, rect1.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft > endpos)
                    break;
            }
        }

        private void movebackward(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft -= 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft < endpos)
                    break;
            }
        }

        private void movedown(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            while (true)
            {
                mtop += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(rect.Margin.Left, mtop, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mtop > endpos)
                    break;
            }
        }

        private void moveup(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            while (true)
            {
                mtop -= 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(rect.Margin.Left, mtop, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mtop < endpos)
                    break;
            }
        }

        private void moveCanvasV(Canvas group, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            if (startpos - endpos >= 0)
            {
                while (true)
                {
                    mtop -= 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(group.Margin.Left, mtop, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop < endpos)
                        break;
                }
            }
            else {
                while (true)
                {
                    mtop += 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(group.Margin.Left, mtop, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop > endpos)
                        break;
                }
            }
        }

        private void moveCanvasH(Canvas group, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            if (startpos - endpos >= 0)
            {
                while (true)
                {
                    mtop -= 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(mtop, group.Margin.Top, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop < endpos)
                        break;
                }
            }
            else
            {
                while (true)
                {
                    mtop += 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(mtop, group.Margin.Top, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop > endpos)
                        break;
                }
            }
        }

        private int flag = 1;

        private void returnOK(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag1 = 1;

        private void returnOK1(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag1 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag2 = 1;

        private void returnOK2(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag2 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag3 = 1;

        private void returnOK3(Rectangle rect)
        {
            if (flag3 == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.Yellow);
                }));
            }
            else if (flag3 == 2)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.Green);
                }));
            }
            else if (flag3 == 3)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.Red);
                }));
            }
            while (flag3 > 0)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag4 = 1;

        private void returnOK4(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag4 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag5 = 1;

        private void returnOK5(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag5 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag6 = 1;

        private void returnOK6(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag6 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag7 = 1;

        private void returnOK7(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag7 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag8 = 1;

        private void returnOK8(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag8 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag9 = 1;

        private void returnOK9(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag9 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag10 = 1;

        private void returnOK10(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag10 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag11 = 1;

        private void returnOK11(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag11 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag12 = 1;

        private void returnOK12(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag12 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag13 = 1;

        private void returnOK13(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag13 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag14 = 1;

        private void returnOK14(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag14 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag15 = 1;

        private void returnOK15(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag15 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag17 = 1;

        private void returnOK17(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag17 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag18 = 1;

        private void returnOK18(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag18 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag19 = 1;

        private void returnOK19(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag19 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag20 = 1;

        private void returnOK20(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag20 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }
        private void returnNG(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
            }));
            Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            //station1Init = 1;
            //Task task1 = new Task(Station1Act);
            //task1.Start();
            //Task task2 = new Task(Station2Act);
            //task2.Start();
            //Task task3 = new Task(Station3Act);
            //task3.Start();
        }

        int deltatime = 0;
        private void wait()
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                deltatime = 999999;
            }

            while (true)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = deltatime - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }
        }



        private int module_Num = 12;
        const int numberofstation = 4;
        private int[] By_pass = new int[numberofstation] { 0, 0, 0, 0 };
        private int[] By_pass_index = new int[numberofstation] { 0, 0, 0, 0 };
        private int current_index = 0;
        private void LailiaoAct(Rectangle rect, Rectangle rect1)
        {
            Task task9, task4;
            current_index++;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            //pallet in
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in";
            }));
            beltmoveflag[0] = 1;
            move2forward(rect, rect1, 10, 69, 20);
            beltmoveflag[0] = 0;
            wait();
            //trigger jiansu IO
            flag = 1;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.LaiLiao_JianSu] = true;
            Task task1 = new Task(() => returnOK(rect20, rect51));
            task1.Start();
            wait();
            //send dingqi IO
            task9 = new Task(() => moveup(rect52, 355, 345, 20));
            task9.Start();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            flag1 = 1;
            Task task2 = new Task(() => returnOK1(rect21, rect52));
            task2.Start();
            wait();
            beltmoveflag[0] = 1;
            move2forward(rect, rect1, 69, 101, 20);
            beltmoveflag[0] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
        step2:
            //scan扫码
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "start scan pallet";
            }));
            flag2 = 1;
            Task task3 = new Task(() => returnOK2(rect22));
            task3.Start();
            Thread.Sleep(3000);
            wait();
            flag2 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "get scan result";
            }));
            Thread.Sleep(1000);
            By_pass_index[0] = current_index;
            By_pass[0] = GenerateRandomNumber(0, 2);
            if (By_pass[0] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                flag3 = 3;
                task4 = new Task(() => returnOK3(rect));
                task4.Start();
                Thread.Sleep(1000);
                goto step5;
            }
        step3:
            //pallet lift up
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            flag3 = 1;
            task4 = new Task(() => returnOK3(rect));
            task4.Start();
            Thread.Sleep(1000);
            wait();
        step4:
            //laser measure激光
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "start laser measure";
            }));
            for (int i = 0; i < module_Num; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action5.Content = $"laser go to module{i + 1} position{j + 1} and trigger laser";
                    }));
                    flag4 = 1;
                    task3 = new Task(() => returnOK4(rect25));
                    task3.Start();
                    Thread.Sleep(200);
                    wait();
                    flag4 = 0;
                }
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "laser measure finished";
            }));
            Thread.Sleep(1000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
        step5:
            task9 = new Task(() => movedown(rect52, 345, 355, 20));
            task9.Start();
            flag1 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();

        }

        public int GenerateRandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private int current_Assembled = 0;
        private int Picker_FOAM_Count = 0;
        private int Picker_OK_FOAM_Count = 0;
        private int BadFoamCount = 0;
        private int has_XueWeiXinXi = 0;
        private int NG_Foam_Count = 0;

        private void ZuzhuangAct(Rectangle rect, Rectangle rect1)
        {
            By_pass_index[1] = By_pass_index[0];
            By_pass[1] = By_pass[0];
            Task task1, task2, task3, task4, task5, task6, task8, task9;
            double xpos, ypos;
            //move to assembly
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            flag17 = 1;
            task8 = new Task(() => returnOK17(rect59));
            task8.Start();
            beltmoveflag[1] = 1;
            move2forward(rect, rect1, 101, 143, 20);
            flag = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.LaiLiao_JianSu] = false;
            move2forward(rect, rect1, 143, 174, 20);
            flag17 = 0;
            move2forward(rect, rect1, 174, 253, 20);
            beltmoveflag[1] = 0;
            wait();
            //trigger jiansu IO
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.ZuZhuang_JianSu] = true;
            flag5 = 1;
            task1 = new Task(() => returnOK5(rect23, rect53));
            task1.Start();
            if (By_pass[1] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                beltmoveflag[1] = 1;
                move2forward(rect, rect1, 253, 290, 20);
                beltmoveflag[1] = 0;
                Thread.Sleep(100);
                wait();
                return;
            }
            //send dingqi IO
            task9 = new Task(() => moveup(rect54, 355, 345, 20));
            task9.Start();
            flag6 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK6(rect24, rect54));
            task2.Start();
            wait();
            beltmoveflag[1] = 1;
            move2forward(rect, rect1, 253, 290, 20);
            beltmoveflag[1] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //pallet lift up
            flag7 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            task4 = new Task(() => returnOK7(rect));
            task4.Start();
            wait();
            current_Assembled = 0;
            //如果吸嘴上有料，直接跳去CCD2精定位
            if (Picker_FOAM_Count > 0)
            {
                goto step4;
            }
        step2:
            //飞达上拍料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to feeder";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task3 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 54, 20));
            task3.Start();
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 217, 20));
            task5.Start();
            Task.WaitAll(task3, task5);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Thread.Sleep(1000);
            wait();
        step3:
            //吸嘴取料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "pick foams";
                picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length - 1) + "X";
            }));
            flag8 = 1;
            task3 = new Task(() => returnOK8(rect26));
            task3.Start();
            Thread.Sleep(1000);
            wait();
        step4:
            //CCD2 精定位
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to CCD2";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 117, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 248, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Thread.Sleep(1000);
            wait();
            if (Picker_FOAM_Count == 0)
            {
                BadFoamCount = GenerateRandomNumber(0, 2);
                Picker_FOAM_Count = GenerateRandomNumber(2, 5);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length-1) + $"{Picker_FOAM_Count}";
                }));
                if (BadFoamCount > 0)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action4.Content = $"{BadFoamCount} foam(s) is NG";
                    }));
                    goto step5;
                }
                else
                {
                    goto step6;
                }
            }
            else
            {
                goto step6;
            }
        step5:
            //如果有坏料，放到坏料盒里
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to CCD2";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 117, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 184, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            wait();
            for (int i = 0; i < BadFoamCount; ++i)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action4.Content = $"throw NG faom {i + 1}";
                    Picker_FOAM_Count = Picker_FOAM_Count - 1;
                    picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length - 1) + $"{Picker_FOAM_Count}";
                }));
                Thread.Sleep(1000);
                wait();
                NG_Foam_Count++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    NGnum.Content = $"{NG_Foam_Count}";
                }));
            }
        step6:
            //拍料盘
            if (has_XueWeiXinXi == 1)
            {
                goto step7;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to pallet";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 199, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 277, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            has_XueWeiXinXi = 1;
            Thread.Sleep(1000);
            wait();
        step7:
            //放料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to pallet";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 199, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 277, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "place faoms";
            }));
            int cnt = 0;
            while (Picker_FOAM_Count > 0)
            {
                Thread.Sleep(1000);
                wait();
                cnt++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action4.Content = $"place faom {cnt}";
                }));
                Picker_FOAM_Count--;
                current_Assembled++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    palletfaomnum.Content = $"{current_Assembled} ";
                    picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length - 1) + $"{Picker_FOAM_Count}";
                }));
                if (current_Assembled >= module_Num)
                {
                    break;
                }
            }
            if (current_Assembled < module_Num)
            {
                flag8 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = "picker has no foams ";
                }));
                Thread.Sleep(1000);
                wait();
                goto step2;
            }
            if (Picker_FOAM_Count == 0)
            {
                flag8 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = "picker has no foams ";
                }));
                Thread.Sleep(1000);
                wait();
            }
            current_Assembled = 0;
            has_XueWeiXinXi = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                palletfaomnum.Content = " ";
            }));
            flag7 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            task9 = new Task(() => movedown(rect54, 345, 355, 20));
            task9.Start();
            flag6 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();
        }

        private int Left_Foam_Count = 0;
        private int Fujian_OK = 0;
        private void FujianAct(Rectangle rect, Rectangle rect1)
        {
            By_pass_index[2] = By_pass_index[1];
            By_pass[2] = By_pass[1];
            Task task1, task2, task3, task4, task8, task9;
            //move to next
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            flag18 = 1;
            task8 = new Task(() => returnOK18(rect60));
            task8.Start();
            beltmoveflag[2] = 1;
            move2forward(rect, rect1, 290, 326, 20);
            flag5 = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.ZuZhuang_JianSu] = false;
            move2forward(rect, rect1, 326, 363, 20);
            flag18 = 0;
            move2forward(rect, rect1, 363, 428, 20);
            beltmoveflag[2] = 0;
            wait();
            //trigger jiansu IO
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] = true;
            flag9 = 1;
            task1 = new Task(() => returnOK9(rect28, rect55));
            task1.Start();
            if (By_pass[2] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                beltmoveflag[2] = 1;
                move2forward(rect, rect1, 428, 467, 20);
                beltmoveflag[2] = 0;
                Thread.Sleep(100);
                wait();
                return;
            }
            //send dingqi IO
            task9 = new Task(() => moveup(rect56, 355, 345, 20));
            task9.Start();
            flag10 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK10(rect27, rect56));
            task2.Start();
            wait();
            beltmoveflag[2] = 1;
            move2forward(rect, rect1, 428, 467, 20);
            beltmoveflag[2] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //pallet lift up
            flag11 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            task4 = new Task(() => returnOK11(rect));
            task4.Start();
            wait();
            Left_Foam_Count = module_Num;
        step2:
            //撕膜
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "start to tear film";
            }));
            for (int i = 0; i < module_Num; ++i)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = $"robot go to module{i + 1} and tear film";
                }));
                flag12 = 1;
                task3 = new Task(() => returnOK12(rect29));
                task3.Start();
                Thread.Sleep(1000);
                wait();
                Left_Foam_Count--;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Leftfoam.Content = $"{Left_Foam_Count}";
                }));
                flag12 = 0;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "tear film finished";
            }));
            Thread.Sleep(1000);
            wait();
        step3:
            //CCD3复检
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Fujian_OK = GenerateRandomNumber(0, 2);
            //flag11 = 1;
            //task4 = new Task(() => returnOK11(rect));
            //task4.Start();
            Thread.Sleep(1000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Leftfoam.Content = " ";
            }));
            flag11 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            task9 = new Task(() => movedown(rect56, 345, 355, 20));
            task9.Start();
            flag10 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();
        }

        private void RejectAct(Rectangle rect, Rectangle rect1)
        {
            By_pass_index[3] = By_pass_index[2];
            By_pass[3] = By_pass[2];
            Task task1, task2, task3, task4, task8, task9;
            //move to next
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            flag19 = 1;
            task8 = new Task(() => returnOK19(rect61));
            task8.Start();
            beltmoveflag[3] = 1;
            move2forward(rect, rect1, 467, 500, 20);
            flag9 = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] = false;
            move2forward(rect, rect1, 500, 541, 20);
            flag19 = 0;
            move2forward(rect, rect1, 541, 612, 20);
            beltmoveflag[3] = 0;
            wait();
            //trigger jiansu IO
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = true;
            flag13 = 1;
            task1 = new Task(() => returnOK13(rect31, rect57));
            task1.Start();
            if (By_pass[3] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                beltmoveflag[3] = 1;
                move2forward(rect, rect1, 612, 646, 20);
                flag20 = 1;
                task8 = new Task(() => returnOK20(rect62));
                task8.Start();
                move2forward(rect, rect1, 646, 685, 20);
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                flag13 = 0;
                move2forward(rect, rect1, 685, 719, 20);
                flag20 = 0;
                move2forward(rect, rect1, 719, 740, 20);
                beltmoveflag[3] = 0;
                Thread.Sleep(1000);
                wait();
                return;
            }
            //send dingqi IO
            task9 = new Task(() => moveup(rect58, 355, 345, 20));
            task9.Start();
            flag14 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK14(rect30, rect58));
            task2.Start();
            wait();
            beltmoveflag[3] = 1;
            move2forward(rect, rect1, 612, 646, 20);
            beltmoveflag[3] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
        step2:
            if (Fujian_OK == 0)
            {
                //NG顶升
                flag15 = 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "NG pallet lift up";
                }));
                task4 = new Task(() => returnOK15(rect));
                task4.Start();
                Thread.Sleep(1000);
                wait();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "NG pallet remove";
                }));
                Thread.Sleep(3000);
                task9 = new Task(() => movedown(rect58, 345, 355, 20));
                task9.Start();
                flag14 = 0;
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                flag13 = 0;
            }
            else
            {
                task9 = new Task(() => movedown(rect58, 345, 355, 20));
                task9.Start();
                flag14 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action2.Content = "send cylinder IO off";
                }));
                Thread.Sleep(1000);
                wait();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet go out";
                }));
                flag20 = 1;
                task8 = new Task(() => returnOK20(rect62));
                task8.Start();
                beltmoveflag[3] = 1;
                move2forward(rect, rect1, 646, 685, 20);
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                flag13 = 0;
                move2forward(rect, rect1, 685, 719, 20);
                flag20 = 0;
                move2forward(rect, rect1, 719, 740, 20);
                beltmoveflag[3] = 0;
                wait();
            }
            flag15 = 0;

        }

        private int[] beltmoveflag = new int[4];
        private int beltdelta = 0;
        private void beltmove()
        {
            int flag = 1;
            while (true)
            {
                int run = 0;
                for (int i = 0; i < 4; i++)
                {
                    run += beltmoveflag[i];
                }
                if (run == 0)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    if (flag == 1)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            arrow1.Visibility = Visibility.Visible;
                            arrow2.Visibility = Visibility.Visible;
                            arrow3.Visibility = Visibility.Visible;
                            arrow4.Visibility = Visibility.Visible;
                            arrow5.Visibility = Visibility.Visible;
                            arrow6.Visibility = Visibility.Visible;
                            arrow7.Visibility = Visibility.Visible;
                            arrow8.Visibility = Visibility.Visible;
                            arrow11.Visibility = Visibility.Hidden;
                            arrow12.Visibility = Visibility.Hidden;
                            arrow13.Visibility = Visibility.Hidden;
                            arrow14.Visibility = Visibility.Hidden;
                            arrow15.Visibility = Visibility.Hidden;
                            arrow16.Visibility = Visibility.Hidden;
                            arrow17.Visibility = Visibility.Hidden;
                            arrow18.Visibility = Visibility.Hidden;
                        }));
                        flag = 0;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            arrow1.Visibility = Visibility.Hidden;
                            arrow2.Visibility = Visibility.Hidden;
                            arrow3.Visibility = Visibility.Hidden;
                            arrow4.Visibility = Visibility.Hidden;
                            arrow5.Visibility = Visibility.Hidden;
                            arrow6.Visibility = Visibility.Hidden;
                            arrow7.Visibility = Visibility.Hidden;
                            arrow8.Visibility = Visibility.Hidden;
                            arrow11.Visibility = Visibility.Visible;
                            arrow12.Visibility = Visibility.Visible;
                            arrow13.Visibility = Visibility.Visible;
                            arrow14.Visibility = Visibility.Visible;
                            arrow15.Visibility = Visibility.Visible;
                            arrow16.Visibility = Visibility.Visible;
                            arrow17.Visibility = Visibility.Visible;
                            arrow18.Visibility = Visibility.Visible;
                        }));
                        flag = 1;
                    }
                    Thread.Sleep(200);
                }
            }
        }

        public static void CopyProperties(object source, object destination)
        {
            var sourceProps = source.GetType().GetProperties();
            foreach (var prop in sourceProps)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(destination, prop.GetValue(source));
                }
            }
        }
        private void Station1Act()
        {
            while (true)
            {
                if (station1Init == 1)
                {
                    Thread.Sleep(500);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect10.Visibility = Visibility.Visible;
                        this.rect50.Visibility = Visibility.Visible;
                    }));
                    station1Finished = 0;
                    LailiaoAct(this.rect10, this.rect50);
                    while (station2Finished == 0)
                    {
                        station1Init = 0;
                        Thread.Sleep(10);
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CopyProperties(rect10, rect32);
                        CopyProperties(rect50, rect63);
                        this.rect10.Visibility = Visibility.Hidden;
                        this.rect50.Visibility = Visibility.Hidden;
                    }));
                    station1Finished = 1;
                    station2Init = 1;
                    Thread.Sleep(10);
                    //if (station2Finished != 0)
                    //{
                    //    this.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        //this.rect10.Visibility = Visibility.Hidden;
                    //        CopyProperties(rect10, rect32);
                    //    }));
                    //    station2Init = 1;
                    //    Thread.Sleep(10);
                    //}
                    //else
                    //{
                    //    station2Init = 0;
                    //    station1Init = 0;
                    //}
                }
            }
        }

        private void Station2Act()
        {
            while (true)
            {
                if (station2Init == 1)
                {
                    station1Init = 1;
                    station2Init = 0;
                    station2Finished = 0;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect32.Visibility = Visibility.Visible;
                        this.rect63.Visibility = Visibility.Visible;
                    }));
                    ZuzhuangAct(this.rect32, this.rect63);
                    while (station3Finished == 0)
                    {
                        station2Init = 0;
                        Thread.Sleep(10);
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CopyProperties(rect32, rect33);
                        CopyProperties(rect63, rect64);
                        this.rect32.Visibility = Visibility.Hidden;
                        this.rect63.Visibility = Visibility.Hidden;
                    }));
                    station2Finished = 1;
                    station3Init = 1;
                    Thread.Sleep(10);
                    //if (station3Finished != 0)
                    //{
                    //    this.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        //this.rect32.Visibility = Visibility.Hidden;
                    //        CopyProperties(rect32, rect33);
                    //    }));
                    //    station3Init = 1;
                    //    Thread.Sleep(10);
                    //}
                    //else
                    //{
                    //    station3Init = 0;
                    //    station2Init = 0;
                    //}
                }
            }
        }

        private void Station3Act()
        {
            while (true)
            {
                if (station3Init == 1)
                {
                    //station2Init = 1;
                    //this.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    this.rect33.Visibility = Visibility.Visible;
                    //}));
                    station3Init = 0;
                    station3Finished = 0;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect33.Visibility = Visibility.Visible;
                        this.rect64.Visibility = Visibility.Visible;
                    }));
                    FujianAct(this.rect33, this.rect64);
                    while (station4Finished == 0)
                    {
                        station3Init = 0;
                        Thread.Sleep(10);
                    }
                    station3Finished = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CopyProperties(rect33, rect34);
                        CopyProperties(rect64, rect65);
                        this.rect33.Visibility = Visibility.Hidden;
                        this.rect64.Visibility = Visibility.Hidden;
                    }));
                    station4Init = 1;
                    Thread.Sleep(10);
                    //if (station4Finished != 0)
                    //{
                    //    this.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        if (station2Finished == 0)
                    //        {
                    //            CopyProperties(rect33, rect34);
                    //            this.rect33.Visibility = Visibility.Hidden;
                    //        }
                    //        else {
                    //            this.rect33.Visibility = Visibility.Visible;
                    //            CopyProperties(rect33, rect34);
                    //        }
                    //    }));
                    //    station4Init = 1;
                    //    Thread.Sleep(10);
                    //}
                    //else
                    //{
                    //    station4Init = 0;
                    //    station3Init = 0;
                    //}
                }
            }
        }

        private void Station4Act()
        {
            while (true)
            {
                if (station4Init == 1)
                {
                    //station3Init = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect34.Visibility = Visibility.Visible;
                        this.rect65.Visibility = Visibility.Visible;
                    }));
                    station4Init = 0;
                    station4Finished = 0;
                    RejectAct(this.rect34, this.rect65);
                    station4Finished = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect34.Visibility = Visibility.Hidden;
                        this.rect65.Visibility = Visibility.Hidden;
                    }));
                }
            }
        }

        private void wholeprocess()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.rect10.Visibility = Visibility.Visible;
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = " ";
            }));
            while (true)
            {
                //LailiaoAct(this.rect10);
                //ZuzhuangAct(this.rect10);
                //FujianAct(this.rect10);
                //RejectAct(this.rect10);

            }
        }

        private void start1_Click(object sender, RoutedEventArgs e)
        {
            //while (!GlobalManager.Current.isRun) {
            //    Thread.Sleep(100);
            //}
            flag = 0;
            flag1 = 0;
            flag2 = 0;
            flag3 = 0;
            flag4 = 0;
            NG_Foam_Count = 0;
            station1Init = 1;
            current_index = 0;
            beltmoveflag[0] = 0;
            beltmoveflag[1] = 0;
            beltmoveflag[2] = 0;
            beltmoveflag[3] = 0;
            rect21.Fill = new SolidColorBrush(Colors.Green);
            rect24.Fill = new SolidColorBrush(Colors.Green);
            rect27.Fill = new SolidColorBrush(Colors.Green);
            rect30.Fill = new SolidColorBrush(Colors.Green);

            rect52.Fill = new SolidColorBrush(Colors.Green);
            rect54.Fill = new SolidColorBrush(Colors.Green);
            rect56.Fill = new SolidColorBrush(Colors.Green);
            rect58.Fill = new SolidColorBrush(Colors.Green);

            Task task1 = new Task(Station1Act);
            task1.Start();
            Task task2 = new Task(Station2Act);
            task2.Start();
            Task task3 = new Task(Station3Act);
            task3.Start();
            Task task4 = new Task(Station4Act);
            task4.Start();
            Task task5 = new Task(beltmove);
            task5.Start();

            //Task task1 = new Task(wholeprocess);
            //task1.Start();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            deltatime = 999999;
        }

        private void resume_Click(object sender, RoutedEventArgs e)
        {
            deltatime = 0;
        }

        private int flag16 = 1;
        private void returnOK16(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag16 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
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

        private void AddStationData(int listIndex,Point point)
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

                AddStationData(selectIndex,pt);

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
                    ToolTip = "Add the top left, top right and bottom left points to fillData",
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

                        var pointPanel = new StackPanel
                        {
                            Tag = "MatrixRow",
                            Orientation = System.Windows.Controls.Orientation.Vertical,
                            Margin = new Thickness(4),
                            Width = 120,
                            Background = new SolidColorBrush(Colors.LightGray),
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
            else if(data == 2)
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
