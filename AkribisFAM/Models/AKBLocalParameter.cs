
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AkribisFAM.Models.Base;
using System.Diagnostics;

namespace AkribisFAM.Models
{
    /// <summary>
    /// AKBLocalParam provide parameters that controls the behavior of the software
    /// For Inherit t
    /// </summary>
    public class AKBLocalParam : AKBParameterBase, INotifyPropertyChanged
    {

        #region Private Member
        /// <summary>
        /// This class include all the paramters that will be used in the application.
        /// Use snippets by for AKBint, AKBdouble, AKBbool, AKBEnum to add property
        /// Only this class should be inherit and modify by programmer
        /// </summary>
        public class ParameterSet : ViewModelBase
        {
            #region Public Properties

            private AKBint _targetUPH = new AKBint(1800, 0, 100000);
            [Browsable(true), ReadOnly(false), Category("Process"), Description("The target number of units produced per hour by the machine")]
            public int TargetUPH
            {
                get { return _targetUPH.Value; }
                set { _targetUPH.Value = value; OnPropertyChanged(); }
            }


            private AKBint _processTimeout = new AKBint(3000, 0, 5000);
            [Browsable(true), ReadOnly(false), Category("Process"), Description("The wait time for the motion to reach position before raising timeout, in milliseconds")]
            public int ProcessTimeout
            {
                get { return _processTimeout.Value; }
                set { _processTimeout.Value = value; OnPropertyChanged(); }
            }

            private AKBstring _localModuleName = new AKBstring("FAM");
            [Browsable(true), ReadOnly(false), Category("System"), Description("The name of this machine")]
            public string MachineName
            {
                get { return _localModuleName.Value; }
                set { _localModuleName.Value = value; OnPropertyChanged(); }
            }

            private AKBint _databaseBackupDaysInterval = new AKBint(7, 0, 100000);
            [Browsable(true), ReadOnly(false), Category("System"), Description("Time interval (in days) for performing a full backup of the MySQL database.\n0 : Disable backup")]
            public int DatabaseBackupDaysInterval
            {
                get { return _databaseBackupDaysInterval.Value; }
                set { _databaseBackupDaysInterval.Value = value; OnPropertyChanged(); }
            }
            private AKBbool _simulationMode = new AKBbool();
            [Browsable(false), ReadOnly(false), Category("System"), Description("Simulation Mode for offline test")]
            public bool SimulationMode
            {
                get { return _simulationMode.Value; }
                set { _simulationMode.Value = value; OnPropertyChanged(); }
            }
            private AKBenum _runMode = new AKBenum(typeof(ProcessMode), (int)ProcessMode.Dryrun);
            [Browsable(true), ReadOnly(false), Category("Process"), Description("The process mode for machine auto run")]
            public ProcessMode RunMode
            {
                get { return (ProcessMode)_runMode.Value; }
                set { _runMode.Value = (int)value; OnPropertyChanged(); }
            }

            private AKBint _speedPercentage = new AKBint(30, 5, 100);
            [Browsable(true), ReadOnly(false), Category("Process"), Description("Percentage of full speed in percentage %")]
            public int SpeedPercentage
            {
                get { return _speedPercentage.Value; }
                set { _speedPercentage.Value = value; OnPropertyChanged(); }
            }


        
          
            private AKBint _rejectX = new AKBint(100, 1, 100000);
            [Browsable(false), ReadOnly(false), Category("Reject"), Description("X component of X over Y rejection")]
            public int RejectX
            {
                get { return _rejectX.Value; }
                set { _rejectX.Value = value; OnPropertyChanged(); }
            }


            private AKBint _rejectY = new AKBint(100, 1, 100000);
            [Browsable(false), ReadOnly(false), Category("Reject"), Description("X component of X over Y rejection")]
            public int RejectY
            {
                get { return _rejectY.Value; }
                set { _rejectY.Value = value; OnPropertyChanged(); }
            }

            private AKBbool _enablePicker1 = new AKBbool(true);
            [Browsable(true), ReadOnly(false), Category("Foam Assembly Station"), Description("Enable/disable picker 1")]
            public bool EnablePicker1
            {
                get { return _enablePicker1.Value; }
                set { _enablePicker1.Value = value; OnPropertyChanged(); }
            }

            private AKBbool _enablePicker2 = new AKBbool(true);
            [Browsable(true), ReadOnly(false), Category("Foam Assembly Station"), Description("Enable/disable picker 2")]
            public bool EnablePicker2
            {
                get { return _enablePicker2.Value; }
                set { _enablePicker2.Value = value; OnPropertyChanged(); }
            }

            private AKBbool _enablePicker3 = new AKBbool(true);
            [Browsable(true), ReadOnly(false), Category("Foam Assembly Station"), Description("Enable/disable picker 3")]
            public bool EnablePicker3
            {
                get { return _enablePicker3.Value; }
                set { _enablePicker3.Value = value; OnPropertyChanged(); }
            }

            private AKBbool _enablePicker4 = new AKBbool(true);
            [Browsable(true), ReadOnly(false), Category("Foam Assembly Station"), Description("Enable/disable picker 4")]
            public bool EnablePicker4
            {
                get { return _enablePicker4.Value; }
                set { _enablePicker4.Value = value; OnPropertyChanged(); }
            }

            private AKBbool _enableTraceLog = new AKBbool();
            [Browsable(true), ReadOnly(false), Category("Logging"), Description("Enable/disable logging of individual process time")]
            public bool EnableTraceLog
            {
                get { return _enableTraceLog.Value; }
                set { _enableTraceLog.Value = value; OnPropertyChanged(); }
            }

            private AKBint _logFileAge = new AKBint(180, 30, 10000);
            [Browsable(true), ReadOnly(false), Category("Logging"), Description("Number of days system will keep the log file")]
            public int LogFileAge
            {
                get { return _logFileAge.Value; }
                set { _logFileAge.Value = value; OnPropertyChanged(); }
            }

            private AKBint _alarmAge = new AKBint(720, 180, 10000);
            [Browsable(true), ReadOnly(false), Category("Logging"), Description("Number of days system will keep the alarm in database")]
            public int AlarmAge
            {
                get { return _alarmAge.Value; }
                set { _alarmAge.Value = value; OnPropertyChanged(); }
            }
            private AKBdouble _foamXOffset = new AKBdouble(16, 0, 200);
            [Browsable(true), ReadOnly(false), Category("Foam Assembly Station"), Description("X pitch between pickers")]
            public double FoamXOffset
            {
                get { return _foamXOffset.Value; }
                set { _foamXOffset.Value = value; OnPropertyChanged(); }
            }
            private AKBdouble _recheckXOffset = new AKBdouble(25.4, 0, 200);
            [Browsable(true), ReadOnly(false), Category("Recheck Station"), Description("X Offset from film remover to Vision camera")]
            public double RecheckXOffset
            {
                get { return _recheckXOffset.Value; }
                set { _recheckXOffset.Value = value; OnPropertyChanged(); }
            }

            private AKBdouble _recheckYOffset = new AKBdouble(56.3, 0, 200);
            [Browsable(true), ReadOnly(false), Category("Recheck Station"), Description("Y Offset from film remover to Vision camera")]
            public double RecheckYOffset
            {
                get { return _recheckYOffset.Value; }
                set { _recheckYOffset.Value = value; OnPropertyChanged(); }
            }


            #endregion Public Properties

            #region Public Methods
            /// <summary>
            /// Clone or return a new and non-identical instance of ParameterSet
            /// </summary>
            /// <returns></returns>
            public ParameterSet Clone()
            {
                ParameterSet cloneSet = new ParameterSet();
                PropertyInfo[] properties = typeof(ParameterSet).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    if (property.CanWrite)
                    {
                        object value = property.GetValue(this);
                        property.SetValue(cloneSet, value);
                    }
                }

                return cloneSet;
            }
            /// <summary>
            /// Check all the properties has equal value with otherSet
            /// </summary>
            /// <param name="otherSet">ParameterSet to be compare</param>
            /// <returns></returns>
            public bool Equals(ParameterSet otherSet)
            {
                PropertyInfo[] properties = typeof(ParameterSet).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    object thisValue = property.GetValue(this);
                    object otherValue = property.GetValue(otherSet);

                    if (!thisValue.Equals(otherValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            #endregion Public Methods

        }


        #endregion Private Member


        #region Private Properties

        private ParameterSet _cacheParam = new ParameterSet();
        private ParameterSet _liveParam = new ParameterSet();

        #endregion Private Properties

        #region Public Properties
        /// <summary>
        /// CacheParam is a set of parameters that will be shown to user,
        /// allow user to modify and make changes setting without changing the live parameter set
        /// NOTE !!! Never use this parameter set to set process
        /// </summary>
        public ParameterSet CacheParam
        {
            get { return _cacheParam; }
            set { _cacheParam = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsChanged)); }
        }
        /// <summary>
        /// LiveParam is a set of parameters used by the software, software will change its behavior based on this
        /// </summary>
        public ParameterSet LiveParam
        {
            get { return _liveParam; }
            set { _liveParam = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsChanged)); }
        }

        /// <summary>
        /// IsChanged check the difference in between CacheParam and LiveParam
        /// True : If the parameters not tally or has changed
        /// False : If the Live and Cache parameters are same
        /// </summary>

        public bool IsChanged => !CacheParam.Equals(LiveParam);

        #endregion Public Properties


        #region Constructors
        public AKBLocalParam( [CallerMemberName] string propName = null) : base(propName)
        {
           // if (InEMngr != null) emgr = InEMngr;
            liveParamBase = _liveParam;
            cacheParamBase = _cacheParam;

            ParamName = propName;
        }
        #endregion Constructors
        public class PropertyEventArgs : EventArgs
        {
            public List<PropertyInfo> propertyInfos { get; set; } = new List<PropertyInfo>();
            public PropertyEventArgs(List<PropertyInfo> infos)
            {
                propertyInfos = infos;
            }
        }
        // Declare the event using the EventHandler delegate and the custom event argument
        public event EventHandler<PropertyEventArgs> ChangesSaved;

        // Method to invoke the event
        protected virtual void onChangesSaved(PropertyEventArgs e)
        {
            ChangesSaved?.Invoke(this, e);
        }

        #region Public Methods
        /// <summary>
        /// Overwrite  live parameters by cache parameters set
        /// </summary>
        /// <returns></returns>akbme
        public override bool Save()
        {
            bool retVal = false;

            List<PropertyInfo> diffPropInfos = GetDifferentProperties(LiveParam, CacheParam);

            SetLiveParam();
            retVal = base.Save();

            if (retVal) onChangesSaved(new PropertyEventArgs(diffPropInfos));

            return retVal;
        }

        /// <summary>
        /// Load data from file and temporary store loaded parameters into cache set
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Load(string filePath)
        {
            bool retVal = false;
            try
            {
                if (File.Exists(filePath))
                {
                    retVal = Json_Load(filePath);
                    SetCacheParam();
                }
            }
            catch (Exception ex)
            {
                //emgr.set_error(DefaultError.SysFileRFail, $"Failed to load {ParamName} json file");
                retVal = false;
            }

            return retVal;
        }

        public bool Load()
        {
            return Load(SettingsFolderPath);
        }
        /// <summary>
        /// Set the loaded json object to cache parameters set
        /// </summary>
        public override void SetCacheParam()
        {
            //CacheParam = ((JObject)cacheParamBase).ToObject<ParameterSet>();
            CacheParam = ((JObject)cacheParamBase).ToObject<ParameterSet>();
        }

        /// <summary>
        /// Set the live parameters set to live data
        /// </summary>
        public override void SetLiveParam()
        {

            LiveParam = CacheParam.Clone();
            liveParamBase = LiveParam;
        }

        /// <summary>
        /// Revert cache changes by overwritting cache by live parameters set
        /// </summary>
        public void UndoChanged()
        {
            if (IsChanged)
            {
                CacheParam = LiveParam.Clone();
                cacheParamBase = CacheParam;
            }

        }
        public static List<PropertyInfo> GetDifferentProperties<T>(T paramset1, T paramset2)
        {
            List<PropertyInfo> differentProperties = new List<PropertyInfo>();

            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var value1 = property.GetValue(paramset1);
                var value2 = property.GetValue(paramset2);

                if (!value1.Equals(value2))
                {
                    differentProperties.Add(property);
                }
            }

            return differentProperties;
        }


        /// <summary>
        /// Retrieve the list of parameter that was changed
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        public List<PropertyInfo> GetChanges()
        {
            List<PropertyInfo> differentProperties = new List<PropertyInfo>();

            if (CacheParam == null || LiveParam == null)
                return differentProperties;

            // Use reflection to get all properties of the ParameterSet class
            PropertyInfo[] properties = typeof(ParameterSet).GetProperties();

            foreach (var property in properties)
            {
                // Get the values of the property from both CacheParam and LiveParam
                var cacheValue = property.GetValue(CacheParam);
                var liveValue = property.GetValue(LiveParam);

                // Compare the values
                if (!Equals(cacheValue, liveValue))
                {
                    // If they are different, add the property to the list
                    differentProperties.Add(property);
                }
            }

            return differentProperties;
        }
        /// <summary>
        /// Check specific property name has changed and updated between cache and live parameter set
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsPropertyChanged(string propertyName)
        {
            List<PropertyInfo> differentProperties = GetChanges();

            if (differentProperties.Count == 0)
                return false;

            return differentProperties.Any(x => x.Name == propertyName);
        }
        #endregion Public Methods



    }


}
