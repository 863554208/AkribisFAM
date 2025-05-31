using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using AkribisFAM.Manager;
using System.Threading;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using AkribisFAM.WorkStation;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FilmRemoveView.xaml
    /// </summary>
    public partial class FilmRemoveView : UserControl
    {
        FilmRemoveVM vm;
        bool stopAllMotion = false;
        class FilmRemoveVM : ViewModelBase
        {
            private int totalProcess;

            public int TotalProcess
            {
                get { return totalProcess; }
                set { totalProcess = value; OnPropertyChanged(); }
            }
            private int progress;

            public int Progress
            {
                get { return progress; }
                set { progress = value; OnPropertyChanged(); }
            }

            private ObservableCollection<SinglePointExt> points = new ObservableCollection<SinglePointExt>();

            public ObservableCollection<SinglePointExt> Points
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
        public FilmRemoveView()
        {
            InitializeComponent();

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }

        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = null;
            List<SinglePointExt> lsp = new List<SinglePointExt>();
            if (cbxTrayType.SelectedIndex < 0) return;
            if (cbxTrayType.SelectedIndex > 4) return;

            var stationsPoints = App.recipeManager.Get_RecipeStationPoints((TrayType)cbxTrayType.SelectedIndex);
            if (stationsPoints == null) return;

            //var laser = stationsPoints.FuJianPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Tearing Points"));
            var laser = stationsPoints.FuJianPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Tearing Points"));
            if (laser == null) return;

            lsp = laser.childList.Select((x, index) => new SinglePointExt
            {
                X = x.childPos[0],
                Y = x.childPos[1],
                Z = x.childPos[2],
                R = x.childPos[3],
                TeachPointIndex = index + 1
            }).ToList();

            var points = new ObservableCollection<SinglePointExt>(lsp);

            vm = new FilmRemoveVM()
            {
                TotalProcess = 0,
                Points = points,
                Row = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartRow,
                Column = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartColumn,
            };
            DataContext = vm;
        }

        private async void btnFilmRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            stopAllMotion = false;
            vm.TotalProcess = 3 * vm.Row * vm.Column;
            vm.Progress = 0;
            grpControl.IsEnabled = false;
            pbProgress.Visibility = System.Windows.Visibility.Visible;
            await Task.Run(() =>
              {

                  foreach (var point in vm.Points)
                  {
                      if (stopAllMotion) return;

                      if (!App.filmRemoveGantryControl.RemoveFilm(point.X, point.Y))
                      {
                          return;
                      }
                      vm.Progress++;
                      if (!App.filmRemoveGantryControl.Toss())
                      {
                          return;
                      }
                      vm.Progress++;

                      Thread.Sleep(100);

                  }
                  foreach (var point in vm.Points)
                  {
                      if (stopAllMotion) return;

                      if (!App.filmRemoveGantryControl.MoveToVisionPos(point.X, point.Y))
                      {
                          return;
                      }
                      if (!App.vision1.Trigger())
                      {
                          return;
                      }
                      //if (!App.vision1.CheckFilm(point.TeachPointIndex, vm.Row, vm.Column))
                      //{
                      //    return;
                      //}
                      vm.Progress++;

                      Thread.Sleep(100);
                  }

              });

            vm.Progress = 0;
            grpControl.IsEnabled = true;
            pbProgress.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            stopAllMotion = true;
            Task.Run(() =>
            {
                AkrAction.Current.StopAllAxis();
            });
        }

        private void btnZUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (!App.filmRemoveGantryControl.ZUp())
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        //AKBMessageBox.ShowDialog("Failed to Z up", "Motion failed",
                        //msgIcon: AKBMessageBox.MessageBoxIcon.Completed);
                    });
                }
            });

        }

        private void btnZDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!App.filmRemoveGantryControl.ZDown())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //AKBMessageBox.ShowDialog("Failed to Z up", "Motion failed",
                    //msgIcon: AKBMessageBox.MessageBoxIcon.Completed);
                });
            }
        }

        private void btnClawOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (!App.filmRemoveGantryControl.ClawOpen())
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        //AKBMessageBox.ShowDialog("Failed to Z up", "Motion failed",
                        //msgIcon: AKBMessageBox.MessageBoxIcon.Completed);
                    });
                }
                else
                {
                    Thread.Sleep(1);
                }
            });
        }

        private void btnClawClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (!App.filmRemoveGantryControl.ClawClose())
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        //AKBMessageBox.ShowDialog("Failed to Z up", "Motion failed",
                        //msgIcon: AKBMessageBox.MessageBoxIcon.Completed);
                    });
                }
                else
                {

                    Thread.Sleep(1);
                }
            });
        }

        private void btnMoveToRejectPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!App.filmRemoveGantryControl.MoveToBinPos())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //AKBMessageBox.ShowDialog("Failed to Z up", "Motion failed",
                    //msgIcon: AKBMessageBox.MessageBoxIcon.Completed);
                });
            }
            ;
        }


        private async void PointXYMoveRemoveInspectView_VisionInspectPressed(object sender, EventArgs e)
        {
            PointXYMoveRemoveInspectView view = (PointXYMoveRemoveInspectView)sender;
            SinglePointExt points = (SinglePointExt)view.DataContext;
            await Task.Run(() =>
            {

                if (!App.filmRemoveGantryControl.MoveToVisionPos(points.X, points.Y))
                {
                    return;
                }

                if (!App.vision1.CheckFilm(points.TeachPointIndex, vm.Row, vm.Column))
                {
                    return;
                }

                if (!App.vision1.Trigger())
                {
                    return;
                }
            });
        }


        private async void PointXYMoveRemoveInspectView_MovePressed(object sender, EventArgs e)
        {
            PointXYMoveRemoveInspectView view = (PointXYMoveRemoveInspectView)sender;
            SinglePointExt points = (SinglePointExt)view.DataContext;
            await Task.Run(() =>
            {

                if (!App.filmRemoveGantryControl.MovePos(points.X, points.Y))
                {
                    return;
                }

            });

        }

        private void PointXYMoveRemoveInspectView_RemoveFilmPressed(object sender, EventArgs e)
        {
            PointXYMoveRemoveInspectView view = (PointXYMoveRemoveInspectView)sender;
            SinglePointExt points = (SinglePointExt)view.DataContext;
            Task.Run(() =>
            {
                if (!App.filmRemoveGantryControl.RemoveFilm(points.X, points.Y))
                {
                    return;
                }
            });
        }

        private void btnToss_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!App.filmRemoveGantryControl.Toss())
            {

            }
        }
    }
}
