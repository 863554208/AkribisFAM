using System.Linq;
using System.Windows.Controls;
using AkribisFAM.ViewModel;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public static SettingVM settingVM = new SettingVM();
        
        public SettingsView()
        {
            InitializeComponent();
            DataContext = settingVM;
            App.Current.Exit += Current_Exit;
        }

        private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            settingVM.PauseUpdateThread();
            settingVM.TerminateUpdateThread();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender!=null)
            {
                ComboBox cbxItem = ((ComboBox)sender);
                //string recipeName = ((Recipe)cbxItem.SelectedItem).RecipeName;
                //Recipe recipe = settingVM.RecipeMngr.Recipes.First(x => x.RecipeName == recipeName);
                //settingVM.RecipePrm = recipe.RecipeSetting;
                //pgridRecipe.DataContext = settingVM.RecipePrm;
                //pgridRecipe.UpdateAllPropertyItem();
            }
            //settingVM.RecipePrm = (((ComboBox)sender).SelectedItem)
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            settingVM.ResumeUpdateThread();
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            settingVM.PauseUpdateThread();
        }
    }
}
