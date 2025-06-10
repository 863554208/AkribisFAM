using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using AkribisFAM.Models.Base;

namespace AkribisFAM.Models.Base
{
    public abstract class AKBParameterBase : ViewModelBase, INotifyPropertyChanged
    {

        #region Private Properties
        protected string ParamName = string.Empty;
        protected object liveParamBase;
        protected object cacheParamBase;

        #endregion Private Properties


        #region Public Properties
        public string SettingsFolderPath { get; set; }
        public string SettingsFilename => $"{ParamName}.json";
        public string RecipeName => ParamName;
        public string FilePath => Path.Combine(SettingsFolderPath, SettingsFilename);
        public string SettingsBackupFolderPath => Path.Combine(SettingsFolderPath, "Backups");
        public bool hasBackupFile => Directory.Exists(SettingsBackupFolderPath) ? Directory.GetFiles(SettingsBackupFolderPath).Length > 0 : false;



        #endregion Public Properties


        #region Constructors

        public AKBParameterBase([CallerMemberName] string propName = null) 
        {
            ParamName = propName;
        }

        protected AKBParameterBase()
        {
        }
        #endregion Constructors 

        #region Public Methods
        private bool InitParamHasSet = false;
        public bool Initialize(string settingPath)
        {

            SetInitParam(settingPath);

            return Initialize();
        }

        public bool Initialize()
        {
            bool retVal = false;

            if (InitParamHasSet)
            {
                if (!Directory.Exists(SettingsFolderPath))
                {
                    Directory.CreateDirectory(SettingsFolderPath);
                }

                if (File.Exists(FilePath))
                {
                    if (Json_Load(FilePath))
                    {
                        SetCacheParam();
                        SetLiveParam();
                        retVal = true;
                    }
                }
                else
                {
                    //AKBMessageBox.Show("New System parameters was created.", msgIcon: AKBMessageBox.MessageBoxIcon.Done);
                    retVal = Save();
                }

                return retVal;
            }
            else
            {
                return retVal;
            }
        }
        public void SetInitParam(string settingPath)
        {

            SettingsFolderPath = settingPath;

            InitParamHasSet = true;
        }

        /// <summary>
        /// SetCacheParam 
        /// </summary>
        public abstract void SetCacheParam();
        public abstract void SetLiveParam();

        /// <summary>
        /// Deserialize data value from filePath and set cacheParamBase, 
        /// After this method is called, SetCacheParam() and SetLiveParam() shall be called externally at the derived class level to set cacheParamBase to ParameterSet class format 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual bool Json_Load(string filePath)
        {
            bool retVal = false;
            JsonSerializer serializer = null;
            FileStream fStream = null;
            TextReader reader = null;
            JsonTextReader jread = null;
            try
            {
                serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
                fStream = new FileStream(filePath, FileMode.Open);
                reader = new StreamReader(fStream);
                jread = new JsonTextReader(reader);
                object paramset = serializer.Deserialize(jread);

                cacheParamBase = paramset;
                //retVal = emgr.normal();
                retVal = true;
            }
            catch (Exception ex)
            {
                //retVal = emgr.set_error(DefaultError.SysFileRFail, ex.Message);
            }
            finally
            {
                jread?.Close();
                reader?.Close();
                fStream?.Close();
            }

            return retVal;
        }
        private bool Json_save()
        {

            var retVal = true;
            JsonSerializer serializer = null;
            StreamWriter fWriter = null;
            try
            {
                string sPath = Path.Combine(SettingsFolderPath, SettingsFilename);
                serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                fWriter = new StreamWriter(sPath);
                //serializer.Serialize(fWriter, this.ParamSet);
                serializer.Serialize(fWriter, liveParamBase);
               // retVal = emgr.normal();
            }
            catch (Exception ex)
            {
                //retVal = emgr.set_error(DefaultError.SysFileWFail, ex.Message);
            }
            finally
            {
                fWriter?.Close();
            }
            return retVal;
        }
        public virtual bool Save()
        {
            bool retVal = false;
            if (Json_save())
            {
                if (BackupJsonSetting())
                {
                    //AKBMessageBox.Show("Successfully saved" , msgIcon : AKBMessageBox.MessageBoxIcon.Done);
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Everytime new Json is saved, BackupJsonSetting is called to generate a backup setting at backup folder
        /// </summary>
        /// <returns></returns>
        protected bool BackupJsonSetting()
        {
            try
            {
                string fileName = DateTime.Now.ToString("BU_yyyyMMdd_HHmmss_") + SettingsFilename;
                string sourceFile = Path.Combine(SettingsFolderPath, SettingsFilename);
                string targetPath = Path.Combine(SettingsFolderPath, "Backups");
                string destFile = Path.Combine(SettingsFolderPath, "Backups", fileName);
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, destFile, true);
                }
            }
            catch (Exception ex)
            {
               // return emgr.set_error(DefaultError.SysFileWFail, ex.Message);
            }
            return true;
        }

       #endregion Public Methods

    }
    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _baseDescriptor;
        private readonly bool _isBrowsable;
        private readonly bool _isReadOnly;

        public CustomPropertyDescriptor(PropertyDescriptor baseDescriptor, bool isBrowsable, bool isReadOnly)
            : base(baseDescriptor)
        {
            _baseDescriptor = baseDescriptor;
            _isBrowsable = isBrowsable;
            _isReadOnly = isReadOnly;
        }

        public override bool CanResetValue(object component) => _baseDescriptor.CanResetValue(component);
        public override Type ComponentType => _baseDescriptor.ComponentType;
        public override object GetValue(object component) => _baseDescriptor.GetValue(component);
        public override bool IsReadOnly => _isReadOnly;
        public override Type PropertyType => _baseDescriptor.PropertyType;
        public override void ResetValue(object component) => _baseDescriptor.ResetValue(component);
        public override void SetValue(object component, object value) => _baseDescriptor.SetValue(component, value);
        public override bool ShouldSerializeValue(object component) => _baseDescriptor.ShouldSerializeValue(component);
        public override bool IsBrowsable => _isBrowsable;
    }


    public class PropertyModifier
    {
        public static CustomPropertyDescriptor GetCustomDescriptor(object obj, string propertyName, bool isBrowsable, bool isReadOnly)
        {
            var properties = TypeDescriptor.GetProperties(obj);
            var property = properties[propertyName];

            if (property == null)
            {
                throw new ArgumentException("Property not found.", nameof(propertyName));
            }

            return new CustomPropertyDescriptor(property, isBrowsable, isReadOnly);
        }
    }

}
