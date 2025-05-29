using AkribisFAM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Manager
{
    public class RecipeManager
    {
        public List<Recipe> Recipes = new List<Recipe>();
        public RecipeManager()
        {
            Default();
        }
        public void Default()
        {
            Recipes.Add(new Recipe()
            {
                RecipeNumber = (int)TrayType.PAM_230_144_3X4,
                TrayType = TrayType.PAM_230_144_3X4,
                RecipeName = TrayType.PAM_230_144_3X4.ToString(),
                SettingPath = "Station_points1.json",
                XPitch = 54.5,
                YPitch = 40,
                Width = 230,
                Height = 144,
                PartRow = 3,
                PartColumn = 4,
            });

            Recipes.Add(new Recipe()
            {
                RecipeNumber = (int)TrayType.RIM_292_120_2X8,
                TrayType = TrayType.RIM_292_120_2X8,
                RecipeName = TrayType.RIM_292_120_2X8.ToString(),
                SettingPath = "Station_points2.json",
                XPitch = 69,
                YPitch = 56,
                Width = 290,
                Height = 120,
                PartRow = 2,
                PartColumn = 8,
            });

            Recipes.Add(new Recipe()
            {
                RecipeNumber = (int)TrayType.SLAW_360_260_4X5,
                TrayType = TrayType.SLAW_360_260_4X5,
                RecipeName = TrayType.SLAW_360_260_4X5.ToString(),
                SettingPath = "Station_points3.json",
                XPitch = 40,
                YPitch = 60,
                Width = 360,
                Height = 260,
                PartRow = 4,
                PartColumn = 5,
            });

            Recipes.Add(new Recipe()
            {
                RecipeNumber = (int)TrayType.VAM_300_220_3X6,
                TrayType = TrayType.VAM_300_220_3X6,
                RecipeName = TrayType.VAM_300_220_3X6.ToString(),
                SettingPath = "Station_points4.json",
                XPitch = 40,
                YPitch = 60,
                Width = 300,
                Height = 220,
                PartRow = 3,
                PartColumn = 6,
            });

            Recipes.Add(new Recipe()
            {
                RecipeNumber = (int)TrayType.RUM_300_200_4x3,
                TrayType = TrayType.RUM_300_200_4x3,
                RecipeName = TrayType.RUM_300_200_4x3.ToString(),
                SettingPath = "Station_points5.json",
                XPitch = 85,
                YPitch = 42,
                Width = 300,
                Height = 200,
                PartRow = 4,
                PartColumn = 3,
            });
        }

        public StationPoints Get_RecipeStationPoints(TrayType type)
        {
            StationPoints points = null; 
            var recipe = Recipes.FirstOrDefault(x => x.TrayType == type);
            if (recipe != null)
            {
                string jsonFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, recipe.SettingPath);
                FileHelper.LoadConfig(jsonFile, out points);
            }
            return points;
        }
        public Recipe GetRecipe(TrayType type)
        {
            Recipe points = null; 
            var recipe = Recipes.FirstOrDefault(x => x.TrayType == type);
            if (recipe != null)
            {
                return recipe;
            }
            return points;
        }
        public void Load_Recipe(TrayType type)
        {
            var recipe = Recipes.First(x => x.TrayType == type);
            if (recipe != null)
            {
                string jsonFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, recipe.SettingPath);
                FileHelper.LoadConfig(jsonFile, out GlobalManager.Current.stationPoints);
            }
        }
    }
    public enum TrayType
    {
        PAM_230_144_3X4,
        RIM_292_120_2X8,
        SLAW_360_260_4X5,
        VAM_300_220_3X6,
        RUM_300_200_4x3,
    }
    public class Recipe
    {
        private int recipeNumber;
        public int RecipeNumber
        {
            get { return recipeNumber; }
            set { recipeNumber = value; }
        }
        private TrayType trayType;
        public TrayType TrayType
        {
            get { return trayType; }
            set { trayType = value; }
        }

        private string recipeName;
        public string RecipeName
        {
            get { return recipeName; }
            set { recipeName = value; }
        }
        private string settingPath;
        public string SettingPath
        {
            get { return settingPath; }
            set { settingPath = value; }
        }

        private int partRow;

        public int PartRow
        {
            get { return partRow; }
            set { partRow = value; }
        }
        private int partColumn;

        public int PartColumn
        {
            get { return partColumn; }
            set { partColumn = value; }
        }
        private double xpitch;

        public double XPitch
        {
            get { return xpitch; }
            set { xpitch = value; }
        }

        private double width;

        public double Width
        {
            get { return width; }
            set { width = value; }
        }

        private double height;

        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        private double ypitch;

        public double YPitch
        {
            get { return ypitch; }
            set { ypitch = value; }
        }

        public int TotalPart => PartRow * PartColumn;

    }
}
