using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AkribisFAM.AAmotionFAM;

namespace AkribisFAM
{
    public class GlobalManager
    {

        // 单例模式，确保全局只有一个实例
        private static GlobalManager _current;
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

        // AGM800 控制器实例
        public AGM800 _Agm800 { get; private set; }


        public string _text { get; private set; }

        public string Text
        {
            get
            {
                if (_text == null)
                {
                    _text = "test";
                }
                return _text;
            }
        }

        // 构造函数初始化 AGM800
        private GlobalManager()
        {
            _Agm800 = new AGM800();
        }
    }
}
