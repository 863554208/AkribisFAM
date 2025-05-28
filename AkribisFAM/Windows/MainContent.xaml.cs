using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;
using Microsoft.Win32;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class MainContent : UserControl
    {

        public MainContent()
        {
            InitializeComponent();

            ScanningGunValue.Text = "ABC12345678911111";
            NozzleForce1Value.Text = "112.3N";
            NozzleForce2Value.Text = "11.8N";
            NozzleForce3Value.Text = "12.1N";
            NozzleForce4Value.Text = "11.9N";
        }


    }
}
