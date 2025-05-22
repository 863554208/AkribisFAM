using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Manager
{
    #region Global Enumerations

    /// <summary>
    /// Enumeration of the types of directories available.
    /// Reference is the main application directory; the others are inside it.
    /// </summary>
    public enum DirectoryType
    {
        Reference = 0,
        Settings,
        Language,
        Results,
        Recipes,
        Stats,
        Logs,
        Materials
    }

    #endregion Global Enumerations

    /// <summary>
    /// Class for creating and maintaining a folder structure for an application.
    /// The folder names can be defined using the DirectoryType enumeration.
    /// </summary>
    public class DirectoryManager
    {
        #region Private Member

        private int _maxValue = 0;
        private readonly string[] _directoryPath;

        #endregion Private Member

        #region Private Properties

        // (No private properties yet)

        #endregion Private Properties

        #region Public Properties

        /// <summary>
        /// True when the directories have been created and initialized.
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Gets the error message of the last error that occurred.
        /// </summary>
        public string LastErrorMessage { get; private set; } = string.Empty;

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Class instantiation.
        /// </summary>
        public DirectoryManager()
        {
            _maxValue = Enum.GetValues(typeof(DirectoryType)).Length;
            _directoryPath = new string[_maxValue];
            InitializeDirectories();
        }

        #endregion Constructors

        #region Event Handlers/Action Methods

        // (No event handlers or action methods)

        #endregion Event Handlers/Action Methods

        #region Private Methods

        /// <summary>
        /// Initializes all directories defined in the DirectoryType enumeration.
        /// </summary>
        private void InitializeDirectories()
        {
            try
            {
                for (int i = 0; i < _maxValue; i++)
                {
                    if (i == (int)DirectoryType.Reference)
                    {
                        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        _directoryPath[i] = Path.GetDirectoryName(path);
                    }
                    else
                    {
                        var dir = _directoryPath[(int)DirectoryType.Reference];
                        var sub = ((DirectoryType)i).ToString();
                        _directoryPath[i] = Path.Combine(dir, sub);
                    }

                    if (_directoryPath[i] != null && !Directory.Exists(_directoryPath[i]))
                    {
                        Directory.CreateDirectory(_directoryPath[i]);
                    }
                }

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = "Error in InitializeDirectories: " + ex.Message;
                IsInitialized = false;
            }
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Returns the full path to the specified directory type.
        /// </summary>
        /// <param name="dType">Type of directory.</param>
        /// <returns>The directory's full path.</returns>
        public string GetDirectoryPath(DirectoryType dType)
        {
            return _directoryPath[(int)dType];
        }

        /// <summary>
        /// Creates a sub-directory inside the specified directory type.
        /// </summary>
        /// <param name="dType">Type of directory.</param>
        /// <param name="subName">Name of the sub-directory to create.</param>
        /// <param name="newDir">The full path to the new sub-directory.</param>
        /// <returns>False when directory creation failed.</returns>
        public bool CreateSubdirectory(DirectoryType dType, string subName, out string newDir)
        {
            try
            {
                subName = subName.Trim();
                newDir = Path.Combine(_directoryPath[(int)dType], subName);
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = "Error in CreateSubdirectory: " + ex.Message;
                newDir = string.Empty;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a date-named sub-directory inside the given path.
        /// </summary>
        /// <param name="basePath">The full path where the date sub-directory will be created.</param>
        /// <param name="newDir">The full path to the new sub-directory.</param>
        /// <returns>False when directory creation failed.</returns>
        public bool CreateDateSubdirectory(string basePath, out string newDir)
        {
            try
            {
                basePath = basePath.Trim();
                string subName = DateTime.Now.ToString("yyyy-MM-dd");
                newDir = Path.Combine(basePath, subName);
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = "Error in CreateDateSubdirectory: " + ex.Message;
                newDir = string.Empty;
                return false;
            }

            return true;
        }

        #endregion Public Methods
    }
}
