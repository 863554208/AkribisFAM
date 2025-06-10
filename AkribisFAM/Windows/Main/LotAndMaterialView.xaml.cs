using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for LotAndMaterialView.xaml
    /// </summary>
    public partial class LotAndMaterialView : UserControl
    {
        private Recipe _selectedRecipe = new Recipe();
        public LotAndMaterialView()
        {
            InitializeComponent();
            cbxRecipe.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxRecipe.SelectedIndex = 0;
        }

        private void cbxRecipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //UpdateButtons();
        }
        private void UpdateButtons()
        {
            if (App.lotManager != null)
            {

                btnStartLot.IsEnabled = App.lotManager.IsCurrLotNull ? true : false;
                btnEndLot.IsEnabled = App.lotManager.IsCurrLotNull ? false : true;
                txtCreator.Content = App.lotManager.IsCurrLotNull ? "" : App.lotManager.CurrLot.CreatedBy;
                txtStartDate.Content = App.lotManager.IsCurrLotNull ? "" : App.lotManager.CurrLot.StartDateTime.ToString();
                txtLotID.Text = App.lotManager.IsCurrLotNull ? "" : App.lotManager.CurrLot.LotNumber;
                txtLotID.IsEnabled = App.lotManager.IsCurrLotNull;
                cbxRecipe.IsEnabled = App.lotManager.IsCurrLotNull;
            }
        }
        private void btnStartLot_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtLotID.Text == "")
            {
                MessageBox.Show("Invalid Lot ID");
                return;
            }

            _selectedRecipe = App.recipeManager.Recipes[cbxRecipe.SelectedIndex];


            App.lotManager.StartLot(_selectedRecipe, "User", txtLotID.Text);
            App.DbManager.AddLotRecord(new LotRecord()
            {
                LotID = txtLotID.Text,
                Creator = "User",
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now,
                LotState = (int)App.lotManager.CurrLot.currLotstate,
                RecipeName = _selectedRecipe.RecipeName
            });
            App.DbManager.AddOeeRecord(new Models.OeeRecord()
            {
                LotID = txtLotID.Text,
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now,
            });
            UpdateButtons();
        }

        private void btnEndLot_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            var lot = App.lotManager.CurrLot;
            var lotrecord = new LotRecord()
            {
                LotID = lot.LotNumber,
                Creator = lot.CreatedBy,
                StartDateTime = lot.StartDateTime,
                EndDateTime = lot.EndDateTime,
                RecipeName = lot.Recipe.RecipeName,
            };
            App.DbManager.UpdateLotRecord(lotrecord);
            App.lotManager.EndLot();
            UpdateButtons();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) return;

            UpdateButtons();
        }
    }
}
