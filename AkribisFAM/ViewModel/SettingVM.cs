using AkribisFAM.Models;

namespace AkribisFAM.ViewModel
{
    public class SettingVM : ViewModelBase
    {
        public SettingVM()
        {
            Lprm = App.paramLocal;
            //RecipeMngr = App.recipeMngr;
            //Machine = App.machine;

            IntializeUpdateThread("SettingVM", 200);
        }

        public override void ThreadBody()
        {
            OnPropertyChanged(nameof(Lprm));
            //OnPropertyChanged(nameof(Machine));
        }
       
        private AKBLocalParam lprm;
        public AKBLocalParam Lprm
        {
            get { return lprm; }
            set
            { lprm = value; OnPropertyChanged(nameof(Lprm)); }
        }
        //private RecipeManager recipeMngr;
        //public RecipeManager RecipeMngr
        //{
        //    get { return recipeMngr; }
        //    set { recipeMngr = value; OnPropertyChanged(nameof(RecipeMngr)); }
        //}
        //private Machine _machine;
        //public Machine Machine
        //{
        //    get { return _machine; }
        //    set { _machine = value; OnPropertyChanged(); }
        //}
        //private RecipeSetting recipePrm;
        //public RecipeSetting RecipePrm
        //{
        //    get { return recipePrm; }
        //    set { recipePrm = value; OnPropertyChanged(nameof(RecipePrm)); }
        //}


    }


}
