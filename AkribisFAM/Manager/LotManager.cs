using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using AAMotion;

namespace AkribisFAM.Manager
{
    public class LotManager : ViewModelBase
    {

        private Lot _currLot;
        private Lot _cacheLot = new Lot();
        public Lot CurrLot
        {
            get { return _currLot; }
            set { _currLot = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCurrLotNull)); }
        }

        public Lot CacheLot
        {
            get { return _cacheLot; }
            set { _cacheLot = value; OnPropertyChanged(); }
        }
        public bool IsCurrLotNull => CurrLot == null;

        public bool Initialize()
        {
            var lotrecord = App.DbManager.GetCurrentLot();
            var recipe = App.recipeManager.Recipes.FirstOrDefault(x => x.RecipeName == lotrecord.RecipeName);
            if (recipe == null)
            {
                return false;
            }
            var lot = new Lot()
            {
                StartDateTime = lotrecord.StartDateTime,
                CreatedBy = lotrecord.Creator,
                LotNumber = lotrecord.LotID,
                currLotstate = (Lot.LotState)lotrecord.LotState,
                Recipe = recipe

            };
            CurrLot = lot;
            CacheLot = lot;
            CurrLot.Recipe = recipe;
            return true;
        }
        public bool StartLot(Recipe recipe, string user, string code)
        {
            var lot = new Lot()
            {
                StartDateTime = DateTime.Now,
                CreatedBy = user,
                LotNumber = code,
                currLotstate = Lot.LotState.Running_Lot,
                Recipe = recipe

            };
            CurrLot = lot;
            CacheLot = lot;

            return true;
        }

        public bool EndLot()
        {
            var lot = new Lot()
            {
                EndDateTime = DateTime.Now,
                currLotstate = Lot.LotState.Completed,

            };
            CurrLot = lot;
            CacheLot = lot;
            CurrLot = null;
            CacheLot = null;
            return true;
        }
    }

    public class Lot : ViewModelBase
    {
        #region Private Member

        #endregion Private Member

        #region Private Properties
        private int _lotId;
        private string _lotNumber;
        private string _createdBy;
        private DateTime _startDateTime;
        private DateTime _endDateTime;

        private Recipe _recipe;


        #endregion Private Properties

        #region Public Properties
        [Key]
        public int LotId
        {
            get { return _lotId; }
            set { _lotId = value; OnPropertyChanged(); }
        }
        public string LotNumber
        {
            get { return _lotNumber; }
            set { _lotNumber = value; OnPropertyChanged(); }
        }
        public string CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; OnPropertyChanged(); }
        }

        public DateTime StartDateTime
        {
            get { return _startDateTime; }
            set { _startDateTime = value; OnPropertyChanged(); }
        }

        public DateTime EndDateTime
        {
            get { return _endDateTime; }
            set { _endDateTime = value; OnPropertyChanged(); }
        }

        [EnumDataType(typeof(LotState))]
        public LotState currLotstate { get; set; }

        [ForeignKey("Recipe")]
        public int? RecipeId { get; set; }
        public Recipe Recipe
        {
            get
            {
                return _recipe;
            }
            set
            {
                _recipe = value;
                OnPropertyChanged(nameof(Recipe));
                OnPropertyChanged(nameof(HasSelectedRecipe));
            }
        }
        public bool HasAllRequiredPart => false;
        //public bool HasAllRequiredPart => Recipe.RecipeParts.All(x => LotMaterials.Any(y => y.PartId == x.PartId)) && (LotMaterials.Count() == Recipe.RecipeParts.Count());
        public bool HasSelectedRecipe => Recipe != null;
        public bool HasLotID => LotNumber != string.Empty;
        public bool IsWaitingLotID => string.IsNullOrEmpty(LotNumber) && currLotstate == LotState.Running_Lot;

        public enum LotState
        {
            Unknown = 0,
            Running_Lot = 1,
            WaitingForLotID = 2,
            PendingRecipe = 3,
            SelectRecipe = 4,
            SelectRecipeDone = 5,
            Purging = 6,
            PendingScan = 7,
            ScanComplete = 8,
            PendingTCR = 9,
            PendingLotStart = 10,
            AdminOverride = 11,
            RunEmpty = 12, //vk2022
            RunEmptyCompleted = 13, //vk2022
            Completed,
            Null
        }
        #endregion Public Properties

        #region Constructors
        public Lot() { }


        public Lot(Recipe recipe)
        {
            Recipe = recipe;
        }

        #endregion Constructors

        #region Event Handlers/Action Methods

        #endregion Event Handlers/Action Methods

        #region Private Methods

        #endregion Private Methods

        #region Public Methods
        //public ObservableCollection<Part> GetDBPart(ObservableCollection<LotMaterial> lotmaterials)
        //{
        //    ObservableCollection<Part> parts = new ObservableCollection<Part>();
        //    using (MyDBContext context = new MyDBContext())
        //    {
        //        foreach (var item in lotmaterials)
        //        {
        //            parts.Add(context.Parts.Find(item.PartId));
        //        }
        //    }
        //    return parts;
        //}



        public Lot Clone()
        {
            Lot cloneSet = new Lot();
            PropertyInfo[] properties = typeof(Lot).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && property != null)
                {
                    try
                    {

                        object value = property.GetValue(this);
                        property.SetValue(cloneSet, value);
                    }

                    catch (Exception)
                    {

                        //throw;
                    }
                }
            }

            return cloneSet;
        }

        #endregion Public Methods

    }

}
