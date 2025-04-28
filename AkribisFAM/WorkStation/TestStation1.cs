using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.WorkStation
{
    public class TestStation1 :WorkStationBase
    {
        private static TestStation1 _instance;

        public static TestStation1 Current
        {
            get
            {
                // 双重锁定检查，确保线程安全并且只创建一次实例
                if (_instance == null)
                {
                        if (_instance == null)
                        {
                            _instance = new TestStation1();
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
            
        }
    }
}
