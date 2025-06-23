using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AkribisFAM.Helper;
using System.Data.Entity.Validation;
using System.Data.Entity;

namespace AkribisFAM.Manager
{
    public class UserManager 
    {
        public string encrypt(string clearText)
        {
            var _encryptionKey = "abc123";
            byte[] _clearBytes = Encoding.Unicode.GetBytes(clearText);

            using (var _encryptor = Aes.Create())
            {
                var _pdb = new Rfc2898DeriveBytes(_encryptionKey
                                                , new byte[]{
                                                                0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76
                                                              , 0x65, 0x64, 0x65, 0x76
                                                            });

                _encryptor.Key = _pdb.GetBytes(32);
                _encryptor.IV = _pdb.GetBytes(16);

                using (var _ms = new MemoryStream())
                {
                    using (var _cs = new CryptoStream(_ms, _encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        _cs.Write(_clearBytes, 0, _clearBytes.Length);
                        _cs.Close();
                    }

                    clearText = Convert.ToBase64String(_ms.ToArray());
                }
            }

            return clearText;
        }

        public string decrypt(string cipherText)
        {
            try
            {
                var _encryptionKey = "abc123";
                cipherText = cipherText.Replace(" ", "+");
                byte[] _cipherBytes = Convert.FromBase64String(cipherText);

                using (var _encryptor = Aes.Create())
                {
                    var _pdb = new Rfc2898DeriveBytes(_encryptionKey
                                                    , new byte[]{
                                                                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76
                                                                  , 0x65, 0x64, 0x65, 0x76
                                                                });

                    _encryptor.Key = _pdb.GetBytes(32);
                    _encryptor.IV = _pdb.GetBytes(16);

                    using (var _ms = new MemoryStream())
                    {
                        using (var _cs = new CryptoStream(_ms, _encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            _cs.Write(_cipherBytes, 0, _cipherBytes.Length);
                            _cs.Close();
                        }

                        cipherText = Encoding.Unicode.GetString(_ms.ToArray());
                    }
                }

                return cipherText;
            }
            catch
            {
                return string.Empty;
            }
        }
        #region Private Member

        #endregion Private Member

        #region Private Properties


        #endregion Private Properties

        public event EventHandler UserLoggedIn;

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }
        public bool HasLoggedIn => CurrentUser != null;
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        protected virtual void OnUserLoggedIn(EventArgs e)
        {
            UserLoggedIn?.Invoke(this, e);
        }


        #region Public Properties
        //[NotMapped]
        //public string currentUser { get; set; } = DEFAULT_USER;


        #endregion Public Properties

        #region Constructors
        public UserManager()
        {
        }
        #endregion Constructors

        #region Event Handlers/Action Methods

        #endregion Event Handlers/Action Methods

        #region Private Methods
        //private void SideNavigationBtnEnabled(bool status)
        //{
        //    _mainVM.HomeBtnEnabled = status;
        //    _mainVM.StatisticsBtnEnabled = status;
        //    _mainVM.HardwareBtnEnabled = status;
        //    _mainVM.CalibrateBtnEnabled = status;
        //    _mainVM.AlarmsBtnEnabled = status;
        //    _mainVM.SystemBtnEnabled = status;
        //    _mainVM.SettingsBtnEnabled = status;
        //}
        #endregion Private Methods

        private string RecentLoginUserFilename = "LoginUser";
        // Define the private field
        private Queue<string> _historyUsers = new Queue<string>();

        // Public property with get and set accessors
        public Queue<string> HistoryUsers
        {
            get { return _historyUsers; }
            set { _historyUsers = value; }
        }

        #region Public Methods
        public bool Login(string username, string password, out string errMsg)
        {

            errMsg = "";
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                errMsg = "[ValidateLogin] Invalid username or password";
                return false;
            }

            // Encrypt password
            var EncryptedPass = encrypt(password);
            LoadFromDB();
            User foundUser = Users.FirstOrDefault(x => x.Username == username.ToLower() && x.Password == EncryptedPass);


            CurrentUser = foundUser;
            if (CurrentUser == null)
            {
                errMsg = "[ValidateLogin] Invalid username or password";
                return false;
            }
            OnUserLoggedIn(EventArgs.Empty);
            return (foundUser != null);


        }

        public ObservableCollection<User> GetUsersEqualOrBelowLevel()
        {
            if (CurrentUser == null) return null;

            using (var context = new MyDBContext())
            {
                return new ObservableCollection<User>(context.Users.Where(x => x.UserLevel.Level <= CurrentUser.UserLevel.Level).Include(u => u.UserLevel).Include(x => x.Creator).ToList());
            }

        }
        public bool CheckDuplicateUsername(string checkUserName)
        {
            bool IsUsernameExist = false;
            using (var context = new MyDBContext())
            {
                IsUsernameExist = context.Users.Any(x => x.Username == checkUserName);
            }
            return IsUsernameExist;
        }
        public bool Initialize()
        {
            Default();

            LoadHistoryFromText();

            if (LoadFromDB())
            {

            }
            else
            {


                if (SaveToDB())
                {
                }

            }
            return true;
        }
        public void Default()
        {
            try
            {
                UserRight AlarmAcknowledge = new UserRight() { Name = "AlarmAcknowledge" };
                UserRight VisionView = new UserRight() { Name = "VisionView" };
                UserRight VisionControl = new UserRight() { Name = "VisionControl" };
                UserRight IOView = new UserRight() { Name = "IOView" };
                UserRight IOControl = new UserRight() { Name = "IOControl" };
                UserRight ManualProcess = new UserRight() { Name = "ManualProcess" };
                UserRight DeviceControl = new UserRight() { Name = "DeviceControl" };
                UserRight LaserConfiguration = new UserRight() { Name = "LaserConfiguration" };
                UserRight VisionSensorConfiguration = new UserRight() { Name = "VisionSensorConfiguration" };
                UserRight LotControl = new UserRight() { Name = "LotControl" };
                UserRight Calibration = new UserRight() { Name = "Calibration" };
                UserRight CalibrationSet = new UserRight() { Name = "CalibrationSet" };
                UserRight Setting = new UserRight() { Name = "Setting" };

                //UserRight RecipeAdding = new UserRight() { Name = "RecipeAdd" };
                UserRight RecipeSetting = new UserRight() { Name = "RecipeSetting" };
                //UserRight RecipeRemove = new UserRight() { Name = "RecipeRemove" };

                UserRight UserAdd = new UserRight() { Name = "UserAdd" };
                UserRight UserEdit = new UserRight() { Name = "UserEdit" };
                UserRight UserRemove = new UserRight() { Name = "UserRemove" };

                //UserRight RightAdd = new UserRight() { Name = "RightAdd" };
                UserRight RightEdit = new UserRight() { Name = "RightEdit" };
                //UserRight RightRemove = new UserRight() { Name = "RightRemove" };

                UserRight Debug = new UserRight() { Name = "Debug" };

                UserRight Overview = new UserRight() { Name = "Overview" };

                UserRight LogView = new UserRight() { Name = "LogView" };
                UserRight StatisticsView = new UserRight() { Name = "StatisticsView" };
                UserRight SystemInfoView = new UserRight() { Name = "SystemInfoView" };


                UserLevel Operator = new UserLevel() { Name = "Operator", Level = 0 };
                UserLevel Engineer = new UserLevel() { Name = "Engineer", Level = 1 };
                //UserLevel Engineer = new UserLevel(){ Name = "AlarmAcknowledge" };
                UserLevel Admin = new UserLevel() { Name = "Admin", Level = 3 };
                UserLevel Developer = new UserLevel() { Name = "Developer", Level = 4 };

                Operator.UserRights = new List<UserRight>()
                {
                    AlarmAcknowledge
                   ,VisionView
                   //,IOView
                   //,IOControl
                   //,ManualProcess
                   //,DeviceControl
                   ,LotControl
                   ,Overview
                   ,LogView
                   ,StatisticsView
                   ,SystemInfoView
                };

                Engineer.UserRights = new List<UserRight>();

                Engineer.UserRights.AddRange(Operator.UserRights);
                Engineer.UserRights.Add(ManualProcess);
                Engineer.UserRights.Add(DeviceControl);
                Engineer.UserRights.Add(IOControl);
                Engineer.UserRights.Add(Setting);
                Engineer.UserRights.Add(Calibration);
                Engineer.UserRights.Add(LaserConfiguration);
                Engineer.UserRights.Add(VisionSensorConfiguration);

                Admin.UserRights = new List<UserRight>();
                Admin.UserRights.AddRange(Engineer.UserRights);
                Admin.UserRights.Add(VisionControl);
                Admin.UserRights.Add(CalibrationSet);
                Admin.UserRights.Add(RecipeSetting);
                Admin.UserRights.Add(UserAdd);
                Admin.UserRights.Add(UserRemove);

                Developer.UserRights = new List<UserRight>();
                Developer.UserRights.AddRange(Admin.UserRights);
                Developer.UserRights.Add(RightEdit);
                Developer.UserRights.Add(Debug);

                //UserRight RecipeAdding = new UserRight() { Name = "RecipeAdd" };
                //UserRight RecipeSetting = new UserRight() { Name = "RecipeSetting" };
                //UserRight RecipeRemove = new UserRight() { Name = "RecipeRemove" };
                User userDeveloper = new User()
                {
                    Username = "Developer".ToLower()
                    ,
                    DisplayName = "Developer"
                    ,
                    UserLevel = Developer
                    ,
                    Password = encrypt("AKBPass5012")
                    //, CreatorId = null
                };
                Users = new ObservableCollection<User>()
                {
                    userDeveloper,

                    new User(){
                        Username = "Operator".ToLower()
                       , DisplayName = "Operator"
                       ,UserLevel = Operator
                       ,Password = encrypt("123")
                       //, CreatorId = userDeveloper.UserID
                       , Creator = userDeveloper
                    },

                    new User(){
                        Username = "Engineer".ToLower()
                        , DisplayName = "Engineer"
                       ,UserLevel = Engineer
                       ,Password = encrypt("123")
                       //, CreatorId = userDeveloper.UserID
                       , Creator = userDeveloper
                    },
                    new User(){
                        Username = "Admin".ToLower()
                       , DisplayName = "Admin"
                       ,UserLevel = Admin
                       ,Password = encrypt("123")
                       //, CreatorId = userDeveloper.UserID
                       , Creator = userDeveloper
                    },
                    //new User(){
                    //    Username = "Developer"
                    //   ,UserLevel = Developer
                    //   ,Password = encrypt("AKBPass5012")
                    //},
                };


            }
            catch (Exception)
            {

                throw;
            }

        }
        public bool LoadHistoryFromText()
        {
            var fn = $"{RecentLoginUserFilename}.txt";
            var fn_temp = $"{RecentLoginUserFilename}_temp.txt";
            var fn_backup = $"{RecentLoginUserFilename}_backup.txt";

            FileRecovery.RecoverFile(fn, fn_temp, fn_backup);

            if (File.Exists(fn))
            {
                var lines = File.ReadAllLines(fn);
                for (int i = 0; i < lines.Length; i++)
                {
                    HistoryUsers.Enqueue(lines[i]);
                }

            }
            else
                return false;


            return (HistoryUsers.Count > 0);
        }

        public bool AddNewAndSaveHistoryToText(string newHistory)
        {
            var fn = $"{RecentLoginUserFilename}.txt";
            var fn_temp = $"{RecentLoginUserFilename}_temp.txt";
            var fn_backup = $"{RecentLoginUserFilename}_backup.txt";


            if (HistoryUsers.Count > 3)
                HistoryUsers.Dequeue();

            if (HistoryUsers.All(x => x != newHistory))
                HistoryUsers.Enqueue(newHistory);

            List<string> lines = new List<string>();
            foreach (string item in HistoryUsers)
            {
                lines.Add(item);
            }

            //// Write the list of strings to the file, overwriting it
            //File.WriteAllLines(fn_temp, lines);
            // Using StreamWriter to overwrite the file with multiple lines
            using (StreamWriter writer = new StreamWriter(fn_temp, false))
            {
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }

            if (File.Exists(fn))
                File.Replace(fn_temp, fn, fn_backup);
            else
                File.Move(fn_temp, fn);


            return (HistoryUsers.Count > 0);

        }
        public bool LoadFromDB()
        {
            try
            {


                using (var context = new MyDBContext())
                {
                    if (context.Users.Count() > 0)
                    {
                        Users = new ObservableCollection<User>(context.Users.Include(x => x.Creator).Include(u => u.UserLevel.UserRights).ToList());
                        return true;
                    }
                    else
                        return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SaveToDB()
        {
            try
            {
                using (var context = new MyDBContext())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            context.Users.AddRange(Users);
                            context.SaveChanges();

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine("Transaction rolled back.");
                            return false;
                        }
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                // Iterate over each validation error and log or handle it appropriately
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool AddUserRight(string userLevel, string rightName)
        {
            bool retVal = false;
            using (var context = new MyDBContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        UserLevel us = context.UserLevels.FirstOrDefault(x => x.Name == userLevel);
                        UserRight ur = context.UserRights.FirstOrDefault(x => x.Name == rightName);

                        if (!us.UserRights.Contains(ur))
                        {
                            us.UserRights.Add(ur);
                            context.SaveChanges();

                            transaction.Commit();
                            retVal = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transaction rolled back.");
                        return false;
                    }
                }
            }
            return retVal;
        }
        public bool RemoveUserRight(string userLevel, string rightName)
        {
            bool retVal = false;
            using (var context = new MyDBContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        UserLevel us = context.UserLevels.FirstOrDefault(x => x.Name == userLevel);
                        UserRight ur = context.UserRights.FirstOrDefault(x => x.Name == rightName);

                        if (us.UserRights.Contains(ur))
                        {
                            us.UserRights.Remove(ur);
                            context.SaveChanges();

                            transaction.Commit();
                            retVal = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transaction rolled back.");
                        return false;
                    }
                }
            }
            return retVal;
        }
        public bool AddUser(string username, string displayName, string userLevel, string password)
        {
            bool retVal = false;
            using (var context = new MyDBContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        UserLevel us = context.UserLevels.FirstOrDefault(x => x.Name == userLevel);

                        User userToAdd = new User()
                        {
                            Username = username.ToLower(),
                            DisplayName = displayName,
                            UserLevel = us,
                            Password = encrypt(password),
                            CreatorId = CurrentUser.UserID
                        };

                        context.Users.Add(userToAdd);
                        context.SaveChanges();

                        transaction.Commit();
                        retVal = true;

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transaction rolled back.");
                        return false;
                    }
                }
            }
            return retVal;
        }
        public bool EditUser(string username, string displayName, string userLevel, string password)
        {
            bool retVal = false;
            using (var context = new MyDBContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        UserLevel levelToUpdate = context.UserLevels.FirstOrDefault(x => x.Name == userLevel);
                        var userToUpdate = context.Users.FirstOrDefault(x => x.Username == username.ToLower());

                        // Check if the customer exists
                        if (userToUpdate != null)
                        {
                            // Update the properties of the customer
                            userToUpdate.DisplayName = displayName;
                            userToUpdate.UserLevel = levelToUpdate;
                            userToUpdate.Password = encrypt(password);
                            userToUpdate.LastUpdate = DateTime.Now;

                            // Save changes to the database
                            context.SaveChanges();
                            transaction.Commit();

                            retVal = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transaction rolled back.");
                    }

                }
            }
            return retVal;
        }

        public bool DeleteUser(User userToDelete)
        {
            bool retVal = false;
            if (userToDelete == null) return false;
            using (var context = new MyDBContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var userToRemove = context.Users.FirstOrDefault(x => x.UserID == userToDelete.UserID);

                        // Check if the customer exists
                        if (userToRemove != null)
                        {
                            // Detach the creator reference from the DbContext

                            // or set the creator property to null
                            userToRemove.Creator = null;

                            // Attach the user entity to the DbContext if it's not already tracked
                            //if (context.Entry(userToRemove).State == EntityState.Detached)
                            //{
                            //    context.Users.Attach(userToRemove);
                            //}
                            //userToRemove.Creator = null;

                            //context.Entry(userToRemove).Reference(u => u.Creator).EntityEntry.State = EntityState.Detached;
                            // Update the properties of the customer
                            context.Users.Remove(userToRemove);
                            //context.Entry(userToRemove).Reference(u => u.Creator).EntityEntry.State = EntityState.Detached;

                            // Save changes to the database
                            context.SaveChanges();
                            transaction.Commit();
                            retVal = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transaction rolled back.");
                    }
                }

            }
            return retVal;
        }

        public string[] GetLowerUserLevelName()
        {
            if (CurrentUser == null) return null;

            using (var context = new MyDBContext())
            {
                var list = context.UserLevels.Where(x => x.Level <= CurrentUser.UserLevel.Level);
                return list.Select(x => x.Name).ToArray();
            }

        }
        public void Logout()
        {
            //currentUser = DEFAULT_USER;
            CurrentUser = null;

            OnUserLoggedIn(EventArgs.Empty);
            //updateUserAccess();
        }

        #endregion Public Methods
    }
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [Index(IsUnique = true)]
        [MaxLength(30)]
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdate { get; set; }

        [ForeignKey("UserLevel")]
        public int? UserLevelId { get; set; }
        public virtual UserLevel UserLevel { get; set; }
        public int? CreatorId { get; set; }
        public User Creator { get; set; }

        public User()
        {
            Created = DateTime.Now;
            LastUpdate = DateTime.Now;
        }

    }
    public class UserLevel
    {
        [Key]
        public int UserLevelId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }

        public virtual List<UserRight> UserRights { get; set; }

        public UserLevel() { }
    }
    public class UserRight
    {
        [Key]
        public int UserRightId { get; set; }
        public string Name { get; set; }
        public virtual List<UserLevel> UserLevels { get; set; }


        public UserRight() { }

        //public int UserLevelId { get; set; } // Foreign key
        //public UserLevel UserLevel { get; set; } // Navigation property
    }


}
