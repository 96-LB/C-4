using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_4
{
    /// <summary>
    /// Represents a collection of configuration settings
    /// </summary>
    class Config
    {

        #region Field

        /// <summary>
        /// The collection of configuration settings
        /// </summary>
        private readonly Dictionary<string, string> settings = new Dictionary<string, string>();

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the value associated with the specified configuration setting
        /// </summary>
        /// <param name="key">The configuration setting to query</param>
        /// <returns>The value of the specified configuration setting, or null if it is unset</returns>
        string this[string key] { get => settings.ContainsKey(key) ? settings[key] : null; }

        #endregion

        #region Constructor

        /// <summary>
        /// Loads configuration settings from the specified file
        /// </summary>
        /// <param name="filename">The file from which to load configuration settings</param>
        public Config(string filename)
        {
            if (File.Exists(filename)) //if the file exists, load the configuration settings
            {
                char[] split = new char[] { '=' }; //the settings delimeter
                foreach (string line in File.ReadAllLines(filename)) //for every line in the file...
                {
                    string[] parts = line.Split(split, 2); //try to split the line by the delimeter
                    if (parts.Length == 2) //if successfully split, set the corresponding configuration setting
                    {
                        settings[parts[0]] = parts[1];
                    }
                }
            }
        }

        #endregion

    }
}
