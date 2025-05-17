using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using LiveCharts;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// ParameterConfig.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterConfig : UserControl
    {

        public ParameterConfig()
        {
            InitializeComponent();
            ReadAxisParamJson();
        }

        private void DoubleText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.Handled =!( new Regex(@"^\d*\.?\d*$").IsMatch(e.Text));
            Regex regex = new Regex("[^0-9\\-\\.]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        /// <summary>
        /// 从指定json文件加载配置信息字典
        /// </summary>
        /// <param name="_config"></param>
        /// <returns></returns>
        public int LoadConfig(string jsonfile)
        {
            if (string.IsNullOrEmpty(jsonfile))
            {
                return 1;
            }
            try
            {
                string content = File.ReadAllText(jsonfile);
                GlobalManager.Current.axisparams = JsonConvert.DeserializeObject<AxisParams>(content);
                if (GlobalManager.Current.axisparams == null)
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 将配置信息字典保存至指定json文件
        /// </summary>
        /// <param name="_config"></param>
        /// <returns></returns>
        public bool SaveToJson(string _config)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                string content = JsonConvert.SerializeObject(GlobalManager.Current.axisparams, settings);
                File.WriteAllText(_config, content);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in GlobalManager.Current.axisparams.AxisSpeedDict.Keys.ToList())
            {
                string speedname = key + "_Speed";
                TextBox tbspeed = (TextBox)FindObject(speedname);
                GlobalManager.Current.axisparams.AxisSpeedDict[key] = (int)(double.Parse(tbspeed.Text) * GlobalManager.Current.coef);
            }
            foreach (var key in GlobalManager.Current.axisparams.AxisAccDict.Keys.ToList())
            {
                string accname = key + "_Acc";
                TextBox tbacc = (TextBox)FindObject(accname);
                GlobalManager.Current.axisparams.AxisAccDict[key] = (int)(double.Parse(tbacc.Text) * GlobalManager.Current.coef);
            }
            foreach (var key in GlobalManager.Current.axisparams.AxisDecDict.Keys.ToList())
            {
                string decname = key + "_Dec";
                TextBox tbdec = (TextBox)FindObject(decname);
                GlobalManager.Current.axisparams.AxisDecDict[key] = (int)(double.Parse(tbdec.Text) * GlobalManager.Current.coef);
            }
            string path = Directory.GetCurrentDirectory() + "\\AxisParams.json";
            SaveToJson(path);
        }

        private Object FindObject(string name)
        {
            Object obj = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
            return obj;
        }

        private void ReadAxisParamJson() {
            string folder = Directory.GetCurrentDirectory(); //获取应用程序的当前工作目录。 
            string path = folder + "\\AxisParams.json";

            LoadConfig(path);
            foreach (var item in GlobalManager.Current.axisparams.AxisSpeedDict)
            {
                string speedname = item.Key + "_Speed";
                TextBox tbspeed = (TextBox)FindObject(speedname);
                tbspeed.Text = ((double)item.Value / GlobalManager.Current.coef).ToString();
            }
            foreach (var item in GlobalManager.Current.axisparams.AxisAccDict)
            {
                string accname = item.Key + "_Acc";
                TextBox tbacc = (TextBox)FindObject(accname);
                tbacc.Text = ((double)item.Value / GlobalManager.Current.coef).ToString();
            }
            foreach (var item in GlobalManager.Current.axisparams.AxisDecDict)
            {
                string decname = item.Key + "_Dec";
                TextBox tbdec = (TextBox)FindObject(decname);
                tbdec.Text = ((double)item.Value / GlobalManager.Current.coef).ToString();
            }
        }

        private int first = 1;
        private int station1Init = 0;
        private int station2Init = 0;
        private int station3Init = 0;
        private int station4Init = 0;
        private int station1Finished = 1;
        private int station2Finished = 1;
        private int station3Finished = 1;
        private int station4Finished = 1;

        private void moveforward(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft > endpos)
                    break;
            }
        }

        private void move2forward(Rectangle rect, Rectangle rect1, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                    rect1.Margin = new Thickness(mleft, rect1.Margin.Top, rect1.Margin.Right, rect1.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft > endpos)
                    break;
            }
        }

        private void movebackward(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mleft = startpos;
            while (true)
            {
                mleft -= 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(mleft, rect.Margin.Top, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mleft < endpos)
                    break;
            }
        }

        private void movedown(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            while (true)
            {
                mtop += 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(rect.Margin.Left, mtop, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mtop > endpos)
                    break;
            }
        }

        private void moveup(Rectangle rect, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            while (true)
            {
                mtop -= 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Margin = new Thickness(rect.Margin.Left, mtop, rect.Margin.Right, rect.Margin.Bottom);
                }));
                Thread.Sleep(interval);
                if (mtop < endpos)
                    break;
            }
        }

        private void moveCanvasV(Canvas group, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            if (startpos - endpos >= 0)
            {
                while (true)
                {
                    mtop -= 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(group.Margin.Left, mtop, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop < endpos)
                        break;
                }
            }
            else {
                while (true)
                {
                    mtop += 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(group.Margin.Left, mtop, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop > endpos)
                        break;
                }
            }
        }

        private void moveCanvasH(Canvas group, int startpos, int endpos, int interval)
        {
            int mtop = startpos;
            if (startpos - endpos >= 0)
            {
                while (true)
                {
                    mtop -= 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(mtop, group.Margin.Top, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop < endpos)
                        break;
                }
            }
            else
            {
                while (true)
                {
                    mtop += 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        group.Margin = new Thickness(mtop, group.Margin.Top, group.Margin.Right, group.Margin.Bottom);
                    }));
                    Thread.Sleep(interval);
                    if (mtop > endpos)
                        break;
                }
            }
        }

        private int flag = 1;

        private void returnOK(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag1 = 1;

        private void returnOK1(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag1 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag2 = 1;

        private void returnOK2(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag2 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag3 = 1;

        private void returnOK3(Rectangle rect)
        {
            if (flag3 == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.Yellow);
                }));
            }
            else if (flag3 == 2)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.Green);
                }));
            }
            else if (flag3 == 3)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.Red);
                }));
            }
            while (flag3 > 0)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag4 = 1;

        private void returnOK4(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag4 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag5 = 1;

        private void returnOK5(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag5 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag6 = 1;

        private void returnOK6(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag6 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag7 = 1;

        private void returnOK7(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag7 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag8 = 1;

        private void returnOK8(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag8 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag9 = 1;

        private void returnOK9(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag9 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag10 = 1;

        private void returnOK10(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag10 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag11 = 1;

        private void returnOK11(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag11 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag12 = 1;

        private void returnOK12(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag12 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag13 = 1;

        private void returnOK13(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag13 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
                rect1.Fill = null;
            }));
        }

        private int flag14 = 1;

        private void returnOK14(Rectangle rect, Rectangle rect1)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect1.Fill = new SolidColorBrush(Colors.Red);
            }));
            while (flag14 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
                rect1.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private int flag15 = 1;

        private void returnOK15(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag15 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
        }

        private int flag17 = 1;

        private void returnOK17(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag17 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag18 = 1;

        private void returnOK18(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag18 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag19 = 1;

        private void returnOK19(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag19 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private int flag20 = 1;

        private void returnOK20(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            while (flag20 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }
        private void returnNG(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Red);
            }));
            Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            //station1Init = 1;
            //Task task1 = new Task(Station1Act);
            //task1.Start();
            //Task task2 = new Task(Station2Act);
            //task2.Start();
            //Task task3 = new Task(Station3Act);
            //task3.Start();
        }

        int deltatime = 0;
        private void wait()
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                deltatime = 999999;
            }

            while (true)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = deltatime - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }
        }



        private int module_Num = 12;
        const int numberofstation = 4;
        private int[] By_pass = new int[numberofstation] { 0, 0, 0, 0 };
        private int[] By_pass_index = new int[numberofstation] { 0, 0, 0, 0 };
        private int current_index = 0;
        private void LailiaoAct(Rectangle rect, Rectangle rect1)
        {
            Task task9, task4;
            current_index++;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Yellow);
            }));
            //pallet in
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in";
            }));
            beltmoveflag[0] = 1;
            move2forward(rect, rect1, 10, 69, 20);
            beltmoveflag[0] = 0;
            wait();
            //trigger jiansu IO
            flag = 1;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.LaiLiao_JianSu] = true;
            Task task1 = new Task(() => returnOK(rect20, rect51));
            task1.Start();
            wait();
            //send dingqi IO
            task9 = new Task(() => moveup(rect52, 355, 345, 20));
            task9.Start();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            flag1 = 1;
            Task task2 = new Task(() => returnOK1(rect21, rect52));
            task2.Start();
            wait();
            beltmoveflag[0] = 1;
            move2forward(rect, rect1, 69, 101, 20);
            beltmoveflag[0] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
        step2:
            //scan扫码
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "start scan pallet";
            }));
            flag2 = 1;
            Task task3 = new Task(() => returnOK2(rect22));
            task3.Start();
            Thread.Sleep(3000);
            wait();
            flag2 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "get scan result";
            }));
            Thread.Sleep(1000);
            By_pass_index[0] = current_index;
            By_pass[0] = GenerateRandomNumber(0, 2);
            if (By_pass[0] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                flag3 = 3;
                task4 = new Task(() => returnOK3(rect));
                task4.Start();
                Thread.Sleep(1000);
                goto step5;
            }
        step3:
            //pallet lift up
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            flag3 = 1;
            task4 = new Task(() => returnOK3(rect));
            task4.Start();
            Thread.Sleep(1000);
            wait();
        step4:
            //laser measure激光
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "start laser measure";
            }));
            for (int i = 0; i < module_Num; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action5.Content = $"laser go to module{i + 1} position{j + 1} and trigger laser";
                    }));
                    flag4 = 1;
                    task3 = new Task(() => returnOK4(rect25));
                    task3.Start();
                    Thread.Sleep(200);
                    wait();
                    flag4 = 0;
                }
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "laser measure finished";
            }));
            Thread.Sleep(1000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
        step5:
            task9 = new Task(() => movedown(rect52, 345, 355, 20));
            task9.Start();
            flag1 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();

        }

        public int GenerateRandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private int current_Assembled = 0;
        private int Picker_FOAM_Count = 0;
        private int Picker_OK_FOAM_Count = 0;
        private int BadFoamCount = 0;
        private int has_XueWeiXinXi = 0;
        private int NG_Foam_Count = 0;

        private void ZuzhuangAct(Rectangle rect, Rectangle rect1)
        {
            By_pass_index[1] = By_pass_index[0];
            By_pass[1] = By_pass[0];
            Task task1, task2, task3, task4, task5, task6, task8, task9;
            double xpos, ypos;
            //move to assembly
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            flag17 = 1;
            task8 = new Task(() => returnOK17(rect59));
            task8.Start();
            beltmoveflag[1] = 1;
            move2forward(rect, rect1, 101, 143, 20);
            flag = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.LaiLiao_JianSu] = false;
            move2forward(rect, rect1, 143, 174, 20);
            flag17 = 0;
            move2forward(rect, rect1, 174, 253, 20);
            beltmoveflag[1] = 0;
            wait();
            //trigger jiansu IO
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.ZuZhuang_JianSu] = true;
            flag5 = 1;
            task1 = new Task(() => returnOK5(rect23, rect53));
            task1.Start();
            if (By_pass[1] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                beltmoveflag[1] = 1;
                move2forward(rect, rect1, 253, 290, 20);
                beltmoveflag[1] = 0;
                Thread.Sleep(100);
                wait();
                return;
            }
            //send dingqi IO
            task9 = new Task(() => moveup(rect54, 355, 345, 20));
            task9.Start();
            flag6 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK6(rect24, rect54));
            task2.Start();
            wait();
            beltmoveflag[1] = 1;
            move2forward(rect, rect1, 253, 290, 20);
            beltmoveflag[1] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //pallet lift up
            flag7 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            task4 = new Task(() => returnOK7(rect));
            task4.Start();
            wait();
            current_Assembled = 0;
            //如果吸嘴上有料，直接跳去CCD2精定位
            if (Picker_FOAM_Count > 0)
            {
                goto step4;
            }
        step2:
            //飞达上拍料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to feeder";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task3 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 54, 20));
            task3.Start();
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 217, 20));
            task5.Start();
            Task.WaitAll(task3, task5);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Thread.Sleep(1000);
            wait();
        step3:
            //吸嘴取料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "pick foams";
                picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length - 1) + "X";
            }));
            flag8 = 1;
            task3 = new Task(() => returnOK8(rect26));
            task3.Start();
            Thread.Sleep(1000);
            wait();
        step4:
            //CCD2 精定位
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to CCD2";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 117, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 248, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Thread.Sleep(1000);
            wait();
            if (Picker_FOAM_Count == 0)
            {
                BadFoamCount = GenerateRandomNumber(0, 2);
                Picker_FOAM_Count = GenerateRandomNumber(2, 5);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length-1) + $"{Picker_FOAM_Count}";
                }));
                if (BadFoamCount > 0)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action4.Content = $"{BadFoamCount} foam(s) is NG";
                    }));
                    goto step5;
                }
                else
                {
                    goto step6;
                }
            }
            else
            {
                goto step6;
            }
        step5:
            //如果有坏料，放到坏料盒里
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to CCD2";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 117, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 184, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            wait();
            for (int i = 0; i < BadFoamCount; ++i)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action4.Content = $"throw NG faom {i + 1}";
                    Picker_FOAM_Count = Picker_FOAM_Count - 1;
                    picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length - 1) + $"{Picker_FOAM_Count}";
                }));
                Thread.Sleep(1000);
                wait();
                NG_Foam_Count++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    NGnum.Content = $"{NG_Foam_Count}";
                }));
            }
        step6:
            //拍料盘
            if (has_XueWeiXinXi == 1)
            {
                goto step7;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to pallet";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 199, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 277, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            has_XueWeiXinXi = 1;
            Thread.Sleep(1000);
            wait();
        step7:
            //放料
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "robot move to pallet";
            }));
            xpos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Left));
            ypos = (double)CCD1picker.Dispatcher.Invoke(new Func<double>(() => CCD1picker.Margin.Top));
            task6 = new Task(() => moveCanvasV(CCD1picker, (int)ypos, 199, 20));
            task5 = new Task(() => moveCanvasH(CCD1picker, (int)xpos, 277, 20));
            task5.Start();
            task6.Start();
            Task.WaitAll(task6, task5);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "place faoms";
            }));
            int cnt = 0;
            while (Picker_FOAM_Count > 0)
            {
                Thread.Sleep(1000);
                wait();
                cnt++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action4.Content = $"place faom {cnt}";
                }));
                Picker_FOAM_Count--;
                current_Assembled++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    palletfaomnum.Content = $"{current_Assembled} ";
                    picker.Content = picker.Content.ToString().Substring(0, picker.Content.ToString().Length - 1) + $"{Picker_FOAM_Count}";
                }));
                if (current_Assembled >= module_Num)
                {
                    break;
                }
            }
            if (current_Assembled < module_Num)
            {
                flag8 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = "picker has no foams ";
                }));
                Thread.Sleep(1000);
                wait();
                goto step2;
            }
            if (Picker_FOAM_Count == 0)
            {
                flag8 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = "picker has no foams ";
                }));
                Thread.Sleep(1000);
                wait();
            }
            current_Assembled = 0;
            has_XueWeiXinXi = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                palletfaomnum.Content = " ";
            }));
            flag7 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            task9 = new Task(() => movedown(rect54, 345, 355, 20));
            task9.Start();
            flag6 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();
        }

        private int Left_Foam_Count = 0;
        private int Fujian_OK = 0;
        private void FujianAct(Rectangle rect, Rectangle rect1)
        {
            By_pass_index[2] = By_pass_index[1];
            By_pass[2] = By_pass[1];
            Task task1, task2, task3, task4, task8, task9;
            //move to next
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            flag18 = 1;
            task8 = new Task(() => returnOK18(rect60));
            task8.Start();
            beltmoveflag[2] = 1;
            move2forward(rect, rect1, 290, 326, 20);
            flag5 = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.ZuZhuang_JianSu] = false;
            move2forward(rect, rect1, 326, 363, 20);
            flag18 = 0;
            move2forward(rect, rect1, 363, 428, 20);
            beltmoveflag[2] = 0;
            wait();
            //trigger jiansu IO
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] = true;
            flag9 = 1;
            task1 = new Task(() => returnOK9(rect28, rect55));
            task1.Start();
            if (By_pass[2] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                beltmoveflag[2] = 1;
                move2forward(rect, rect1, 428, 467, 20);
                beltmoveflag[2] = 0;
                Thread.Sleep(100);
                wait();
                return;
            }
            //send dingqi IO
            task9 = new Task(() => moveup(rect56, 355, 345, 20));
            task9.Start();
            flag10 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK10(rect27, rect56));
            task2.Start();
            wait();
            beltmoveflag[2] = 1;
            move2forward(rect, rect1, 428, 467, 20);
            beltmoveflag[2] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
            //pallet lift up
            flag11 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift up";
            }));
            task4 = new Task(() => returnOK11(rect));
            task4.Start();
            wait();
            Left_Foam_Count = module_Num;
        step2:
            //撕膜
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "start to tear film";
            }));
            for (int i = 0; i < module_Num; ++i)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action5.Content = $"robot go to module{i + 1} and tear film";
                }));
                flag12 = 1;
                task3 = new Task(() => returnOK12(rect29));
                task3.Start();
                Thread.Sleep(1000);
                wait();
                Left_Foam_Count--;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Leftfoam.Content = $"{Left_Foam_Count}";
                }));
                flag12 = 0;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = "";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "tear film finished";
            }));
            Thread.Sleep(1000);
            wait();
        step3:
            //CCD3复检
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "flying photography";
            }));
            Thread.Sleep(3000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = "get result of flying photography";
            }));
            Fujian_OK = GenerateRandomNumber(0, 2);
            //flag11 = 1;
            //task4 = new Task(() => returnOK11(rect));
            //task4.Start();
            Thread.Sleep(1000);
            wait();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Leftfoam.Content = " ";
            }));
            flag11 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet lift down";
            }));
            Thread.Sleep(1000);
            wait();
            task9 = new Task(() => movedown(rect56, 345, 355, 20));
            task9.Start();
            flag10 = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO off";
            }));
            Thread.Sleep(1000);
            wait();
        }

        private void RejectAct(Rectangle rect, Rectangle rect1)
        {
            By_pass_index[3] = By_pass_index[2];
            By_pass[3] = By_pass[2];
            Task task1, task2, task3, task4, task8, task9;
            //move to next
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet go to next station";
            }));
            flag19 = 1;
            task8 = new Task(() => returnOK19(rect61));
            task8.Start();
            beltmoveflag[3] = 1;
            move2forward(rect, rect1, 467, 500, 20);
            flag9 = 0;
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] = false;
            move2forward(rect, rect1, 500, 541, 20);
            flag19 = 0;
            move2forward(rect, rect1, 541, 612, 20);
            beltmoveflag[3] = 0;
            wait();
            //trigger jiansu IO
            GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = true;
            flag13 = 1;
            task1 = new Task(() => returnOK13(rect31, rect57));
            task1.Start();
            if (By_pass[3] == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet ByPass";
                }));
                beltmoveflag[3] = 1;
                move2forward(rect, rect1, 612, 646, 20);
                flag20 = 1;
                task8 = new Task(() => returnOK20(rect62));
                task8.Start();
                move2forward(rect, rect1, 646, 685, 20);
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                flag13 = 0;
                move2forward(rect, rect1, 685, 719, 20);
                flag20 = 0;
                move2forward(rect, rect1, 719, 740, 20);
                beltmoveflag[3] = 0;
                Thread.Sleep(1000);
                wait();
                return;
            }
            //send dingqi IO
            task9 = new Task(() => moveup(rect58, 355, 345, 20));
            task9.Start();
            flag14 = 1;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = "send cylinder IO on";
            }));
            task2 = new Task(() => returnOK14(rect30, rect58));
            task2.Start();
            wait();
            beltmoveflag[3] = 1;
            move2forward(rect, rect1, 612, 646, 20);
            beltmoveflag[3] = 0;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = "pallet in place";
            }));
            wait();
        step2:
            if (Fujian_OK == 0)
            {
                //NG顶升
                flag15 = 1;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "NG pallet lift up";
                }));
                task4 = new Task(() => returnOK15(rect));
                task4.Start();
                Thread.Sleep(1000);
                wait();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "NG pallet remove";
                }));
                Thread.Sleep(3000);
                task9 = new Task(() => movedown(rect58, 345, 355, 20));
                task9.Start();
                flag14 = 0;
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                flag13 = 0;
            }
            else
            {
                task9 = new Task(() => movedown(rect58, 345, 355, 20));
                task9.Start();
                flag14 = 0;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action2.Content = "send cylinder IO off";
                }));
                Thread.Sleep(1000);
                wait();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    action1.Content = "pallet go out";
                }));
                flag20 = 1;
                task8 = new Task(() => returnOK20(rect62));
                task8.Start();
                beltmoveflag[3] = 1;
                move2forward(rect, rect1, 646, 685, 20);
                GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] = false;
                flag13 = 0;
                move2forward(rect, rect1, 685, 719, 20);
                flag20 = 0;
                move2forward(rect, rect1, 719, 740, 20);
                beltmoveflag[3] = 0;
                wait();
            }
            flag15 = 0;

        }

        private int[] beltmoveflag = new int[4];
        private int beltdelta = 0;
        private void beltmove()
        {
            int flag = 1;
            while (true)
            {
                int run = 0;
                for (int i = 0; i < 4; i++)
                {
                    run += beltmoveflag[i];
                }
                if (run == 0)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    if (flag == 1)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            arrow1.Visibility = Visibility.Visible;
                            arrow2.Visibility = Visibility.Visible;
                            arrow3.Visibility = Visibility.Visible;
                            arrow4.Visibility = Visibility.Visible;
                            arrow5.Visibility = Visibility.Visible;
                            arrow6.Visibility = Visibility.Visible;
                            arrow7.Visibility = Visibility.Visible;
                            arrow8.Visibility = Visibility.Visible;
                            arrow11.Visibility = Visibility.Hidden;
                            arrow12.Visibility = Visibility.Hidden;
                            arrow13.Visibility = Visibility.Hidden;
                            arrow14.Visibility = Visibility.Hidden;
                            arrow15.Visibility = Visibility.Hidden;
                            arrow16.Visibility = Visibility.Hidden;
                            arrow17.Visibility = Visibility.Hidden;
                            arrow18.Visibility = Visibility.Hidden;
                        }));
                        flag = 0;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            arrow1.Visibility = Visibility.Hidden;
                            arrow2.Visibility = Visibility.Hidden;
                            arrow3.Visibility = Visibility.Hidden;
                            arrow4.Visibility = Visibility.Hidden;
                            arrow5.Visibility = Visibility.Hidden;
                            arrow6.Visibility = Visibility.Hidden;
                            arrow7.Visibility = Visibility.Hidden;
                            arrow8.Visibility = Visibility.Hidden;
                            arrow11.Visibility = Visibility.Visible;
                            arrow12.Visibility = Visibility.Visible;
                            arrow13.Visibility = Visibility.Visible;
                            arrow14.Visibility = Visibility.Visible;
                            arrow15.Visibility = Visibility.Visible;
                            arrow16.Visibility = Visibility.Visible;
                            arrow17.Visibility = Visibility.Visible;
                            arrow18.Visibility = Visibility.Visible;
                        }));
                        flag = 1;
                    }
                    Thread.Sleep(200);
                }
            }
        }

        public static void CopyProperties(object source, object destination)
        {
            var sourceProps = source.GetType().GetProperties();
            foreach (var prop in sourceProps)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(destination, prop.GetValue(source));
                }
            }
        }
        private void Station1Act()
        {
            while (true)
            {
                if (station1Init == 1)
                {
                    Thread.Sleep(500);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect10.Visibility = Visibility.Visible;
                        this.rect50.Visibility = Visibility.Visible;
                    }));
                    station1Finished = 0;
                    LailiaoAct(this.rect10, this.rect50);
                    while (station2Finished == 0)
                    {
                        station1Init = 0;
                        Thread.Sleep(10);
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CopyProperties(rect10, rect32);
                        CopyProperties(rect50, rect63);
                        this.rect10.Visibility = Visibility.Hidden;
                        this.rect50.Visibility = Visibility.Hidden;
                    }));
                    station1Finished = 1;
                    station2Init = 1;
                    Thread.Sleep(10);
                    //if (station2Finished != 0)
                    //{
                    //    this.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        //this.rect10.Visibility = Visibility.Hidden;
                    //        CopyProperties(rect10, rect32);
                    //    }));
                    //    station2Init = 1;
                    //    Thread.Sleep(10);
                    //}
                    //else
                    //{
                    //    station2Init = 0;
                    //    station1Init = 0;
                    //}
                }
            }
        }

        private void Station2Act()
        {
            while (true)
            {
                if (station2Init == 1)
                {
                    station1Init = 1;
                    station2Init = 0;
                    station2Finished = 0;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect32.Visibility = Visibility.Visible;
                        this.rect63.Visibility = Visibility.Visible;
                    }));
                    ZuzhuangAct(this.rect32, this.rect63);
                    while (station3Finished == 0)
                    {
                        station2Init = 0;
                        Thread.Sleep(10);
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CopyProperties(rect32, rect33);
                        CopyProperties(rect63, rect64);
                        this.rect32.Visibility = Visibility.Hidden;
                        this.rect63.Visibility = Visibility.Hidden;
                    }));
                    station2Finished = 1;
                    station3Init = 1;
                    Thread.Sleep(10);
                    //if (station3Finished != 0)
                    //{
                    //    this.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        //this.rect32.Visibility = Visibility.Hidden;
                    //        CopyProperties(rect32, rect33);
                    //    }));
                    //    station3Init = 1;
                    //    Thread.Sleep(10);
                    //}
                    //else
                    //{
                    //    station3Init = 0;
                    //    station2Init = 0;
                    //}
                }
            }
        }

        private void Station3Act()
        {
            while (true)
            {
                if (station3Init == 1)
                {
                    //station2Init = 1;
                    //this.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    this.rect33.Visibility = Visibility.Visible;
                    //}));
                    station3Init = 0;
                    station3Finished = 0;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect33.Visibility = Visibility.Visible;
                        this.rect64.Visibility = Visibility.Visible;
                    }));
                    FujianAct(this.rect33, this.rect64);
                    while (station4Finished == 0)
                    {
                        station3Init = 0;
                        Thread.Sleep(10);
                    }
                    station3Finished = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CopyProperties(rect33, rect34);
                        CopyProperties(rect64, rect65);
                        this.rect33.Visibility = Visibility.Hidden;
                        this.rect64.Visibility = Visibility.Hidden;
                    }));
                    station4Init = 1;
                    Thread.Sleep(10);
                    //if (station4Finished != 0)
                    //{
                    //    this.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        if (station2Finished == 0)
                    //        {
                    //            CopyProperties(rect33, rect34);
                    //            this.rect33.Visibility = Visibility.Hidden;
                    //        }
                    //        else {
                    //            this.rect33.Visibility = Visibility.Visible;
                    //            CopyProperties(rect33, rect34);
                    //        }
                    //    }));
                    //    station4Init = 1;
                    //    Thread.Sleep(10);
                    //}
                    //else
                    //{
                    //    station4Init = 0;
                    //    station3Init = 0;
                    //}
                }
            }
        }

        private void Station4Act()
        {
            while (true)
            {
                if (station4Init == 1)
                {
                    //station3Init = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect34.Visibility = Visibility.Visible;
                        this.rect65.Visibility = Visibility.Visible;
                    }));
                    station4Init = 0;
                    station4Finished = 0;
                    RejectAct(this.rect34, this.rect65);
                    station4Finished = 1;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.rect34.Visibility = Visibility.Hidden;
                        this.rect65.Visibility = Visibility.Hidden;
                    }));
                }
            }
        }

        private void wholeprocess()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.rect10.Visibility = Visibility.Visible;
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action1.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action2.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action4.Content = " ";
            }));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                action5.Content = " ";
            }));
            while (true)
            {
                //LailiaoAct(this.rect10);
                //ZuzhuangAct(this.rect10);
                //FujianAct(this.rect10);
                //RejectAct(this.rect10);

            }
        }

        private void start1_Click(object sender, RoutedEventArgs e)
        {
            //while (!GlobalManager.Current.isRun) {
            //    Thread.Sleep(100);
            //}
            flag = 0;
            flag1 = 0;
            flag2 = 0;
            flag3 = 0;
            flag4 = 0;
            NG_Foam_Count = 0;
            station1Init = 1;
            current_index = 0;
            beltmoveflag[0] = 0;
            beltmoveflag[1] = 0;
            beltmoveflag[2] = 0;
            beltmoveflag[3] = 0;
            rect21.Fill = new SolidColorBrush(Colors.Green);
            rect24.Fill = new SolidColorBrush(Colors.Green);
            rect27.Fill = new SolidColorBrush(Colors.Green);
            rect30.Fill = new SolidColorBrush(Colors.Green);

            rect52.Fill = new SolidColorBrush(Colors.Green);
            rect54.Fill = new SolidColorBrush(Colors.Green);
            rect56.Fill = new SolidColorBrush(Colors.Green);
            rect58.Fill = new SolidColorBrush(Colors.Green);

            Task task1 = new Task(Station1Act);
            task1.Start();
            Task task2 = new Task(Station2Act);
            task2.Start();
            Task task3 = new Task(Station3Act);
            task3.Start();
            Task task4 = new Task(Station4Act);
            task4.Start();
            Task task5 = new Task(beltmove);
            task5.Start();

            //Task task1 = new Task(wholeprocess);
            //task1.Start();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            deltatime = 999999;
        }

        private void resume_Click(object sender, RoutedEventArgs e)
        {
            deltatime = 0;
        }

        private int flag16 = 1;
        private void returnOK16(Rectangle rect)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = new SolidColorBrush(Colors.Green);
            }));
            while (flag16 == 1)
            {
                Thread.Sleep(100);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                rect.Fill = null;
            }));
        }
    }
}
