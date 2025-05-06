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

namespace AkribisFAM
{
    public class GlobalManager
    {

        // 单例模式，确保全局只有一个实例
        private static GlobalManager _current;

        //全局心跳包
        private Timer heartbeatTimer;

        //错误队列
        private DispatcherTimer _errorCheckTimer;


        // 记录 A 轴和 B 轴的是否到位的状态
        public bool IsAInTarget { get; set; }
        public bool IsBInTarget { get; set; }

        //模拟进板位置有料和无料IO信号
        public bool IO_test1 { get; set; }

        public bool IsPause { get; set; }
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


        #region AGM800初始化以及状态显示
        // AGM800 控制器实例
        public AGM800 _Agm800 { get; private set; }

        // 构造函数初始化 AGM800
        private GlobalManager()
        {
            _Agm800 = new AGM800();


            // 初始化心跳定时器
            heartbeatTimer = new Timer(3000); // 每300ms触发一次
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            //heartbeatTimer.AutoReset = true; // 自动重复触发
            heartbeatTimer.Enabled = true;   // 启动定时器

            StartErrorMonitor();


            IsAInTarget = false;
            IsBInTarget = false;
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
        #region 1·
        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if(_Agm800.controller.IsConnected == false)
                {
                    Console.WriteLine("监测到的状态是:" + false.ToString());
                }

                //Console.WriteLine("连接状态:" + _Agm800.controller.IsConnected.ToString());
                AGM800Connection = _Agm800.controller.IsConnected;
            }
            catch(Exception ex)
            {
                MessageBox.Show("心跳包发生异常 : " + ex.ToString());
            }
        }
        #endregion

        #region A,B轴状态
        public void UpdateAStatus()
        {
            Enum.TryParse<AxisRef>("A", out AxisRef axisRef);
            IsAInTarget = GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat == 4;
        }

        // 更新 B 轴状态
        public void UpdateBStatus()
        {
            Enum.TryParse<AxisRef>("B", out AxisRef axisRef);
            IsBInTarget = GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat == 4;
        }
        #endregion

        private void StartErrorMonitor()
        {
            Console.WriteLine("开启全局错误监视器");
            _errorCheckTimer = new DispatcherTimer();
            _errorCheckTimer.Interval = TimeSpan.FromSeconds(1);
            _errorCheckTimer.Tick += (s, e) =>
            {
                while (ErrorReportManager.ErrorQueue.TryDequeue(out var ex))
                {
                    MessageBox.Show(ex.Message, "线程异常", MessageBoxButton.OK, MessageBoxImage.Error);

                    // 可选：终止运行
                    AutorunManager.Current.isRunning = false;
                }
            };
            _errorCheckTimer.Start();
        }
    }
}
