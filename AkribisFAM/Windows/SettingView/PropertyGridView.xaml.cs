using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using AkribisFAM.Models;


namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for PropertyGrid.xaml
    /// </summary>
    public partial class PropertyGridView : UserControl, INotifyPropertyChanged
    {
        AKBLocalParam lprm { get; set; }
        //RecipeSetting rPrm { get; set; }

        string changesString { get; set; }


        public PropertyGridView()
        {
            InitializeComponent();
        }

        //private void Log(MessageType type, string msg, int ecode = 0)
        //{
        //    Task.Run(() => App.msgHandler.StoreMessage(type, msg, ecode));
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public void UpdateAllPropertyItem()
        {
            bool[] hasChanged = new bool[propGrid.Properties.Count];
            object valueCache;
            object valueLive;
            changesString = "settings change(s) : ";

            for (int i = 0; i < propGrid.Properties.Count; i++)
            {
                //if (DataContext.GetType() == typeof(AKBLocalParam))
                //{
                    valueLive = typeof(AKBLocalParam.ParameterSet).GetProperty(((PropertyItem)propGrid.Properties[i]).PropertyName).GetValue(lprm.LiveParam);
                    valueCache = typeof(AKBLocalParam.ParameterSet).GetProperty(((PropertyItem)propGrid.Properties[i]).PropertyName).GetValue(lprm.CacheParam);
                    btnBackToOri.IsEnabled = ((AKBLocalParam)DataContext).IsChanged;
                    btnSave.IsEnabled = ((AKBLocalParam)DataContext).IsChanged;
                //}
                //else //(DataContext.GetType() == typeof(RecipeSetting))
                //{
                //    valueLive = typeof(RecipeSetting.ParameterSet).GetProperty(((PropertyItem)propGrid.Properties[i]).PropertyName).GetValue(rPrm.LiveParam);
                //    valueCache = typeof(RecipeSetting.ParameterSet).GetProperty(((PropertyItem)propGrid.Properties[i]).PropertyName).GetValue(rPrm.CacheParam);
                //    btnBackToOri.IsEnabled = ((RecipeSetting)DataContext).IsChanged;
                //    btnSave.IsEnabled = ((RecipeSetting)DataContext).IsChanged;

                //}

                if (valueCache.ToString() == valueLive.ToString())
                {
                    ((PropertyItem)propGrid.Properties[i]).FontWeight = FontWeights.Normal;
                    ((PropertyItem)propGrid.Properties[i]).Background = Brushes.White;
                }
                else
                {
                    ((PropertyItem)propGrid.Properties[i]).FontWeight = FontWeights.Bold;
                    ((PropertyItem)propGrid.Properties[i]).Background = Brushes.Orange;
                    hasChanged[i] = true;
                    changesString += $"[ {((PropertyItem)propGrid.Properties[i]).PropertyName} : {valueLive} → {valueCache} ] , ";
                }
            }
            //propGrid.Update();
            //btnBackToOri.IsEnabled = hasChanged.Any(x => x == false); 
            //btnSave.IsEnabled = hasChanged.Any(x => x == false); 

        }


        private void PropertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (e.NewValue == null) return;

            //if (DataContext.GetType() == typeof(AKBLocalParam))
            //{
            //lprm.CacheParam = lprm.CacheParam.Clone();  //For button biding refresh
            //}
            //else if (DataContext.GetType() == typeof(RecipeSetting))
            //{
            //    rPrm.CacheParam = rPrm.CacheParam;  //For button biding refresh
            //}

            //propGrid.Focus();
            UpdateAllPropertyItem();

        }

        private bool PreSaveCheck()
        {
            bool isLocalParam = DataContext.GetType() == typeof(AKBLocalParam);
            //bool isRecipeSetting = DataContext.GetType() == typeof(RecipeSetting);
            //if (isLocalParam)
            //{
            //    if (!App.lotMngr.IsCurrLotNull && App.lotMngr.CurrLot.currLotstate == Model.Lot.Lotstate.Running_Lot && lprm.IsPropertyChanged("RunMode"))
            //    {
            //        if (AKBMessageBox.ShowDialog("You cannot change the machine's Run Mode while a lot is running. Please end the current lot before attempting to change the Run Mode.", msgIcon: AKBMessageBox.MessageBoxIcon.Warning))
            //            return false;
            //    }

            //}
            //if (isRecipeSetting)
            //{

            //}
            return true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!PreSaveCheck())
                return;


            //if (AKBMessageBox.ShowDialog("Are you sure you want to save the setting?", msgBtn: MessageBoxButton.YesNo, msgIcon: AKBMessageBox.MessageBoxIcon.Question))
            //{
            //    if (DataContext.GetType() == typeof(AKBLocalParam))
            //    {
            //        lprm.Save();

            //        Log(MessageType.Info, $"'{App.userManager.CurrentUser.DisplayName}' saved {changesString}");
            //    }
            //    else if (DataContext.GetType() == typeof(RecipeSetting))
            //    {
            //        rPrm.Save();
            //        RecipeSetting setting = (RecipeSetting)DataContext;
            //        Log(MessageType.Info, $"'{App.userManager.CurrentUser.DisplayName}' saved recipe '{setting.RecipeName}' {changesString}");
            //    }
            //    UpdateAllPropertyItem();
            //    propGrid.Update();

            //}
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (DataContext.GetType() == typeof(AKBLocalParam))
            {
                openFileDialog.InitialDirectory = lprm.SettingsBackupFolderPath;
                openFileDialog.Filter = $"json files (*{lprm.SettingsFilename})|*{lprm.SettingsFilename}";
            }
            //else if (DataContext.GetType() == typeof(RecipeSetting))
            //{
            //    openFileDialog.InitialDirectory = rPrm.SettingsBackupFolderPath;
            //    openFileDialog.Filter = $"json files (*{rPrm.SettingsFilename})|*{rPrm.SettingsFilename}";
            //}
            //openFileDialog.Filter = "json files (*.json)|*.json";
            openFileDialog.RestoreDirectory = true;
            bool? res = openFileDialog.ShowDialog();

            if (res == true)
            {
                if (DataContext.GetType() == typeof(AKBLocalParam))
                {
                    lprm.Load(openFileDialog.FileName);
                }
                //else if (DataContext.GetType() == typeof(RecipeSetting))
                //{
                //    rPrm.Load(openFileDialog.FileName);
                //}
                UpdateAllPropertyItem();
                var fn = Path.GetFileName(openFileDialog.FileName);
                //Log(MessageType.Info, $"'{App.userManager.CurrentUser.DisplayName}' loaded '{fn}' with {changesString}");
            }
        }

        private void ButtonUnchange_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext.GetType() == typeof(AKBLocalParam))
            {
                lprm.UndoChanged();
            }
            //else if (DataContext.GetType() == typeof(RecipeSetting))
            //{
            //    rPrm.UndoChanged();
            //}

            UpdateAllPropertyItem();

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //    lprm = AkbParam;
            //DataContext = AkbParam;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext.GetType() == typeof(AKBLocalParam))
            {
                lprm = (AKBLocalParam)DataContext;
            }
            //else if (DataContext.GetType() == typeof(RecipeSetting))
            //{
            //    rPrm = (RecipeSetting)DataContext;
            //}
            UpdateAllPropertyItem();
        }

        private void propGrid_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void propGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void propGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void propGrid_PreviewKeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    

    }

}
