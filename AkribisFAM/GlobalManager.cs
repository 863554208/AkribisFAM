using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AkribisFAM.AAmotionFAM;
using System.Timers;
using AAMotion;
using AGM800 = AkribisFAM.AAmotionFAM.AGM800;
using System.Diagnostics;
using AkribisFAM.Manager;
using System.Threading;
using AkribisFAM.ViewModel;
using LiveCharts;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Windows;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Windows.Forms.Design;
using System.Windows.Documents;

namespace AkribisFAM
{
    [DataContract]
    public class Point
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int type { get; set; }
        [DataMember]

        public List<ChildPoint> childList { get; set; }
        [DataMember]

        public int col { get; set; }
        [DataMember]
        public int row { get; set; }
        [DataMember]
        public double X { get; set; }
        [DataMember]
        public double Y { get; set; }
        [DataMember]
        public double Z { get; set; }
        [DataMember]
        public double R { get; set; }

        [DataMember]
        public double spacingX { get; set; }

        [DataMember]
        public double spacingY { get; set; }

        [DataMember]
        public double offer10{ get; set; }

        [DataMember]
        public double offer11 { get; set; }

        [DataMember]
        public List<int> axisMap { get; set; }

        [DataMember]
        public double general { get; set; }

    }

    [DataContract]


    public class ChildPoint
    {
        [DataMember]
        public List<string> childName { get; set; }

        [DataMember]
        public List<double> childPos { get; set; }

    }



    [DataContract]
    public class StationPoints
    {
        [DataMember]
        public List<Point> LaiLiaoPointList { get; set; }
        [DataMember]
        public List<Point> ZuZhuangPointList { get; set; }
        [DataMember]
        public List<Point> FuJianPointList { get; set; }
        [DataMember]
        public List<Point> RejectPointList { get; set; }
    }



    [JsonObject]
    public class AxisParams
    {
        [JsonProperty("AxisSpeedDict")]
        public Dictionary<string, double> AxisSpeedDict { get; set; }
        [JsonProperty("AxisAccDict")]
        public Dictionary<string, double> AxisAccDict { get; set; }
        [JsonProperty("AxisDecDict")]
        public Dictionary<string, double> AxisDecDict { get; set; }
    }



    public class GlobalManager
    {



        // 单例模式，确保全局只有一个实例
        private static GlobalManager _current;

        //全局心跳包
        private System.Timers.Timer heartbeatTimer;

        private System.Timers.Timer PosTimer;

        public Queue<string> BarcodeQueue = new Queue<string>();
        public bool IsUseMES = false;

        //delay (etc. 300 means 300 milliseconds) to trigger laser height after the LSX&LSY reaches its destination.
        public int LaserHeightDelay = 50;

        public double[][] laser_data;

        public bool isLowerCCD = false;
        //错误队列
        private DispatcherTimer _errorCheckTimer;

        //A轴实时位置
        public long current_APos = 0;

        public string username;

        //传送带到位标志位
        public int flag_RangeFindingTrayArrived;  //测距位料盘到位
        public int flag_assembleTrayArrived;      //贴装位料盘到位
        public int flag_RecheckTrayArrived;       //复检位料盘到位

        public int flag_TrayProcessCompletedNumber;

        public int flag_TrayArrivedNumber;

        public int flag_NGStationAllowTrayEnter;

        public int flag_RecheckStationHaveTray;

        public int flag_RecheckStationRequestOutflowTray;
        public int flag_Bypass;

        public int laserpoint1_shift_X = 20;
        public int laserpoint1_shift_Y = 0;
        public int laserpoint2_shift_X = 20;
        public int laserpoint2_shift_Y = 20;
        public int laserpoint3_shift_X = 0;
        public int laserpoint3_shift_Y = 20;

        //参数界面
        public int NozzleGap_X = 20;
        public int PalleteGap_X = 50;
        public int PalleteGap_Y = 40;
        public int TotalRow = 3;
        public int TotalColumn = 4;

        public SinglePoint RecheckRecylePos = new SinglePoint();
        public SinglePoint SafeZPos = new SinglePoint();
        public SinglePoint StartPoint = new SinglePoint();
        public double TearX = 0;
        public double TearY = 0;
        public double TearZ = 0;
        public double TearXvel = 0;
        public double TearYvel = 0;
        public double TearZvel = 0;

        //记录每个工站是否在气缸上气和顶升的状态
        public bool station1_IsLifting;
        public bool station2_IsLifting;
        public bool station3_IsLifting;
        public bool station4_IsLifting;

        //记录每个工站是否在气缸放气和下降的状态
        public bool station1_IsLiftingDown;
        public bool station2_IsLiftingDown;
        public bool station3_IsLiftingDown;
        public bool station4_IsLiftingDown;

        public bool station1_IsBoardInHighSpeed;
        public bool station2_IsBoardInHighSpeed;
        public bool station3_IsBoardInHighSpeed;
        public bool station4_IsBoardInHighSpeed;

        public bool station1_IsBoardInLowSpeed;
        public bool station2_IsBoardInLowSpeed;
        public bool station3_IsBoardInLowSpeed;
        public bool station4_IsBoardInLowSpeed;

        public bool station1_IsBoardOut;
        public bool station2_IsBoardOut;
        public bool station3_IsBoardOut;
        public bool station4_IsBoardOut;

        // 记录 A 轴和 B 轴的是否到位的状态

        public bool UsePicker1;
        public bool UsePicker2;
        public bool UsePicker3;
        public bool UsePicker4;

        //判断每个吸到的料在经过CCD2复检之后时候合格
        public bool picker1State;
        public bool picker2State;           
        public bool picker3State;
        public bool picker4State;

        //记录3号工位检测出的是否是NG板
        public bool isNGPallete;
        public bool IsByPass;
        public bool SendByPassToStation2;
        public bool SendByPassToStation3;
        public bool SendByPassToStation4;
        public bool IsAInTarget { get; set; }
        public bool IsBInTarget { get; set; }

        public bool UseFeedar1 = true;
        public bool UseFeedar2 = false;

        public int NGTrayDelaytime = 1000;
        //测试用
        public bool isRun = false;

        public List<SinglePoint> laserPoints = new List<SinglePoint>();

        public List<SinglePoint> feedar1Points = new List<SinglePoint>();

        public List<SinglePoint> feedar2Points = new List<SinglePoint>();

        public List<SinglePoint> pickFoam1Points = new List<SinglePoint>();

        public List<SinglePoint> pickFoam2Points = new List<SinglePoint>();

        public List<SinglePoint> lowerCCDPoints = new List<SinglePoint>();

        public List<SinglePoint> dropBadFoamPoints = new List<SinglePoint>();

        public List<SinglePoint> snapPalletePoints = new List<SinglePoint>();

        public List<SinglePoint> placeFoamPoints = new List<SinglePoint>();

        public List<SinglePoint> recheckPoints = new List<SinglePoint>();

        public List<SinglePoint> tearingPoints = new List<SinglePoint>();


        public int TotalLaserCount = 48;

        public StationPoints stationPoints;


        public int TotalBadFoam = 0;
        #region 全局用来判断机器状态的标志位

        //模拟进板位置有料和无料IO信号
        public bool IO_test1 { get; set; }

        public bool IO_test2 { get; set; }

        public bool IO_test3 { get; set; }

        public bool IO_test4 { get; set; }

        public bool hive_Result { get; set; }

        public bool IsPause { get; set; }

        //是否已经拍了pallete拼盘
        public bool palleteSnaped { get; set; }

        //当前有多少组装到pallete里面
        public int current_Assembled { get; set; }

        //当前吸嘴上吸了多少foam
        public int current_FOAM_Count { get; set; }

        public int currentLasered = 0;
        public int BadFoamCount { get; set; }

        //总共需要安装的穴位总数
        public int total_Assemble_Count = 12;

        public int laser_point_length = 4;
        public bool lailiao_ChuFaJinBan { get; set; }
        public bool lailiao_JinBanWanCheng { get; set; }
        public bool lailiao_SaoMa { get; set; }
        public bool lailiao_JiGuangCeJu { get; set; }
        public bool CCD1InPosition { get; set; }
        public bool Feedar1Captured { get; set; }
        public bool CCD2Captured { get; set; }
        public bool MoveToLiaopan { get; set; }
        public bool GrabLiaoPan { get; set; }

        public bool Pause1 { get; set; }

        //给每个step设置一个全局标志
        public int step1_time = 10;
        public int step2_time = 20;
        public int step3_time = 30;
        public int step4_time = 10;

        public int current_Lailiao_step = 0;
        public int current_Zuzhuang_step = 0;
        public int current_FuJian_step = 0;
        public int current_Reject_step = 0;

        public bool Lailiao_exit = false;
        public bool Zuzhuang_exit = false;
        public bool FuJian_exit = false;
        public bool Reject_exit = false;

        const int Lailiao_stepnum = 10;
        const int Zuzhuang_stepnum = 10;
        const int FuJian_stepnum = 10;
        const int Reject_stepnum = 10;
        public int Pausetime = 999999;

        public int[] Lailiao_state = new int[Lailiao_stepnum];
        public int[] Zuzhuang_state = new int[Zuzhuang_stepnum];
        public int[] FuJian_state = new int[FuJian_stepnum];
        public int[] Reject_state = new int[Reject_stepnum];

        public int[] Lailiao_delta = new int[Lailiao_stepnum];
        public int[] Zuzhuang_delta = new int[Zuzhuang_stepnum];
        public int[] FuJian_delta = new int[FuJian_stepnum];
        public int[] Reject_delta = new int[Reject_stepnum];


        public bool IsPass { get; set; }

        #endregion

        #region 全局IO信号

        public enum IO
        {
            None = 0,
            LaiLiao_BoardIn,
            LaiLiao_JianSu,
            LaiLiao_QiGang,
            LaiLiao_DingSheng,
            LaiLiao_BoardOut,

            ZuZhuang_BoardIn,
            ZuZhuang_JianSu,
            ZuZhuang_QiGang,
            ZuZhuang_DingSheng,
            ZuZhuang_BoardOut,

            FuJian_BoardIn,
            FuJian_JianSu,
            FuJian_QiGang,
            FuJian_DingSheng,
            FuJian_BoardOut,

            Reject_BoardIn,
            Reject_JianSu,
            Reject_QiGang,
            Reject_DingSheng,
            Reject_BoardOut,

            Total,
        }

        public bool[] IOTable = new bool[(int)IO.Total];

        #endregion

        public static GlobalManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new GlobalManager();
                }
                return _current;
            }
        }

        private void InitializeLaserData()
        {
            laser_data = new double[total_Assemble_Count][];
            for (int i = 0; i < TotalRow; i++)
            {
                laser_data[i] = new double[laser_point_length];
            }
        }

        public void Lailiao_CheckState()
        {
            if (GlobalManager.Current.Lailiao_state[current_Lailiao_step] == 0)
            {
                GlobalManager.Current.Lailiao_delta[current_Lailiao_step] = 0;
            }
            else
            {
                GlobalManager.Current.Lailiao_delta[current_Lailiao_step] = Pausetime;
            }
        }

        public void ZuZhuang_CheckState()
        {
            if (GlobalManager.Current.Zuzhuang_state[current_Zuzhuang_step] == 0)
            {
                GlobalManager.Current.Zuzhuang_delta[current_Zuzhuang_step] = 0;
            }
            else
            {
                GlobalManager.Current.Zuzhuang_delta[current_Zuzhuang_step] = Pausetime;
            }
        }
        public void FuJian_CheckState()
        {
            if (GlobalManager.Current.FuJian_state[current_FuJian_step] == 0)
            {
                GlobalManager.Current.FuJian_delta[current_FuJian_step] = 0;
            }
            else
            {
                GlobalManager.Current.FuJian_delta[current_FuJian_step] = Pausetime;
            }
        }

        public void Reject_CheckState()
        {
            if (GlobalManager.Current.Reject_state[current_Reject_step] == 0)
            {
                GlobalManager.Current.Reject_delta[current_Reject_step] = 0;
            }
            else
            {
                GlobalManager.Current.Reject_delta[current_Reject_step] = Pausetime;
            }
        }
        #region AGM800初始化以及状态显示
        // AGM800 控制器实例
        //public AGM800 _Agm800 { get; private set; }

        // 构造函数初始化 AGM800
        private GlobalManager()
        {
            //_Agm800 = new AGM800();

            // 初始化心跳定时器
            heartbeatTimer = new System.Timers.Timer(3000); // 每300ms触发一次
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            //heartbeatTimer.AutoReset = true; // 自动重复触发
            heartbeatTimer.Enabled = true;   // 启动定时器

            PosTimer = new System.Timers.Timer(200); // 每300ms触发一次
            PosTimer.Elapsed += PosTimer_Elapsed;
            //heartbeatTimer.AutoReset = true; // 自动重复触发
            PosTimer.Enabled = true;   // 启动定时器

            //StartErrorMonitor();

            InitializeLaserData();

            IsPause = false;

        }
        //与AGM800的连接状态
        private bool _agm800Connection;

        public bool AGM800Connection
        {
            get => _agm800Connection;
            set
            {
                if (_agm800Connection != value)
                {
                    _agm800Connection = value;
                }
            }
        }
        #endregion
        //第一次连接成功后再连心跳包，在297ms - 305 ms内，阻塞的处理
        #region 12
        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //if (AAmotionFAM.AGM800.Current.controller[0].IsConnected == false)
                //{
                //    //Console.WriteLine("监测到的状态是:" + false.ToString());
                //}

                //Console.WriteLine("连接状态:" + _Agm800.controller.IsConnected.ToString());
                //AGM800Connection = _Agm800.controller0.IsConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show("心跳包发生异常 : " + ex.ToString());
            }
        }

        private void PosTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //try
            //{
            //    if (AGM800.Current.controller[0].IsConnected)
            //    {
            //        AxisControlViewModel.Current.UpdateAxisPostion();
            //        //Debug.WriteLine("当前A轴位置:" + current_APos.ToString());
            //    }

            //}
            //catch (Exception ex)
            //{

            //}
        }
        #endregion

        public bool WaitIO(IO_INFunction_Table pos, int value)
        {
            int FeederRetry_Count = 0;
            int val = 0;
            if (value == 0) {
                val = 1;
            }
            while (IOManager.Instance.INIO_status[(int)pos] == val)
            {
                Thread.Sleep(30);
                FeederRetry_Count++;
                if (FeederRetry_Count > 10)
                {
                    return false;
                }
            }
            return true;
        }


        public AxisRef GetAxisRefFromInteger(int index)
        {
            switch (index)
            {
                    case 0: return AxisRef.A;
                    case 1: return AxisRef.B;
                    case 2: return AxisRef.C;
                    case 3: return AxisRef.D;
                    case 4: return AxisRef.E;
                    case 5: return AxisRef.F;            
                    case 6: return AxisRef.G;
                    case 7: return AxisRef.H;
                    default : return AxisRef.A; 

            }

          
        }

        public AxisName GetAxisNameFromInteger(int index)
        {
            switch (index)
            {
                case 1:
                    return AxisName.LSX;

                case 2:
                    return AxisName.LSY;

                case 3:
                    return AxisName.BL1;

                case 4:
                    return AxisName.BL2;

                case 5:
                    return AxisName.BL3;

                case 6:
                    return AxisName.BL4;

                case 7:
                    return AxisName.BL5;

                case 8:
                    return AxisName.BR1;

                case 9:
                    return AxisName.BR2;

                case 10:
                    return AxisName.BR3;

                case 11:
                    return AxisName.BR4;

                case 12:
                    return AxisName.BR5;

                case 13:
                    return AxisName.FSX;

                case 14:
                    return AxisName.FSY;

                case 15:
                    return AxisName.PICK1_Z;

                case 16:
                    return AxisName.PICK1_T;

                case 17:
                    return AxisName.PICK2_Z;

                case 18:
                    return AxisName.PICK2_T;

                case 19:
                    return AxisName.PICK3_Z;

                case 20:
                    return AxisName.PICK3_T;

                case 21:
                    return AxisName.PICK4_Z;

                case 22:
                    return AxisName.PICK4_T;

                case 23:
                    return AxisName.PRX;

                case 24:
                    return AxisName.PRY;

                case 25:
                    return AxisName.PRZ;

                default:
                    return AxisName.LSX;
            }

        }


        public AxisName GetAxisNameFromString(string line)
        {
            switch (line) 
            {
                case "LSX":
                    return AxisName.LSX;

                case "LSY":
                    return AxisName.LSY;

                case "FSX":
                    return AxisName.FSX;

                case "FSY":
                    return AxisName.FSY;

                case "BL1":
                    return AxisName.BL1;

                case "BL2":
                    return AxisName.BL2;
                case "BL3":
                    return AxisName.BL3;
                case "BL4":
                    return AxisName.BL4;
                case "BL5":
                    return AxisName.BL5;
                case "BR1":
                    return AxisName.BR1;
                case "BR2":
                    return AxisName.BR2;
                case "BR3":
                    return AxisName.BR3;
                case "BR4":
                    return AxisName.BR4;
                case "BR5":
                    return AxisName.BR5;
                case "PICK1_Z":
                    return AxisName.PICK1_Z;
                case "PICK1_T":
                    return AxisName.PICK1_T;
                case "PICK2_Z":
                    return AxisName.PICK2_Z;
                case "PICK2_T":
                    return AxisName.PICK2_T;
                case "PICK3_Z":
                    return AxisName.PICK3_Z;
                case "PICK3_T":
                    return AxisName.PICK3_T;
                case "PICK4_Z":
                    return AxisName.PICK4_Z;
                case "PICK4_T":
                    return AxisName.PICK4_T;
                case "PRX":
                    return AxisName.PRX;
                case "PRY":
                    return AxisName.PRY;
                case "PRZ":
                    return AxisName.PRZ;
                default:
                    return AxisName.PRZ;
            }
        }


        public string GetAxisStringFromInteger(int index)
        {
            switch (index)
            {
                case 1:
                    return "LSX";

                case 2:
                    return "LSY";

                case 3:
                    return "BL1";

                case 4:
                    return "BL2";

                case 5:
                    return "BL3";

                case 6:
                    return "BL4";

                case 7:
                    return "BL5";

                case 8:
                    return "BR1";

                case 9:
                    return "BR2";

                case 10:
                    return "BR3";

                case 11:
                    return "BR4";

                case 12:
                    return "BR5";

                case 13:
                    return "FSX";

                case 14:
                    return "FSY";

                case 15:
                    return "PICK1_Z";

                case 16:
                    return "PICK1_T";

                case 17:
                    return "PICK2_Z";

                case 18:
                    return "PICK2_T";

                case 19:
                    return "PICK3_Z";

                case 20:
                    return "PICK3_T";

                case 21:
                    return "PICK4_Z";

                case 22:
                    return "PICK4_T";

                case 23:
                    return "PRX";

                case 24:
                    return "PRY";

                case 25:
                    return "PRZ";

                default:
                    return "LSX";
            }

        }


        #region A,B轴状态
        public void UpdateAStatus()
        {
            //Enum.TryParse<AxisRef>("A", out AxisRef axisRef);
            //IsAInTarget = GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).InTargetStat == 4;
        }

        // 更新 B 轴状态
        public void UpdateBStatus()
        {
            //Enum.TryParse<AxisRef>("B", out AxisRef axisRef);
            //IsBInTarget = GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).InTargetStat == 4;
        }
        #endregion

        //private void StartErrorMonitor()
        //{
        //    Console.WriteLine("开启全局错误监视器");
        //    _errorCheckTimer = new DispatcherTimer();
        //    _errorCheckTimer.Interval = TimeSpan.FromSeconds(1);
        //    _errorCheckTimer.Tick += (s, e) =>
        //    {
        //        while (ErrorReportManager.ErrorQueue.TryDequeue(out var ex))
        //        {
        //            MessageBox.Show(ex.Message, "线程异常", MessageBoxButton.OK, MessageBoxImage.Error);

        //            // 可选：终止运行
        //            AutorunManager.Current.isRunning = false;
        //        }
        //    };
        //    _errorCheckTimer.Start();
        //}
        // 可选：终止运行
        //            AutorunManager.Current.isRunning = false;
        //        }
        //    };
        //    _errorCheckTimer.Start();
        //}

        //public void InitializeAxisMode()
        //{
        //    try
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.A).MotionMode = 11;
        //        GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.B).MotionMode = 11;
        //        GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.C).MotionMode = 11;
        //        GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.D).MotionMode = 11;

        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).ClearBuffer();
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).ClearBuffer();
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(ex.ToString());
        //    }

        //}

        public enum AxisName
        {
            //AGM800[0]
            LSX = 0,
            LSY = 1,
            FSX = 2,
            FSY = 3,
            BL5 = 4,
            BR5 = 5,

            //AGM800[1]
            BL1 = 8,
            BL2 = 9,
            BL3 = 10,
            BL4 = 11,
            BR1 = 12,
            BR2 = 13,
            BR3 = 14,
            BR4 = 15,

            //AGM800[2]
            PICK1_Z = 16,
            PICK1_T = 17,
            PICK2_Z = 18,
            PICK2_T = 19,
            PICK3_Z = 20,
            PICK3_T = 21,
            PICK4_Z = 22,
            PICK4_T = 23,

            //AGM800[3]
            PRX = 24,
            PRY = 25,
            PRZ = 26,

        }
        public enum AxisSpeed
        {
            //AGM800[0]
            LSX = 100,
            LSY = 100,
            FSX = 50,
            FSY = 50,
            BL5 = 100,
            BR5 = 100,

            //AGM800[1]
            BL1 = 100,
            BL2 = 100,
            BL3 = 100,
            BL4 = 100,
            BR1 = 100,
            BR2 = 100,
            BR3 = 100,
            BR4 = 100,

            //AGM800[2]
            PICK1_Z = 20,
            PICK1_T = 90,
            PICK2_Z = 20,
            PICK2_T = 90,
            PICK3_Z = 20,
            PICK3_T = 90,
            PICK4_Z = 20,
            PICK4_T = 90,

            //AGM800[3]
            PRX = 200,
            PRY = 200,
            PRZ = 30,
        }
        public enum AxisAcc
        {
            //AGM800[0]
            LSX = 1000,
            LSY = 1000,
            FSX = 500,
            FSY = 500,
            BL5 = 500,
            BR5 = 500,

            //AGM800[1]
            BL1 = 800,
            BL2 = 800,
            BL3 = 800,
            BL4 = 800,
            BR1 = 800,
            BR2 = 800,
            BR3 = 800,
            BR4 = 800,

            //AGM800[2]
            PICK1_Z = 50,
            PICK1_T = 50,
            PICK2_Z = 50,
            PICK2_T = 50,
            PICK3_Z = 50,
            PICK3_T = 50,
            PICK4_Z = 50,
            PICK4_T = 50,

            //AGM800[3]
            PRX = 2000,
            PRY = 2000,
            PRZ = 300,
        }

        //轴参数
        public AxisParams axisparams = new AxisParams();

    }
}
[DataContract]
public class SinglePoint
{
    [DataMember]
    public double X { get; set; }
    [DataMember]
    public double Y { get; set; }
    [DataMember]
    public double Z { get; set; }
    [DataMember]
    public double R { get; set; }
}
