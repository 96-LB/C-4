using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Reflection;
using Markdig;
using Ganss.XSS;
using System.Net;
using System.Net.Http;

namespace C_4
{
    public partial class CFourm : Form
    {

        #region Constants

        /// <summary>
        /// The compilation and execution timeout
        /// </summary>
        public const int TIMEOUT = 10000;

        #region Verdicts

        /// <summary>
        /// The verdict string for a correct answer
        /// </summary>
        public const string AC = "Accepted";

        /// <summary>
        /// The verdict string for a wrong answer
        /// </summary>
        public const string WA = "Wrong Answer";

        /// <summary>
        /// The verdict string for an exceeded time limit
        /// </summary>
        public const string TLE = "Timed Out";

        /// <summary>
        /// The verdict string for a runtime error
        /// </summary>
        public const string RE = "Error";

        #endregion

        #endregion

        #region Properties

        private List<Problem> Buttons { get => submitted ? results : problems; }
        
        private Config Settings { get; } = new Config("C-4.config");

        #region Fake Constants

        /// <summary>
        /// The problem directory location
        /// </summary>
        string PROB_DIR { get; }

        /// <summary>
        /// The temporary directory location
        /// </summary>
        string TEMP_DIR { get; }

        /// <summary>
        /// The temporary image directory location
        /// </summary>
        string IMG_DIR { get; }

        /// <summary>
        /// The temporary compilation directory location
        /// </summary>
        string COMP_DIR { get; }

        /// <summary>
        /// The temporary download directory location
        /// </summary>
        string LOAD_DIR { get; }

        Problem PROB_DEF { get; }

        #endregion

        #endregion

        public CFourm()
        {

            //this code loads assemblies that are embedded into the project
            //embedding the dll's allows the application to be shipped as a single file
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = GetType().Assembly.GetManifestResourceNames().First(element => element.EndsWith(resourceName));

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            //sets all the directory constants
            PROB_DIR = Path.Combine(Application.CommonAppDataPath, "PROB");

            TEMP_DIR = Path.Combine(Path.GetTempPath(), "C-4");
            if (Directory.Exists(TEMP_DIR))
            {
                try
                {
                    Directory.Delete(TEMP_DIR, true);
                }
                catch (IOException) { }
            }
            IMG_DIR = Path.Combine(TEMP_DIR, "IMG");
            COMP_DIR = Path.Combine(TEMP_DIR, "COMP");
            LOAD_DIR = Path.Combine(TEMP_DIR, "LOAD");

            //sets properties and events that can't be done in the designer
            DoubleBuffered = true;
            MouseWheel += CFourm_MouseWheel;

            InitializeComponent();

            webBrowser1.Navigate("about:blank");

            //renders the default problem and loads the problem list
            current = PROB_DEF = new Problem() { Text = "No problem loaded" };
            RenderCurrentProblem();
            LoadProblems();
        }

        Problem current;
        private void button1_Click(object sender, EventArgs e)
        {
            //prompt the user with a dialog to upload a script
            if (!backgroundWorker2.IsBusy)
            {
                FileDialog f = new OpenFileDialog() { Filter = "Java scripts|*.java" };
                if (f.ShowDialog() == DialogResult.OK)
                {
                    backgroundWorker2.RunWorkerAsync(f.FileName);
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorker2_Progress(0, "LOADING COMPILER", e))
            {
                return;
            }

            Java javac = Settings["java"] != null ? Java.GetJava(Settings["java"], Java.JavaFlags.Development) : Java.SearchJava(Java.JavaFlags.Development, Java.JavaFlags.Development); //loads the current java development version
            if (javac.error != null) //if there's an error loading, display and abort
            {
                MessageBox.Show(javac.error, "Error Locating Compiler!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {

                Directory.CreateDirectory(COMP_DIR); //this all takes place in a temporary directory

                //set up the process for compiling the given file
                ProcessStartInfo ps = new ProcessStartInfo(javac.exe, $"-d {TEMP_DIR} \"{e.Argument}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                string error = "";
                try
                {
                    //read the error output of the compilaton
                    using (OutputReader o = new OutputReader(Process.Start(ps)))
                    {
                        //start error reading
                        o.BeginError();
                        for (int i = 0; i < 9; i++)
                        {
                            if (backgroundWorker2_Progress(1, "COMPILING", e))
                            {
                                return;
                            }
                            if (o.Wait(TIMEOUT / 10))
                            {
                                break;
                            }
                        }
                        if (o.Kill(TIMEOUT / 10)) //if compilation lasts too long, end it and throw an error
                        {
                            error = o.Error;
                        }
                        else
                        {
                            error = "Compilation time limit exceeded.";
                        }
                    }
                }
                catch (Win32Exception) //if the process fails to run, throw an error and abort
                {
                    error = "Something went wrong while trying to compile your script. Try re-installing your Java Development Kit.";
                }

                if (error != "")
                {
                    if (backgroundWorker2_Progress(-1, "ERROR!", e))
                    {
                        return;
                    }
                    MessageBox.Show(error, "Compilation Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (backgroundWorker2_Progress(2, "LOADING TESTER", e))
                    {
                        return;
                    }

                    Java java = Settings["java"] != null ? Java.GetJava(Settings["java"], Java.JavaFlags.Default) : Java.SearchJava(Java.JavaFlags.Default, Java.JavaFlags.Development); //loads the current java development version                                                                             //loads the current java runtime version
                    if (java.error != null) //if there's an error loading, display and abort
                    {
                        MessageBox.Show(java.error, "Error Locating Tester!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        //sets up the process for executing the given file
                        ps = new ProcessStartInfo(java.exe, $"\"{Path.GetFileNameWithoutExtension((string)e.Argument)}\"")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = TEMP_DIR,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = true
                        };

                        if (backgroundWorker2_Progress(96, "RUNNING TESTS", e))
                        {
                            return;
                        }

                        try
                        {
                            int num = 0; //the test number

                            foreach (Problem.Test test in current?.Tests ?? new Problem.Test[0])
                            {
                                //read input, output, and test number
                                string input = test.input;
                                string output = test.output;
                                num++;

                                //read the output of the program
                                using (OutputReader o = new OutputReader(Process.Start(ps)))
                                {
                                    if (backgroundWorker2_Progress(100, $"RUNNING TEST #{num}", e))
                                    {
                                        return;
                                    }
                                    Problem result = new Problem();
                                    result.Name = $"Executing Test {num}...";
                                    result.Text = "This test has not yet finished executing. Click back later to see the results.";
                                    results.Add(result);
                                    Invoke((EventHandler)CFourm_Resize);

                                    //begin reading output and error
                                    o.BeginOutput();
                                    o.BeginError();

                                    //write the test case input
                                    o.Process.StandardInput.Write(input);
                                    o.Process.StandardInput.Close();

                                    for (int i = 0; i < 9; i++)
                                    {
                                        if (backgroundWorker2_Progress(100, $"RUNNING TEST #{num}", e))
                                        {
                                            return;
                                        }
                                        if (o.Wait(TIMEOUT / 10))
                                        {
                                            break;
                                        }
                                    }

                                    //build the results string based on the result
                                    string verdict;
                                    if (o.Kill(TIMEOUT / 10)) //if the program doesn't terminate naturally, it exceeded the time limit
                                    {
                                        if (backgroundWorker2_Progress(100, $"RUNNING TEST #{num}", e))
                                        {
                                            return;
                                        }
                                        //if the answer is correct, return accepted
                                        //if not, check the exit code to see if it's a runtime error or simply wrong
                                        verdict = ValidateOutput(o.Output, output) ? AC : (o.ExitCode == 0 ? WA : RE);
                                    }
                                    else
                                    {
                                        verdict = TLE;
                                    }
                                    result.Name = $"Test {num}: {verdict}";
                                    result.Text = $@"Your program {(verdict == TLE ? "was terminated after" : "completed in")} **{o.Time.TotalSeconds:F3}** seconds and exited with code **{o.ExitCode}**.
The received verdict was **{verdict}**.
***";
                                    if (test.visible)
                                    {
                                        result.Text += $@"
### TEST CASE 
{Codify(input)}
***
### CORRECT ANSWER
{Codify(output)}
***
### YOUR OUTPUT
{(o.Output.Length > 0 ? Codify(o.Output) : "Your program produced no output.")}
{(o.Error.Length > 0 ? $@"***
### ERROR
{Codify(o.Error)}" : "")}";
                                    }
                                    else
                                    {
                                        result.Text += $@"
The contents of this test are hidden.";
                                    }
                                }
                            }
                        }
                        catch (Win32Exception)
                        {
                            MessageBox.Show("Something went wrong while trying to run your script. Try re-installing your Java Runtime Environment.", "Error Running Tester!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private bool backgroundWorker2_Progress(int progress, string state, DoWorkEventArgs e)
        {
            if (backgroundWorker2.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                backgroundWorker2.ReportProgress(progress, state);
            }
            return e.Cancel;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!backgroundWorker2.CancellationPending)
            {
                button1.Enabled = false;
                button1.BackColor = Color.FromArgb(6, 98, 2);
                button1.Cursor = Cursors.No;
                button1.Text = (string)e.UserState;
                if (e.ProgressPercentage == 96)
                {
                    bool render = submitted && selected != 0;
                    submitted = true;
                    button8.Text = "PROBLEM LIST";
                    selected = 0;
                    highlighted = -1;
                    click = false;
                    results.Clear();
                    results.Add(current);
                    if(render)
                    {
                        RenderCurrentProblem();
                    }
                }
            }
        }

        private void backgroundWorker2_Cancel()
        {
            if (backgroundWorker2.IsBusy)
            {
                backgroundWorker2.CancelAsync();
                backgroundWorker2_RunWorkerCompleted(null, null);
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e == null || !e.Cancelled)
            {
                CFourm_Resize(null, null);
                button1.Enabled = current.Tests != null && current.Tests.Length > 0;
                button1.BackColor = current.Tests != null && current.Tests.Length > 0 ? Color.FromArgb(12, 196, 4) : Color.FromArgb(6, 98, 2);
                button1.Cursor = current.Tests != null && current.Tests.Length > 0 ? Cursors.Default : Cursors.No;
                button1.Text = "UPLOAD CODE FILE";
                if (Directory.Exists(COMP_DIR)) //if the temporary directory exists, delete it
                {
                    Directory.Delete(COMP_DIR, true);
                }
            }
        }

        public string Codify(string str)
        {
            if (str.Length > 1000)
            {
                str = str.Substring(0, 1000) + "... (truncated)";
            }
            string fence = "```";
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '`')
                {
                    if (++count > fence.Length - 1)
                    {
                        fence += '`';
                    }
                }
                else
                {
                    count = 0;
                }
            }
            return $@"{fence} {{style=""width:100%;overflow-x:auto""}}
{str}
{fence}";
        }

        /// <summary>
        /// Checks to see if a program's output is correct
        /// </summary>
        /// <param name="answer">The correct output</param>
        /// <param name="output">The output given by the program</param>
        /// <returns></returns>
        public bool ValidateOutput(string answer, string output)
        {
            //splits both of the answers into lines
            string[] answerLines = SplitLines(answer);
            string[] outputLines = SplitLines(output);

            //if lengths and contents match, the output is correct
            return answerLines.Length == outputLines.Length && answerLines.SequenceEqual(outputLines);
        }

        /// <summary>
        /// Splits a string into the content on its lines
        /// </summary>
        /// <param name="str">The string to split</param>
        /// <returns>A string split by the new line and carriage return characters, with empty entries removed</returns>
        public string[] SplitLines(string str)
        {
            return str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        byte[] Checksum(Stream stream, long start, long end)
        {
            end = Math.Min(stream.Length, end);
            byte[] bytes = new byte[12] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            byte[] nbytes = new byte[12];
            stream.Position = start;

            int n = stream.Read(nbytes, 0, Math.Min(12, (int)Math.Min(int.MaxValue, end - stream.Position)));
            while (n > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    bytes[i] ^= nbytes[i];
                }
                byte temp = (byte)((bytes[11] >> 7) & 1);
                for (int i = 11; i > 0; i--)
                {
                    bytes[i] <<= 1;
                    bytes[i] |= (byte)((bytes[i - 1] >> 7) & 1);
                }
                bytes[0] <<= 1;
                bytes[0] |= temp;
                n = stream.Read(nbytes, 0, Math.Min(12, (int)Math.Min(int.MaxValue, end - stream.Position)));
            }

            return bytes;
        }

        bool VerifyChecksum(Stream stream, byte[] checksum)
        {
            try
            {
                stream.Position = stream.Length - 12;
                byte[] c = Checksum(stream, 0, stream.Length - 12);
                byte[] b = new byte[12];
                stream.Read(b, 0, 12);
                if (c.Length == b.Length && c.Length == checksum.Length && c.SequenceEqual(b) && c.SequenceEqual(checksum))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }

        bool tryProblem(Stream stream, out Problem problem)
        {
            problem = null;
            using (MemoryStream copy = new MemoryStream())
            {
                stream.Position = 0;
                stream.CopyTo(copy, (int)stream.Length - 12);
                copy.Position = 0;
                using (GZipStream deflate = new GZipStream(copy, CompressionMode.Decompress))
                {
                    using (StreamReader sr = new StreamReader(deflate, Encoding.UTF8))
                    {
                        string json;
                        try
                        {
                            json = sr.ReadToEnd();
                            problem = JsonConvert.DeserializeObject<Problem>(json);
                        }
                        catch (InvalidDataException e)
                        {
                            return false;
                        }
                        catch (JsonReaderException e)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
        }

        string LoadImage(Problem problem, string name)
        {
            if (!problem.Images.ContainsKey(name))
            {
                return null;
            }
            string folder = Path.Combine(IMG_DIR, $"{problem.Name}_{problem.GetHashCode()}");
            string filename = Path.Combine(folder, name);
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(filename, problem.Images[name]);
            filename = Uri.EscapeUriString("file:///" + filename);
            unsanitize.Add(filename);
            return $"![{name}]({filename})";
        }

        void RenderCurrentProblem()
        {
            Problem problem = 0 <= selected && selected < Buttons.Count ? Buttons[selected] : PROB_DEF;
            const string HTML_HEAD = @"
<!DOCTYPE html>
<head>
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge""/> 
<style>

body {
    font-family: Arial, sans-serif;
}

abbr {
    text-decoration: underline;
}

table {
    border-collapse: collapse;
}

table, th, td, figure {
    border: 1px solid black;
}

figure > figcaption {
    border-top: 1px solid black
}

code {
    display: inline-block;
    background-color: #c4c4c4;
}

blockquote {
    color: #4c4c4c;
}

dd {
    font-style: italic;
}

</style>
</head>
<html>
<body>
";
            const string HTML_TAIL = @"
</body>
</html>";
            string body = problem.Text ?? "";

            unsanitize.Clear();
            if (problem.Images != null)
            {
                foreach (string i in problem.Images.Keys)
                {
                    body = body.Replace($"{{{{{i}}}}}", LoadImage(problem, i));
                }
            }
            HtmlSanitizer sanitizer = new HtmlSanitizer();
            sanitizer.FilterUrl += Sanitizer_FilterUrl;
            sanitizer.AllowedAttributes.Add("id");
            body = sanitizer.Sanitize(Markdown.ToHtml(body, new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().DisableHtml().Build()));

            navigate = true;
            webBrowser1.Document.OpenNew(true);
            webBrowser1.Document.Write(HTML_HEAD + body + HTML_TAIL);
            if (!submitted)
            {
                Text = string.IsNullOrWhiteSpace(problem.Name) ? "C-4" : $"C-4 ({problem.Name})";
                button1.Enabled = problem.Tests != null && problem.Tests.Length > 0;
                button1.BackColor = problem.Tests != null && problem.Tests.Length > 0 ? Color.FromArgb(12, 196, 4) : Color.FromArgb(6, 98, 2);
                button1.Cursor = problem.Tests != null && problem.Tests.Length > 0 ? Cursors.Default : Cursors.No;
            }
            webBrowser1.Refresh();
            Invalidate();
        }

        List<string> unsanitize = new List<string>();
        private void Sanitizer_FilterUrl(object sender, FilterUrlEventArgs e)
        {
            if (unsanitize.Contains(e.OriginalUrl))
            {
                e.SanitizedUrl = e.OriginalUrl;
            }
        }

        bool navigate = true;
        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (navigate || (e.Url.Scheme == "about" && e.Url.ToString() != "about:blank"))
            {
                navigate = false;
            }
            else
            {
                e.Cancel = true;
                if (e.Url.Scheme != "about")
                {
                    Process.Start(e.Url.ToString());
                }
            }
        }

        bool submitted = false;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Brush b = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(Text, Font, b, new RectangleF(0, 0, Width, 29), new StringFormat() { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap });
            }
            e.Graphics.FillRectangle(brushes["box"], box);

            for (int i = 0; i < Buttons.Count; i++)
            {
                Problem problem = Buttons[i];
                Rectangle rect = new Rectangle(24, 29 + 25 * i - vScrollBar1.Value, 130, 25);
                bool clicked = i == highlighted && click && box.Contains(PointToClient(Cursor.Position)) && rect.Contains(PointToClient(Cursor.Position));

                e.Graphics.SetClip(box);
                e.Graphics.FillRectangle(brushes[i == selected ? "select" : clicked ? "click" : "problem" + (i % 2 + 1)], rect);
                if (i == highlighted)
                {
                    e.Graphics.DrawRectangle(pens["hover"], rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    e.Graphics.DrawRectangle(pens["hover"], rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
                }
                e.Graphics.DrawString(problem.Name, btnFont, brushes[i == selected ? "selectText" : clicked ? "clickText" : "text"], rect, new StringFormat(StringFormatFlags.NoWrap) { LineAlignment = StringAlignment.Center });
                e.Graphics.ResetClip();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_NCHITTEST = 132, WM_SYSCOMMAND = 274;
            const int HTCAPTION = 2, HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTTOPLEFT = 13, HTTOPRIGHT = 14, HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;
            const int SC_MASK = 0xFFF0, SC_MAXIMIZE = 0xF030, SC_RESTORE = 0xF120, SC_MINIMIZE = 0xF020;
            const int SIZE = 4;
            const int TOP = 32;

            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = PointToClient(pos);

                if (WindowState == FormWindowState.Maximized)
                {
                    if (pos.Y <= TOP)
                    {
                        m.Result = (IntPtr)HTCAPTION;
                    }
                }
                else
                {
                    if (pos.X <= SIZE)
                    {
                        if (pos.Y <= SIZE)
                            m.Result = (IntPtr)HTTOPLEFT;
                        else if (pos.Y >= ClientSize.Height - SIZE)
                            m.Result = (IntPtr)HTBOTTOMLEFT;
                        else
                            m.Result = (IntPtr)HTLEFT;
                    }
                    else if (pos.X >= ClientSize.Width - SIZE)
                    {
                        if (pos.Y <= SIZE)
                            m.Result = (IntPtr)HTTOPRIGHT;
                        else if (pos.Y >= ClientSize.Height - SIZE)
                            m.Result = (IntPtr)HTBOTTOMRIGHT;
                        else
                            m.Result = (IntPtr)HTRIGHT;
                    }
                    else if (pos.Y <= SIZE)
                    {
                        m.Result = (IntPtr)HTTOP;
                    }
                    else if (pos.Y >= ClientSize.Height - SIZE)
                    {
                        m.Result = (IntPtr)HTBOTTOM;
                    }
                    else if (pos.Y <= TOP)
                    {
                        m.Result = (IntPtr)HTCAPTION;
                    }
                }
            }
            else if (m.Msg == WM_SYSCOMMAND)
            {
                int wParam = m.WParam.ToInt32() & SC_MASK;
                if (wParam == SC_MAXIMIZE || wParam == SC_RESTORE || wParam == SC_MINIMIZE)
                {
                    OnResize(null);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            OnResize(null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        List<Problem> problems = new List<Problem>();
        List<Problem> results = new List<Problem>();
        private void LoadProblems()
        {
            if (!backgroundWorker1.IsBusy)
            {
                highlighted = selected = -1;
                click = false;
                CFourm_Resize(null, null);
                current = PROB_DEF;
                RenderCurrentProblem();
                vScrollBar1.Value = vScrollBar1.Minimum;
                button8.Enabled = false;
                button8.BackColor = Color.FromArgb(102, 102, 0);
                button8.Cursor = Cursors.No;
                problems.Clear();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        HttpClient client = new HttpClient() { BaseAddress = new Uri("https://carver-coding-club-computer.cf") };
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Directory.Exists(IMG_DIR))
            {
                Directory.Delete(IMG_DIR, true);
            }
            Dictionary<string, byte[]> d;
            try
            {
                d = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(client.PostAsync("problems", null).Result.Content.ReadAsStringAsync().Result);
            }
            catch (AggregateException)
            {
                MessageBox.Show("A connection could not be established to the remote server. Only previously downloaded files will be available.", "Error Downloading Files!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                foreach (string filename in Directory.EnumerateFiles(PROB_DIR))
                {
                    if (File.Exists(filename))
                    {
                        using (FileStream file = File.OpenRead(filename))
                        {
                            if (VerifyChecksum(file, Checksum(file, 0, file.Length - 12)) && tryProblem(file, out Problem problem))
                            {
                                problems.Add(problem);
                                Invoke((EventHandler)CFourm_Resize);
                            }
                        }
                    }
                }
                return;
            }
            foreach (string name in d.Keys)
            {
                string filename = Path.Combine(PROB_DIR, name);
                string loadname = Path.Combine(LOAD_DIR, name);
                bool download = true;
                if (File.Exists(filename))
                {
                    using (FileStream file = File.OpenRead(filename))
                    {
                        if (VerifyChecksum(file, d[name]))
                        {
                            download = false;
                        }
                    }
                }
                if (download)
                {
                    Directory.CreateDirectory(LOAD_DIR);
                    try
                    {
                        File.WriteAllBytes(loadname, client.GetByteArrayAsync("problems/" + name).Result);
                    }
                    catch (HttpRequestException)
                    {
                        MessageBox.Show("A connection could not be established to the remote server. Only previously downloaded files will be available.", "Error Downloading Files!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    Directory.CreateDirectory(PROB_DIR);
                    File.Move(loadname, filename);
                }
                using (FileStream file = File.OpenRead(filename))
                {
                    if (tryProblem(file, out Problem problem))
                    {
                        problems.Add(problem);
                        Invoke((EventHandler)CFourm_Resize);
                    }
                }
            }
            foreach (string name in Directory.EnumerateFiles(PROB_DIR))
            {
                if (!d.ContainsKey(Path.GetFileName(name)) && File.Exists(name))
                {
                    File.Delete(name);
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button8.Enabled = true;
            button8.BackColor = Color.FromArgb(204, 204, 0);
            button8.Cursor = Cursors.Default;
            button8.Text = "RELOAD PROBLEMS";
        }

        int scrollAmount;
        private void CFourm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (box.Contains(e.Location))
            {
                scrollAmount += e.Delta;
                vScrollBar1.Value = Math.Max(vScrollBar1.Minimum, Math.Min(vScrollBar1.Value - (SCROLL_SPEED * SystemInformation.MouseWheelScrollLines * scrollAmount / 120), vScrollBar1.Maximum - vScrollBar1.LargeChange + 1));
                scrollAmount %= 120 / SystemInformation.MouseWheelScrollLines / SCROLL_SPEED;
                Invalidate();
            }
        }

        int highlighted = -1;
        int selected = -1;
        bool click = false;
        private void CFourm_MouseOver(object sender, EventArgs e)
        {
            if (click)
            {
                Invalidate();
                return;
            }
            Point pointer = PointToClient(Cursor.Position);
            highlighted = -1;
            if (box.Contains(pointer))
            {
                Invalidate();
                for (int i = 0; i < Buttons.Count; i++)
                {
                    Rectangle rect = new Rectangle(24, 29 + 25 * i - vScrollBar1.Value, 130, 25);

                    if (rect.Contains(pointer))
                    {
                        highlighted = i == selected ? -1 : i;
                        break;
                    }
                }
            }
        }

        private void CFourm_MouseOver(object sender, MouseEventArgs e)
        {
            CFourm_MouseOver(null, EventArgs.Empty);
        }

        private void CFourm_MouseDown(object sender, MouseEventArgs e)
        {
            if (highlighted > -1 && box.Contains(e.Location) && new Rectangle(24, 29 + 25 * highlighted - vScrollBar1.Value, 130, 25).Contains(e.Location))
            {
                click = true;
                Invalidate();
            }
        }

        private void CFourm_MouseUp(object sender, MouseEventArgs e)
        {
            if (highlighted > -1 && click && box.Contains(e.Location) && new Rectangle(24, 29 + 25 * highlighted - vScrollBar1.Value, 130, 25).Contains(e.Location))
            {
                selected = highlighted;
                highlighted = -1;
                if (!submitted)
                {
                    current = Buttons[selected];
                    backgroundWorker2_Cancel();
                }
                RenderCurrentProblem();
            }
            click = false;
            Invalidate();
        }

        private void CFourm_MouseCaptureChanged(object sender, EventArgs e)
        {
            highlighted = -1;
            click = false;
            Invalidate();
        }

        Rectangle box;
        Dictionary<string, Brush> brushes = new Dictionary<string, Brush>
        {
            {"text", new SolidBrush(Color.Black) },
            {"box",  new SolidBrush(Color.FromArgb(228, 228, 228))},
            {"problem1", new SolidBrush(Color.FromArgb(196, 196, 196)) },
            {"problem2", new SolidBrush(Color.FromArgb(164, 164, 164)) },
            {"click", new SolidBrush(Color.FromArgb(196, 196, 255)) },
            {"clickText", new SolidBrush(Color.FromArgb(76, 76, 196)) },
            {"select", new SolidBrush(Color.FromArgb(196, 255, 196)) },
            {"selectText", new SolidBrush(Color.FromArgb(76, 196, 76)) }
        };
        Dictionary<string, Pen> pens = new Dictionary<string, Pen>
        {
            {"hover", new Pen(Color.FromArgb(12, 64, 196)) },
        };
        Font btnFont = new Font("Arial Narrow", 9.75f);

        private void button8_Click(object sender, EventArgs e)
        {
            if (!submitted)
            {
                LoadProblems();
            }
            else
            {
                submitted = false;
                selected = Buttons.IndexOf(current);
                button8.Text = "RELOAD PROBLEMS";
                CFourm_Resize(null, null);
                vScrollBar1.Value = Math.Max(vScrollBar1.Minimum, Math.Min(25 * selected, vScrollBar1.Maximum - vScrollBar1.LargeChange + 1));
                RenderCurrentProblem();
                Refresh();
            }
            backgroundWorker2_Cancel();
        }

        public const int SCROLL_SPEED = 2;
        private void CFourm_Resize(object sender, EventArgs e)
        {
            vScrollBar1.SmallChange = SystemInformation.MouseWheelScrollLines * SCROLL_SPEED;
            vScrollBar1.LargeChange = 100;
            vScrollBar1.Maximum = Math.Max(0, 25 * Buttons.Count - vScrollBar1.Height) + vScrollBar1.LargeChange - 1;
            vScrollBar1.Value = Math.Max(vScrollBar1.Minimum, Math.Min(vScrollBar1.Value, vScrollBar1.Maximum - vScrollBar1.LargeChange + 1));
            vScrollBar1.Enabled = vScrollBar1.Maximum > vScrollBar1.LargeChange - 1;
            vScrollBar1.Height = ClientSize.Height - 83;
            box = new Rectangle(4, 29, 150, vScrollBar1.Height);
            Invalidate();
        }
    }
}
