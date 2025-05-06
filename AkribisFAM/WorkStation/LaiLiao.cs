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
        public override string Name => nameof(LaiLiao);

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
                    //
                    if (GlobalManager.Current.IO_test1)
                    {
                        //TODO
                    }
                }
            }
            catch (Exception ex)
            { 
            
            }
        }
    }
}
