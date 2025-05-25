using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for LaserHeighCheckView.xaml
    /// </summary>
    public partial class LaserHeighCheckView : UserControl
    {
        LaserHeighCheckVM vm;
        bool stopAllMotion = false;
         class LaserHeighCheckVM
        {

            private List<ObservableCollection<SinglePoint>> points = new List<ObservableCollection<SinglePoint>>();

            public List<ObservableCollection<SinglePoint>> Points
            {
                get { return points; }
                set { points = value; }
            }
            private int col;

            public int Column
            {
                get { return col; }
                set { col = value; }
            }
            private int row;

            public int Row
            {
                get { return row; }
                set { row = value; }
            }
        }
        public LaserHeighCheckView()
        {
            InitializeComponent();

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;

        }

        private void cbxTrayType_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = null;
            List<SinglePoint> lsp = new List<SinglePoint>();
            if (cbxTrayType.SelectedIndex < 0) return;
            if (cbxTrayType.SelectedIndex > 0) return;

            var stationsPoints = App.recipeManager.Get_RecipeStationPoints((TrayType)cbxTrayType.SelectedIndex);
            if (stationsPoints == null) return;

            var laser = stationsPoints.LaiLiaoPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Laser Points"));
            if (laser == null) return;

            lsp = laser.childList.Select(x => new SinglePoint
            {
                X = x.childPos[0],
                Y = x.childPos[1],
                Z = x.childPos[2],
                R = x.childPos[3],
            }).ToList();


            var points = new ObservableCollection<SinglePoint>(lsp);
            List<ObservableCollection<SinglePoint>> pts = new List<ObservableCollection<SinglePoint>>();
            var newpt = new ObservableCollection<SinglePoint>();
            newpt.Add(points[0]);
            newpt.Add(points[1]);
            newpt.Add(points[2]);
            newpt.Add(points[3]);

            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);
            pts.Add(newpt);

             vm = new LaserHeighCheckVM()
            {
                Points = pts,
                Row = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartRow,
                Column = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartColumn,
            };
            DataContext = vm;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            //Points = new ObservableCollection<SinglePoint>(GlobalManager.Current.laserPoints);
            //DataContext = Points;
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) return;

            //Points = new ObservableCollection<SinglePoint>(GlobalManager.Current.laserPoints);
            //DataContext = Points;
        }

        private void btnCheckAllTeachPoint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            stopAllMotion = false;
            if (vm != null)
            {
                Task.Run(() =>
                {
                    foreach (var pts in vm.Points)
                    {
                        foreach (var pt in pts)
                        {
                            if (!stopAllMotion)
                            {
                                AkrAction.Current.MoveNoWait(AxisName.LSX, (int)pt.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX);
                                AkrAction.Current.Move(AxisName.LSY, (int)pt.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY);
                                App.laser.Measure(out int readout);
                                Thread.Sleep(50);
                            }
                        }
                    }

                });
            }
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            stopAllMotion = true;
            Task.Run(() =>
            {
                AkrAction.Current.StopAllAxis();
            });
        }
    }
}
