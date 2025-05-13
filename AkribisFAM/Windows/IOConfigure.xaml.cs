using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using AkribisFAM.CommunicationProtocol;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// IOConfigure.xaml 的交互逻辑
    /// </summary>
    public partial class IOConfigure : UserControl
    {
        public IOConfigure()
        {
            InitializeComponent();
        }


        //private void INOUTIOshow()
        //{
        //    Task.Run(new Action(() =>
        //    {
        //        while (true)
        //        {
        //            //显示在UI界面//输入
        //            this.Label5.Dispatcher.BeginInvoke(new Action<bool>((IO_INstatus) =>
        //            {
        //                this.Label5.Background = IO_INstatus ? Brushes.Green : Brushes.Red;

        //            }), IOManager.Instance.INIO_status[(int)IO_INFunction_Table.Stop_Sign1]);


        //            this.Label9.Dispatcher.BeginInvoke(new Action<bool>((IO_INstatus) =>
        //            {

        //                this.Label9.Background = IO_INstatus ? Brushes.Green : Brushes.Red;
        //            }), IOManager.Instance.INIO_status[(int)IO_INFunction_Table.NG_cover_plate1]);


        //            //输出
        //            this.Button9.Dispatcher.BeginInvoke(new Action<bool>((IO_OUTstatus) =>
        //            {

        //                this.Button9.Background = IO_OUTstatus ? Brushes.Green : Brushes.Red;
        //            }), IOManager.Instance.OutIO_status[(int)IO_OutFunction_Table.Left_3_lift_cylinder_extend]);

        //            this.Button11.Dispatcher.BeginInvoke(new Action<bool>((IO_OUTstatus) =>
        //            {

        //                this.Button11.Background = IO_OUTstatus ? Brushes.Green : Brushes.Red;
        //            }), IOManager.Instance.OutIO_status[(int)IO_OutFunction_Table.Right_3_lift_cylinder_extend]);

        //            Thread.Sleep(100);
        //        }
        //    }));
        //}




    }
}
