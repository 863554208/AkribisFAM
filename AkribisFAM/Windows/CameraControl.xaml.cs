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

                    //rowPanel.Children.Add(new TextBlock
                    //{
                    //    Text = $"ID: {pt.name}",
                    //    //Width = 100,
                    //    Margin = new Thickness(0, 0, 15, 0),
                    //    VerticalAlignment = VerticalAlignment.Center
                    //});



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


                    rowPanel.Children.Add(CreateLabeledTextBox("X", pt.X, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.X = val;
                    }));

                    rowPanel.Children.Add(CreateLabeledTextBox("Y", pt.Y, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.Y = val;
                    }));

                    rowPanel.Children.Add(CreateLabeledTextBox("Z", pt.Z, newText =>
                    {
                        if (double.TryParse(newText, out double val)) pt.Z = val;
                    }));

                    rowPanel.Children.Add(CreateLabeledTextBox("R", pt.R, newText =>
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
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // 输入框宽度
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

                    // 添加整行到主 panel
                    panel.Children.Add(rowGrid);


                    int childIndex = 0;
                    for (int r = 0; r < pt.row; r++)
                    {
                        var rowPanel = new StackPanel
                        {
                            Tag = "MatrixRow",
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 4, 0, 4)
                        };

                        for (int c = 0; c < pt.col; c++)
                        {
                            var child = pt.childList[childIndex++];
                            string displayName = child.childName[0];
                            var pos = child.childPos;

                            var pointPanel = new StackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(4),
                                Width = 140,
                                Background = new SolidColorBrush(Colors.LightGray),
                            };

                            pointPanel.Children.Add(new TextBlock
                            {
                                Text = $"ID: {displayName}",
                                Margin = new Thickness(0, 0, 0, 6)
                            });

                            //回写，用于保存文件
                            pointPanel.Children.Add(CreateLabeledTextBox("X", pos[0], newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[0] = val;
                            }));

                            pointPanel.Children.Add(CreateLabeledTextBox("Y", pos[1], newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[1] = val;
                            }));

                            pointPanel.Children.Add(CreateLabeledTextBox("Z", pos[2], newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[2] = val;
                            }));

                            pointPanel.Children.Add(CreateLabeledTextBox("R", pos[3], newText =>
                            {
                                if (double.TryParse(newText, out double val)) pos[3] = val;
                            }));

                            pointPanel.Children.Add(GreateButton(pt.axisMap, pointPanel));

                            rowPanel.Children.Add(pointPanel);

                        }

                        panel.Children.Add(rowPanel);


                    }
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
                        Width = 100,
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
        private FrameworkElement CreateLabeledTextBox(string label, double initialValue, Action<string> onTextChanged = null)
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

            var tb = new TextBox
            {
                Width = 110,
                Text = initialValue.ToString()
            };

            tb.PreviewTextInput += FloatTextBox_PreviewTextInput;
            tb.PreviewKeyDown += FloatTextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(tb, FloatTextBox_Pasting);

            // 禁用输入法
            InputMethod.SetIsInputMethodEnabled(tb, false);

            // 如果有绑定回调，就在文本变更时触发
            if (onTextChanged != null)
            {
                tb.TextChanged += (s, e) =>
                {
                    onTextChanged(tb.Text);
                };
            }

            panel.Children.Add(tb);

            return panel;
        }

        private Button GreateButton(List<int> axisIndex, StackPanel backSP)
        {
            // 添加 Teaching Point 按钮
            var teachBtn = new Button
            {
                ToolTip = "Teaching point",
                Style = (Style)Application.Current.FindResource("MaterialDesignFloatingActionButton"),
                Width = 36,
                Height = 36,
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

                rowPanel.Children.Add(CreateLabeledTextBox("X", 0, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.X = val;
                }));

                rowPanel.Children.Add(CreateLabeledTextBox("Y", 0, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.Y = val;
                }));

                rowPanel.Children.Add(CreateLabeledTextBox("Z", 0, newText =>
                {
                    if (double.TryParse(newText, out double val)) pt.Z = val;
                }));

                rowPanel.Children.Add(CreateLabeledTextBox("R", 0, newText =>
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
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // 输入框宽度
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



                int childIndex = 0;
                for (int r = 0; r < row; r++)
                {
                    var rowPanel = new StackPanel
                    {
                        Tag = "MatrixRow",
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new Thickness(0, 4, 0, 4)
                    };

                    for (int c = 0; c < col; c++)
                    {
                        var child = pt.childList[childIndex++];
                        string displayName = child.childName[0];
                        var pos = child.childPos;

                        var pointPanel = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Vertical,
                            Margin = new Thickness(4),
                            Width = 140,
                            Background = new SolidColorBrush(Colors.LightGray),
                        };

                        pointPanel.Children.Add(new TextBlock
                        {
                            Text = $"ID: {displayName}",
                            Margin = new Thickness(0, 0, 0, 6)
                        });

                        //回写，用于保存文件
                        pointPanel.Children.Add(CreateLabeledTextBox("X", 0, newText =>
                        {
                            if (double.TryParse(newText, out double val)) pos[0] = val;
                        }));

                        pointPanel.Children.Add(CreateLabeledTextBox("Y", 0, newText =>
                        {
                            if (double.TryParse(newText, out double val)) pos[1] = val;
                        }));

                        pointPanel.Children.Add(CreateLabeledTextBox("Z", 0, newText =>
                        {
                            if (double.TryParse(newText, out double val)) pos[2] = val;
                        }));

                        pointPanel.Children.Add(CreateLabeledTextBox("R", 0, newText =>
                        {
                            if (double.TryParse(newText, out double val)) pos[3] = val;
                        }));

                        pointPanel.Children.Add(GreateButton(pt.axisMap, pointPanel));

                        rowPanel.Children.Add(pointPanel);

                    }

                    mainPanel.Children.Add(rowPanel);
                }
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
                    Width = 100,
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
