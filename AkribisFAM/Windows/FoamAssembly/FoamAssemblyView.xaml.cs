using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AkribisFAM.DeviceClass;
using AkribisFAM.Manager;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static AkribisFAM.Windows.FoamAssemblyView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FoamAssemblyView.xaml
    /// </summary>
    public partial class FoamAssemblyView : UserControl
    {

        FoamAssemblyVM vm;
        bool stopAllMotion = false;
        class FoamAssemblyVM
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
        public FoamAssemblyView()
        {
            InitializeComponent();

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }

        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = null;
            itemControl.ItemsSource = null;
            List<SinglePointExt> lsp = new List<SinglePointExt>();
            if (cbxTrayType.SelectedIndex < 0) return;
            if (cbxTrayType.SelectedIndex > 4) return;

            var stationsPoints = App.recipeManager.Get_RecipeStationPoints((TrayType)cbxTrayType.SelectedIndex);
            if (stationsPoints == null) return;

            var laser = stationsPoints.ZuZhuangPointList.FirstOrDefault(x => x.name != null && x.name.Equals("PlaceFoam Points")); 
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

            vm = new FoamAssemblyVM()
            {
                Points = points,
                Row = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartRow,
                Column = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartColumn,
            };
            DataContext = vm;
            itemControl.ItemsSource = vm.Points;
        }
        private void PointXYPickerMoveAndPlaceView_PickerMovePressed(object sender, EventArgs e)
        {
            Button button = sender as Button;
            var dc = (PointXYPickerMoveAndPlaceView)sender;

            SinglePointExt point = (SinglePointExt)dc.DataContext;
            App.assemblyGantryControl.MovePlacePos((AssemblyGantryControl.Picker)dc.SelectedPicker, point.TeachPointIndex);
        }

        private void PointXYPickerMoveAndPlaceView_PickerPlacePressed(object sender, EventArgs e)
        {

            Button button = sender as Button;
            var dc = (PointXYPickerMoveAndPlaceView)sender;

            SinglePointExt point = (SinglePointExt)dc.DataContext;
            App.assemblyGantryControl.PlaceFoam((AssemblyGantryControl.Picker)dc.SelectedPicker, point.TeachPointIndex);
        }

        private void btnVis3OTF_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var recipe = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex);
            if (!App.vision1.Vision1OnTheFlyPalletTrigger(recipe.PartRow, recipe.PartColumn))
            {
                MessageBox.Show("Fail to perform on the fly for pallet");
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveStandby_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveEnding_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbxTrayType_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
    public class SinglePointExt : SinglePoint
    {
        private int teachPointIndex;

        public int TeachPointIndex
        {
            get { return teachPointIndex; }
            set { teachPointIndex = value; }
        }

        public SinglePointExt() { }
    }

}
