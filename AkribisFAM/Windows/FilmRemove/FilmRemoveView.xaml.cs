using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using AkribisFAM.Manager;
using AkribisFAM;
using static AkribisFAM.Windows.FoamAssemblyView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FilmRemoveView.xaml
    /// </summary>
    public partial class FilmRemoveView : UserControl
    {
        FilmRemoveVM vm;
        bool stopAllMotion = false;
        class FilmRemoveVM
        {

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
            var laser = stationsPoints.LaiLiaoPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Laser Points"));
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
                Points = points,
                Row = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartRow,
                Column = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartColumn,
            };
            DataContext = vm;
        }

        private void btnFilmRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //stopAllMotion = true;
            //Task.Run(() =>
            //{
            //    AkribisFAM.Current.StopAllAxis();
            //});
        }

        private void btnZUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnZDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnClawOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnClawClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnMoveToRejectPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnReject_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
