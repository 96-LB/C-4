using Microsoft.Win32;
using System;
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
        public const string JDK = "JDK";

        /// <summary>
        /// The name of legacy development Java
        /// </summary>
        public const string LJDK = "Java Development Kit";

        /// <summary>
        /// The name of runtime Java
        /// </summary>
        public const string JRE = "JRE";

        /// <summary>
        /// The name of legacy runtime Java
        /// </summary>
        public const string LJRE = "Java Runtime Environment";

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
        /// Whether this is a 32-bit installation
        /// </summary>
        public bool is32bit;

        /// <summary>
        /// Whether this is a legacy installation
        /// </summary>
        public bool legacy;

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

        #region Enum

        /// <summary>
        /// Specifies search options that are used for locating Java installations
        /// </summary>
        [Flags]
        public enum JavaFlags
        {
            /// <summary>
            /// Specifies that no search settings are defined
            /// </summary>
            Default = 0,

            /// <summary>
            /// Specifies to search for a 32-bit Java installation
            /// </summary>
            Registry32 = 1 << 0,

            /// <summary>
            /// Specifies to search for a legacy Java installation
            /// </summary>
            Legacy = 1 << 1,

            /// <summary>
            /// Specifies to search for a Java Development Kit installation
            /// </summary>
            Development = 1 << 2
        }

        #endregion

        #region Methods

        /// <summary>
        /// Searches for any Java installation, preferring the specified search options
        /// </summary>
        /// <param name="flags">The search options to prefer when locate a Java installation</param>
        /// <param name="locked">The search options specified in the flags which must be used when locating a Java installation</param>
        /// <returns>Information about the current located installation</returns>
        public static Java SearchJava(JavaFlags flags = JavaFlags.Default, JavaFlags locked = JavaFlags.Default)
        {
            Java java = GetJava(flags); //try to find a Java installation with the default settings
            int[] vals = (int[])Enum.GetValues(typeof(JavaFlags)); //enumerate the flag values
            for(int i = 1; i < 1 << vals.Length; i++) //for every combination of flag values...
            {
                if(java.errtype == 0) //if an installation has already been found, break
                {
                    break;
                }
                JavaFlags flags2 = flags; //a modified set of search options
                for(int j = 0; j < vals.Length; j++) //for every flag
                {
                    //if the flag is not locked and is part of the current combination, toggle it
                    if(((int)locked & vals[j]) == 0 && (i & vals[j]) != 0)
                    {
                        flags2 ^= (JavaFlags)vals[j];
                    }
                }
                Java java2 = GetJava(flags2); //try to find a Java installation with the modified sttings
                if(java2.errtype == 0 || java2.errtype < java.errtype) //if the new search had better results, store it
                {
                    java = java2;
                }
            }
            return java; //returns the located installation information
        }

        /// <summary>
        /// Searches for the Java installation with the specified settings
        /// </summary>
        /// <param name="flags">The search options to use to locate a Java installation</param>
        /// <returns>Information about the located Java installation</returns>
        public static Java GetJava(JavaFlags flags)
        {
            string JTYPE = flags.HasFlag(JavaFlags.Development) ? JDK : JRE; //the type of Java installation
            if (flags.HasFlag(JavaFlags.Legacy))
            {
                JTYPE = flags.HasFlag(JavaFlags.Development) ? LJDK : LJRE; //the legacy registry keys for Java installations
            }
            string BTYPE = flags.HasFlag(JavaFlags.Development) ? JAVAC : JAVA; //the name of the relevant Java executable
            RegistryView VIEW = flags.HasFlag(JavaFlags.Registry32) ? RegistryView.Registry32 : RegistryView.Registry64; //the registry to search in

            Java output = new Java()
            {
                development = flags.HasFlag(JavaFlags.Development),
                is32bit = flags.HasFlag(JavaFlags.Registry32),
                legacy = flags.HasFlag(JavaFlags.Legacy)
            }; //the output Java installation information

            //check each property step-by-step, returning error information if the registry key can't be found
            //the output.error messages are sufficient as comments lol
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, VIEW);
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

            return output; //returns the located installation information
        }

        #endregion

    }
}
