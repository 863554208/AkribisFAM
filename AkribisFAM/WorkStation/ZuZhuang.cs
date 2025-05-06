using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AkribisFAM.Manager;

namespace AkribisFAM.WorkStation
{
    internal class ZuZhuang : WorkStationBase
    {
        private static ZuZhuang _instance;
        public override string Name => nameof(ZuZhuang);

        public static ZuZhuang Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new ZuZhuang();
                    }
                }
                return _instance;
            }
        }

        public override void ReturnZero()
        {
            throw new NotImplementedException();
        }


        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool Ready()
        {
            return true;
        }

        public override void AutoRun()
        {
            bool has_board = false;
            bool has_error = false;
            int WorkState = 0;
            try
            {
                while (true)
                {
                    if (GlobalManager.Current.IO_test2 && !has_board)
                    {
                        WorkState = 1;
                        has_board = true;
                        Console.WriteLine("板已进");
                    }

                    // 处理板
                    if (has_board && WorkState == 1)
                    {
                        try
                        {
                            WorkState = 2;
                            GlobalManager.Current.current_Assembled = 0;
                            GlobalManager.Current.current_FOAM_Count = 0;
                            while (GlobalManager.Current.current_Assembled < GlobalManager.Current.total_Assemble_Count) 
                            {

                                if(GlobalManager.Current.current_FOAM_Count == 0)
                                {
                                    //TODO 相机拍飞达上的料

                                    //TODO 吸嘴吸取飞达上的4个料

                                    //现在吸嘴上实际吸了4个料
                                    GlobalManager.Current.current_FOAM_Count += 4; 
                                }

                                //TODO 相机到CCD2拍照精定位

                                if (!GlobalManager.Current.has_XueWeiXinXi)
                                {
                                    //TODO 对料盘拍照获取穴位信息
                                }

                                //TODO 组装

                                //目前料盘上组装了多少料
                                GlobalManager.Current.current_Assembled += 4;

                                //吸嘴上现在有多少foam（减去实际贴上去的料的数量） ： 如果没有foam，下一片板子走正常流程 ；如果有foam , 不再拍feeder上的料的图片
                                GlobalManager.Current.current_FOAM_Count -= 4;
                            }

                            WorkState = 3; // 更新状态为出板
                        }
                        catch (Exception ex)
                        {
                            has_error = true; // 标记为出错
                        }
                    }

                    // 出板
                    if (WorkState == 3 || has_error)
                    {
                        if (has_error)
                        {
                            AutorunManager.Current.isRunning = false;
                        }

                        WorkState = 0;
                        has_board = false;
                        Console.WriteLine("板已出");
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }

    }
}
