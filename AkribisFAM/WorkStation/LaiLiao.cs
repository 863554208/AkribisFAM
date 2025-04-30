using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.WorkStation
{
    internal class LaiLiao : WorkStationBase
    {
        private static LaiLiao _instance;

        public static LaiLiao Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new LaiLiao();
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
            bool has_daowei = false;
            int WorkState = 11;
            try
            {
                while (true)
                {
                    //要料
                    //if(Global.IO_1 == false  &&   has_board==false  )
                    //{
                    //    System.Threading.Thread.Sleep(10);
                    //    continue;
                    //}

                    //has_board = true;
                    ////有料了

                    ////控制步进电机运动

                    ////
                    //if (!has_daowei)
                    //{
                    //    //TODO AMotion 控制移动;
                    //    Console.WriteLine("正在执行到位");
                    //}
                    //has_daowei = true;

                    ////
                    //if (IO _到位 != true) 
                    //{
                    //    continue;
                    //}

                    //测距

                    //送走
                    has_board = false;
                    has_daowei = false;

                }
            }
            catch (Exception ex) { }
        }
    }
}
