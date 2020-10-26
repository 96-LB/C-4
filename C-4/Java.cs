using Microsoft.Win32;
using System.IO;

namespace C_4
{
    /// <summary>
    /// Holds information about a Java installation
    /// </summary>
    struct Java
    {

        #region Constants

        /// <summary>
        /// The name of development Java
        /// </summary>
        public const string JDK = "Java Development Kit";

        /// <summary>
        /// The name of runtime Java
        /// </summary>
        public const string JRE = "Java Runtime Environment";

        /// <summary>
        /// The name of the Java runtime executable
        /// </summary>
        public const string JAVA = "java.exe";

        /// <summary>
        /// The name of the Java compiler executable
        /// </summary>
        public const string JAVAC = "javac.exe";

        #endregion

        #region Fields

        /// <summary>
        /// Whether this is a Java Development Kit or a Java Runtime Environment installation
        /// </summary>
        public bool development;

        /// <summary>
        /// The type of error that was reached when searching for the installation
        /// </summary>
        public int errtype;

        /// <summary>
        /// The description of the error that was reached when searching for the installation
        /// </summary>
        public string error;

        /// <summary>
        /// The version string of this installation
        /// </summary>
        public string version; 

        /// <summary>
        /// The JavaHome directory of this installation
        /// </summary>
        public string home; 

        /// <summary>
        /// The main executable of this installation
        /// </summary>
        public string exe;

        #endregion

        #region Methods

        /// <summary>
        /// Searches for the Java installation in both the 64-bit and 32-bit registries
        /// </summary>
        /// <param name="development">Whether to search for a Java Development Kit or a Java Runtime Environment</param>
        /// <returns>Information about the current Java installation</returns>
        public static Java GetJava(bool development)
        {
            Java java64 = GetJava(development, RegistryView.Registry64); //try to find a Java installation in the 64-bit registry
            if (java64.errtype > 0)
            {
                Java java32 = GetJava(development, RegistryView.Registry32); //if there's an error, try the 32-bit registry
                if (java32.errtype == 0 || java32.errtype > java64.errtype) //return whichever one got farther in the process
                {
                    return java32;
                }
            }
            return java64;
        }

        /// <summary>
        /// Searches for the Java installation in the specified registry
        /// </summary>
        /// <param name="development">Whether to search for a Java Development Kit or a Java Runtime Environment</param>
        /// <returns>Information about the current Java installation in the specified registry</returns>
        public static Java GetJava(bool development, RegistryView view)
        {
            string JTYPE = development ? JDK : JRE; //the type of Java installation
            string BTYPE = development ? JAVAC : JAVA; //the name of the relevant Java executable
            Java output = new Java() { development = development }; //the output information

            //check each property step-by-step, returning error information if the registry key can't be found
            //the output.error messages are sufficient as comments lol
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            if (key == null)
            {
                output.error = "Windows Registry is not available.";
                output.errtype = 1;
            }
            else
            {
                RegistryKey javakey = key.OpenSubKey($@"SOFTWARE\JavaSoft\{JTYPE}");
                if (javakey == null)
                {
                    output.error = $"Your computer is missing a {JTYPE}. Try installing the latest version.";
                    output.errtype = 2;
                }
                else
                {
                    object version = javakey.GetValue("CurrentVersion");
                    if (version == null)
                    {
                        output.error = $"Your computer has some {JTYPE} registry keys but none seem to be installed. Try re-installing a {JTYPE}.";
                        output.errtype = 3;
                    }
                    else
                    {
                        output.version = version.ToString();
                        RegistryKey verkey = javakey.OpenSubKey(output.version);
                        if (verkey == null)
                        {
                            output.error = $"Java {output.version} is marked as your default {JTYPE} version but lacks a complete installation. Try re-installing {JTYPE} {output.version}.";
                            output.errtype = 4;
                        }
                        else
                        {
                            object home = verkey.GetValue("JavaHome");
                            if (home == null)
                            {
                                output.error = $"Java {output.version} is marked as your default {JTYPE} version but lacks a JavaHome directory. Try re-installing {JTYPE} {output.version}.";
                                output.errtype = 5;
                            }
                            else
                            {
                                output.home = home.ToString();
                                string exe = Path.Combine(output.home, "bin", BTYPE);
                                if (!File.Exists(exe))
                                {
                                    output.error = $"Failed to locate {BTYPE} in {output.home}. Try re-installing {JTYPE} {output.version}.";
                                    output.errtype = 6;
                                }
                                else
                                {
                                    output.exe = exe;
                                }
                            }
                        }
                    }
                }
            }

            return output;
        }

        #endregion

    }
}
