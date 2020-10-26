using System;
using System.Diagnostics;

namespace C_4
{
    /// <summary>
    /// A wrapper class that facilitates asynchronous reading of process output into a string
    /// </summary>
    sealed class OutputReader : IDisposable
    {

        #region Properties

        /// <summary>
        /// The <see cref="System.Diagnostics.Process"/> being recorded
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// The text sent by the <see cref="System.Diagnostics.Process"/> to its standard output stream
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// The text sent by the <see cref="System.Diagnostics.Process"/> to its standard error stream
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Whether the output stream is currently being read
        /// </summary>
        public bool ReadingOutput { get; private set; }

        /// <summary>
        /// Whether the error stream is currently being read
        /// </summary>
        public bool ReadingError { get; private set; }

        /// <summary>
        /// The exit code of the <see cref="System.Diagnostics.Process"/>, only available after exiting
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// The time that the <see cref="System.Diagnostics.Process"/> spent running, only available after exiting
        /// </summary>
        public TimeSpan Time { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputReader"/> that wraps the specified <see cref="System.Diagnostics.Process"/>
        /// </summary>
        /// <param name="process">The <see cref="System.Diagnostics.Process"/> to be recorded</param>
        public OutputReader(Process process)
        {
            //initializes variables
            Process = process;
            Output = "";
            Error = "";

            //enables recording of data on process exit
            Process.EnableRaisingEvents = true;
            Process.Exited += GetExitData;
        }

        #endregion

        #region Methods

        #region Output

        /// <summary>
        /// Begins asynchronously reading from the standard output stream
        /// </summary>
        public void BeginOutput()
        {
            if (!ReadingOutput) //if not already reading, add the delegate and start reading
            {
                Process.OutputDataReceived += ReadOutput;
                Process.BeginOutputReadLine();
                ReadingOutput = true;
            }
        }

        /// <summary>
        /// A delegate for reading from the standard output stream
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">A <see cref="DataReceivedEventArgs"/> that contains the event data</param>
        private void ReadOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) //if there is any data, append it to a new line on the output string
            {
                Output += e.Data + Environment.NewLine;
            }
        }

        /// <summary>
        /// Stops reading from the standard output stream
        /// </summary>
        public void EndOutput()
        {
            if (ReadingOutput) //if output is currently being received, cancel it and remove the event
            {
                Process.CancelOutputRead();
                Process.OutputDataReceived -= ReadOutput;
                ReadingOutput = false;
            }
        }

        #endregion

        #region Error

        /// <summary>
        /// Begins asynchronously reading from the standard error stream
        /// </summary>
        public void BeginError()
        {
            if (!ReadingError) //if not already reading, add the delegate and start reading
            {
                Process.ErrorDataReceived += ReadError;
                Process.BeginErrorReadLine();
                ReadingError = false;
            }
        }

        /// <summary>
        /// A delegate for reading from the standard error stream
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">A <see cref="DataReceivedEventArgs"/> that contains the event data</param>
        private void ReadError(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) //if there is any data, append it to a new line on the error string
            {
                Error += e.Data + Environment.NewLine;
            }
        }

        /// <summary>
        /// Stops reading from the standard error stream
        /// </summary>
        public void EndError()
        {
            if (ReadingError) //if error is currently being received, cancel it and remove the event
            {
                Process.CancelErrorRead();
                Process.ErrorDataReceived -= ReadError;
                ReadingError = false;
            }
        }

        #endregion

        #region Wait/Kill

        /// <summary>
        /// Waits indefinitely until the wrapped <see cref="System.Diagnostics.Process"/> exits
        /// </summary>
        public void Wait()
        {
            Process.WaitForExit(); //you have three guesses as to what this line does
        }

        /// <summary>
        /// Waits until the wrapped<see cref= "System.Diagnostics.Process" /> exits or until the specified time elapses, returning whether the<see cref="System.Diagnostics.Process"/> exited
        /// </summary>
        /// <param name="milliseconds">The maximum time to wait, in milliseconds</param>
        /// <returns>True if the process exited, or false if the timeout was reached</returns>
        public bool Wait(int milliseconds)
        {
            return Process.WaitForExit(milliseconds);  //returns true if the process exited and false if the timeout occurred
        }

        /// <summary>
        /// Terminates the <see cref="System.Diagnostics.Process"/> and stops reading output
        /// </summary>
        public void Kill()
        {
            if (!Process.HasExited) //if the process is running, kill it
            {
                Process.Kill();
            }

            //wait until the process is truly finished and then stop tracking output 
            Process.WaitForExit();
            EndOutput();
            EndError();
        }

        /// <summary>
        /// Waits for the <see cref="System.Diagnostics.Process"/> to exit, forcefully terminating it after a specified timeout
        /// </summary>
        /// <param name="milliseconds">The maximum time to wait, in milliseconds</param>
        /// <returns>True if the process exited naturally, or false if the timeout was reached</returns>
        public bool Kill(int milliseconds)
        {
            bool output = false; //whether the process ended normally
            if (Wait(milliseconds)) //if the process doesn't end in time, we must kill it
            {
                output = true;
            }

            //kill the program
            Kill();
            return output;
        }

        #endregion

        #region Exit Data

        /// <summary>
        /// Collects data of the <see cref="System.Diagnostics.Process"/> upon exit
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">An object that contains no event data</param>
        private void GetExitData(object sender, EventArgs e)
        {
            //collects data from the process
            Time = Process.ExitTime - Process.StartTime;
            ExitCode = Process.ExitCode;

            //unsubscribes from the event
            Process.Exited -= GetExitData;
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Releases all components used by the <see cref="OutputReader"/>
        /// </summary>
        public void Dispose()
        {
            Process.Dispose(); //disposes the process
        }

        #endregion

        #endregion

    }
}
