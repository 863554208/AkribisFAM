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

namespace AkribisFAM
{
    public class GlobalManager
    {

        // 单例模式，确保全局只有一个实例
        private static GlobalManager _current;

        //全局心跳包
        private Timer heartbeatTimer;

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
            heartbeatTimer = new Timer(1000); // 每300ms触发一次
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            //heartbeatTimer.AutoReset = true; // 自动重复触发
            heartbeatTimer.Enabled = true;   // 启动定时器

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
    }
}
