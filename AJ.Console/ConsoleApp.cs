// Source: https://github.com/ajdotnet/AJ.Console
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;

namespace AJ.Console
{
    /// <summary>
    ///   <para>
    /// Baseclass for console applications.
    /// Supports the following features:
    ///   </para>
    ///   <list type="bullet">
    ///   <item><term>Clean and easy handling of arguments (parameters, switches)</term></item>
    ///   <item><term>Errorhandling</term></item>
    ///   <item><term>Standard parameter (help, etc.)</term></item>
    ///   <item><term>Output control (Verbose)</term></item>
    ///   <item><term>Output of logo, syntax, etc.</term></item>
    ///   <item><term>parameter file support</term></item>
    ///   <item><term>log file support</term></item>
    ///   <item><term>colored output (via p/invoke)</term></item>
    ///   <item><term>...</term></item>
    ///   </list>
    ///   <para>
    /// Usage: Derive a class and overwrite the abstract methods
    ///   <see cref="ApplyArguments" />,
    ///   <see cref="ApplySwitch" />, and
    ///   <see cref="Process" />.
    /// Add a *.resx-file (name=name of your derived class
    /// containing messages (Help, Logo etc.)
    ///   </para>
    ///   <para>
    /// The applications main-method should look like this:
    ///   </para>
    ///   <code>
    /// [STAThread]
    /// static int Main(string[] args)
    /// {
    /// MyApp app= new MyApp();
    /// return app.Run(args);
    /// }
    ///   </code>
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class ConsoleApp
    {
        ///////////////////////////////////////////////////////////////////////////////
        #region interface

        /// <summary>
        /// This method will be called with the application arguments,
        /// i.e. the parameters preceeding switches. e.g.
        /// <c>cl.exe file1.txt file2.txt /p a b /c</c>
        /// will call this method with {"file1.txt", "file2.txt"}
        /// </summary>
        /// <param name="values">The values.</param>
        protected abstract void ApplyArguments(string[] values);

        /// <summary>
        ///   <para>
        /// This method will be called for every switch, i.e. parameters beginning with a slash.
        ///   </para>
        ///   <para>
        /// Parameters following a switch will be treated as arguments to that switch.
        /// e.g.: <c>cl.exe file1.txt file2.txt /p a b /c</c>
        /// will call this method with
        ///   <c>"/p", {"a", "b"}</c>
        /// and again with
        ///   <c>"/c", {}</c>.
        ///   </para>
        /// </summary>
        /// <param name="name">name of the switch (including the slash</param>
        /// <param name="values">arguments of the switch</param>
        protected abstract void ApplySwitch(string name, string[] values);

        /// <summary>
        /// Called to start the actuall processing. This method is only
        /// called if no help has been requested and no error was raised during
        /// the argument parsing phase
        /// </summary>
        protected abstract void Process();

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region singleton implementation

        static ConsoleApp _current = null;

        /// <summary>
        /// c'tor checks if this class is the first (and thus only) singleton
        /// </summary>
        protected ConsoleApp()
        {
            Debug.Assert(_current == null);
            _current = this;

            this.CurrentShowLevel = ShowLevel.Normal;
        }

        /// <summary>
        ///  Singleton instance
        /// </summary>
        static public ConsoleApp Current
        {
            get { return _current; }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region properties & members

        Parameter _arguments = null;
        List<Parameter> _switches = null;
        ResourceManager _rmThis = null;
        ResourceManager _rmFallback = null;
        bool _helpWanted = false;
        bool _logoHasBeenShown = false;
        bool _showLogo = true;
        string _logFileName = null;
        StreamWriter _logFileStream = null;

        /// <value>
        /// The current show level.
        /// </value>
        public ShowLevel CurrentShowLevel { get; set; }

        /// <value>
        /// The application return value.
        /// </value>
        public byte ReturnValue { get; set; }

        /// <value>
        /// Gets the 'Syntax' message from the resources
        /// </value>
        public string SyntaxMessage { get { return GetString(ResKey.Syntax); } }
        /// <value>
        /// gets the 'System' parameters (part of the syntax) from the resources
        /// </value>
        public string SystemParametersMessage { get { return GetString(ResKey.SystemParameters); } }
        /// <value>
        /// gets the 'Logo' message from the resources
        /// </value>
        public string LogoMessage { get { return GetString(ResKey.Logo); } }
        /// <value>
        /// gets the 'Error' message from the resources (default feedback, will be complemented with exception message).
        /// </value>
        public string ErrorMessage { get { return GetString(ResKey.Error); } }
        /// <value>
        /// gets the 'Help' message from the resources
        /// </value>
        public string HelpMessage { get { return GetString(ResKey.Help); } }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region messages

        /// <summary>
        /// print Logo-Text (once)
        /// </summary>
        protected virtual void PrintLogoText()
        {
            if (_logoHasBeenShown)
                return;
            _logoHasBeenShown = true;
            if (_showLogo)
                this.WriteLine(ShowLevel.Important, LogoMessage);
        }

        /// <summary>
        /// print syntax (including system parameters)
        /// </summary>
        protected virtual void PrintSyntaxText()
        {
            this.WriteLine(ShowLevel.Normal, SyntaxMessage);
            this.WriteLine(ShowLevel.Normal, SystemParametersMessage);
        }

        /// <summary>
        /// print logo and help, syntax, and help text
        /// </summary>
        protected virtual void PrintHelpText()
        {
            PrintLogoText();
            PrintSyntaxText();
            WriteLine(ShowLevel.Normal, HelpMessage);
        }

        /// <summary>
        /// helper to get a ressource string
        /// including fallback
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected string GetString(string name)
        {
            try
            {
                string ret = _rmThis.GetString(name);
                if (ret != null)
                    return ret;
            }
            catch (MissingManifestResourceException)
            {
            }
            return _rmFallback.GetString(name);
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region parameter parsing

        /// <summary>
        /// parse command line parsen
        /// </summary>
        /// <param name="args">actual command line arguments</param>
        void ParseCommandLine(string[] args)
        {
            _switches = new List<Parameter>();
            var currentParam = new Parameter();

            for (int i = 0; i < args.Length; ++i)
                ParseParam(args[i], ref currentParam);
            AddParsedParam(ref currentParam);
        }

        /// <summary>
        /// evaluates the single parameter (including @parameterfile and !logfile)
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="currentParam">The current parameter.</param>
        void ParseParam(string arg, ref Parameter currentParam)
        {
            arg = arg.Trim();
            if (arg.Length == 0)
            {
                // might happen in parameter files
            }
            else if (arg[0] == '@')
            {
                // parameter file support: xy.exe @params.txt
                InsertParamsFromFile(arg.Substring(1));
            }
            else if (arg[0] == '!')
            {
                // logfile name xy.exe !logfile.txt
                _logFileName = arg.Substring(1);
            }
            if (arg[0] != '/')
            {
                // next argument
                currentParam.Values.Add(arg);
            }
            else
            {
                // next switch 
                AddParsedParam(ref currentParam);
                currentParam = new Parameter();
                currentParam.Name = arg;
            }
        }

        /// <summary>
        /// take last arguments or switch
        /// </summary>
        void AddParsedParam(ref Parameter currentParam)
        {
            if (currentParam == null)
                return;

            if (string.IsNullOrEmpty(currentParam.Name))
            {
                System.Diagnostics.Debug.Assert(_arguments == null);
                _arguments = currentParam;
            }
            else
            {
                _switches.Add(currentParam);
            }
            currentParam = null;
        }

        /// <summary>
        /// read textfile lines as arguments
        /// </summary>
        /// <param name="paramfile">The paramfile.</param>
        /// <exception cref="AJ.Console.ConsoleException"></exception>
        void InsertParamsFromFile(string paramfile)
        {
            try
            {
                var currentParam = new Parameter();
                var contents = File.ReadAllLines(paramfile);
                foreach (var line in contents)
                    ParseParam(line, ref currentParam);
                AddParsedParam(ref currentParam);
            }
            catch (Exception ex)
            {
                throw new ConsoleException(string.Format(CultureInfo.CurrentCulture, GetString(ResEx.CouldNotReadParameterFile), paramfile), ex);
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region argument and switch handling

        /// <summary>
        /// get all arguments
        /// </summary>
        /// <returns>
        /// all arguments
        /// </returns>
        public string[] GetArguments()
        {
            if (_arguments == null)
                return new string[] { };
            return _arguments.Values.ToArray();
        }

        /// <summary>
        /// get switch by name: "/d file1 file2" returns {"file1", "file2"} for switch "/d"
        /// </summary>
        /// <param name="name">switch name</param>
        /// <returns>
        /// arguments for the switch
        /// </returns>
        public string[] GetSwitch(string name)
        {
            var sw = _switches.Where(s => s.Name == name).FirstOrDefault();
            if (sw == null)
                return new string[] { };
            return sw.Values.ToArray();
        }

        /// <summary>
        /// check if switch has been provided (for boolean switches)
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it ignores the case.</param>
        /// <returns></returns>
        public bool HasSwitch(string name, bool ignoreCase = true)
        {
            return _switches
                .Where(s => string.Compare(s.Name, name, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) == 0)
                .Any();
        }

        /// <summary>
        /// apply all switches
        /// </summary>
        protected virtual void ApplySwitches()
        {
            foreach (var p in _switches)
            {
                if (!p.IsApplied)
                    ApplySwitch(p.Name, p.Values.ToArray());
                p.IsApplied = true;
            }
        }

        /// <summary>
        /// handle special switches (help, output control, ...)
        /// </summary>
        protected virtual void ApplySpecialSwitches()
        {
            foreach (var p in _switches)
            {
                if (!p.IsApplied && ApplySpecialSwitch(p.Name, p.Values.ToArray()))
                    p.IsApplied = true;
            }
        }

        /// <summary>
        /// handle special switches (help, output control, ...)
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        protected virtual bool ApplySpecialSwitch(string name, string[] values)
        {
            switch (name)
            {
                case "/?":
                case "/h":
                case "/help": _helpWanted = true; return true;

                case "/v":
                case "/verbose": this.CurrentShowLevel = ShowLevel.Verbose; return true;

                case "/q":
                case "/quiet": this.CurrentShowLevel = ShowLevel.Warning; return true;

                case "/nologo": _showLogo = false; return true;
            }

            return false;
        }

        /// <summary>
        /// Helper: checks if the array length is in a given range,
        /// throws an exception if not.
        /// to be used in <see cref="ApplyArguments" /> and <see cref="ApplySwitch" />.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <exception cref="AJ.Console.ConsoleException">
        /// </exception>
        public void EnsureLength(string[] test, int min, int max, string name)
        {
            if (name == null)
                name = "arguments";
            if (test == null)
                throw new ConsoleException(GetString(ResEx.InvalidArgumentsNull), name);
            if (test.Length < min)
                throw new ConsoleException(GetString(ResEx.InvalidArgumentsMin), name, min);
            if (test.Length > max)
                throw new ConsoleException(GetString(ResEx.InvalidArgumentsMax), name, max);
        }

        /// <summary>
        /// Helper: throws an exception for an unknown switch
        /// to be used in <see cref="ApplySwitch" />.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="AJ.Console.ConsoleException"></exception>
        public void ThrowUnknownSwitch(string name)
        {
            throw new ConsoleException(GetString(ResEx.UnknownSwitch), name);
        }

        /// <summary>
        /// Helper: throws an exception for invalid arguments
        /// to be used in <see cref="ApplyArguments" /> .
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="AJ.Console.ConsoleException"></exception>
        public void ThrowInvalidArguments(string message)
        {
            throw new ConsoleException(GetString(ResEx.InvalidArguments), message);
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region running

        /// <summary>
        /// Helper: call run with command line arguments.
        /// </summary>
        /// <returns></returns>
        protected int Run()
        {
            return Run(Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// application workflow:
        /// <list type="bullet">
        /// <item><term><see cref="Init" />: override for initialization</term></item>
        /// <item><term><see cref="ParseCommandLine" />: command line parsing (internal)</term></item>
        /// <item><term><see cref="OpenLogFile" />: open log file (internal)</term></item>
        /// <item><term><see cref="ApplySpecialSwitches" />: handle special switches (override if necessary)</term></item>
        /// <item><term><see cref="ApplyArguments" />: abstract</term></item>
        /// <item><term><see cref="ApplySwitches" />: abstract</term></item>
        /// <item><term><see cref="Process" />: abstract</term></item>
        /// <item><term><see cref="CleanUp" />: override for cleanup</term></item>
        /// </list>
        /// In case orf exceptions <see cref="ConsoleException" /> will be
        /// printed with message only, other exceptions including stacktrace.<br />
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "last chance exception handler")]
        protected virtual int Run(string[] args)
        {
            try
            {
                Init();
                ParseCommandLine(args);
                OpenLogFile();
                ApplySpecialSwitches();
                if (_helpWanted)
                {
                    PrintHelpText();
                    ReturnValue = SpecialReturnValue.HelpRequested;
                }
                else
                {
                    PrintLogoText();
                    ApplyArguments(GetArguments());
                    ApplySwitches();
                    Process();
                }
            }
            catch (ConsoleException ex)
            {
                PrintLogoText();
                PrintExceptionHierarchy(ex.InnerException);
                if (!string.IsNullOrEmpty(ex.Message))
                    WriteLine(ShowLevel.Error, ex.Message);
                WriteLine(ShowLevel.Warning, ErrorMessage);
                ReturnValue = SpecialReturnValue.HandledException;
            }
            catch (Exception ex)
            {
                PrintLogoText();
                WriteLine(ShowLevel.Error, "Exception: " + ex.ToString());
                ReturnValue = SpecialReturnValue.UnHandledException;
            }
            finally
            {
                CleanUp();
                Terminating();
            }

            return ReturnValue;
        }

        /// <summary>
        /// helper to print exceptions (including inner exceptions)
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void PrintExceptionHierarchy(Exception ex)
        {
            if (ex == null)
                return;
            if (ex.InnerException != null)
                PrintExceptionHierarchy(ex.InnerException);
            WriteLine(ShowLevel.Error, "Exception: " + ex.GetType().FullName);
            WriteLine(ShowLevel.Error, "  Message: " + ex.Message);
        }

        /// <summary>
        /// initialization
        /// </summary>
        /// <exception cref="System.Exception">resource file missing:  + this.GetType().FullName</exception>
        protected virtual void Init()
        {
            try
            {
                if (_rmThis == null)
                    _rmThis = new ResourceManager(this.GetType());
                if (_rmFallback == null)
                    _rmFallback = new ResourceManager(typeof(ConsoleApp));
                // make sure resources are available
                _rmThis.GetString("Logo");
                _rmFallback.GetString("Logo");
            }
            catch (Exception ex)
            {
                throw new InvalidProgramException("resource file missing: " + this.GetType().FullName, ex);
            }
        }

        /// <summary>
        /// cleanup
        /// </summary>
        protected virtual void CleanUp()
        {
            _rmThis = null;
            _rmFallback = null;
            CloseLogFile();
        }

        /// <summary>
        /// init logfile
        /// </summary>
        /// <exception cref="AJ.Console.ConsoleException"></exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        void OpenLogFile()
        {
            if (string.IsNullOrEmpty(_logFileName))
                return;

            try
            {
                Stream strm = new FileStream(_logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                _logFileStream = new StreamWriter(strm);
                _logFileStream.WriteLine("### LOG START : " + DateTime.Now.ToString() + " ###");
            }
            catch (Exception ex)
            {
                throw new ConsoleException(string.Format(CultureInfo.CurrentCulture, GetString(ResEx.LogfileCouldNotBeOpened), _logFileName), ex);
            }
        }

        /// <summary>
        /// close logfile
        /// </summary>
        void CloseLogFile()
        {
            if (_logFileStream == null)
                return;
            _logFileStream.WriteLine("### LOG END : " + DateTime.Now.ToString() + " ###");
            _logFileStream.Close();
        }

        /// <summary>
        /// if being debugged, wait before the window closes
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.Write(System.String)")]
        [Conditional("DEBUG")]
        void Terminating()
        {
            if (!System.Diagnostics.Debugger.IsAttached)
                return;
            System.Console.WriteLine();
            Win32.SetConsoleTextColor(Win32.StdHandle.Output, Color.Blue, Color.White);
            System.Console.Write("Terminated with return value " + ReturnValue.ToString(CultureInfo.InvariantCulture) + ". <Press Return>");
            Win32.SetConsoleTextColor(Win32.StdHandle.Output, Color.White, Color.Red);
            System.Console.Read();
            System.Console.Write("Terminating...");
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////
        #region output

        /// <summary>
        /// print normal line
        /// </summary>
        /// <param name="line">The line.</param>
        public void WriteLine(string line)
        {
            if (ShowLevel.Normal < this.CurrentShowLevel)
                return;
            ColorWriteLine(ShowLevel.Normal, line);
        }

        /// <summary>
        /// print line depending on level
        /// </summary>
        /// <param name="show">The show.</param>
        /// <param name="line">The line.</param>
        public void WriteLine(ShowLevel show, string line)
        {
            if (show < this.CurrentShowLevel)
                return;
            ColorWriteLine(show, line);
        }

        /// <summary>
        /// print line depending on level
        /// </summary>
        /// <param name="show">The show.</param>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        /// <param name="line">The line.</param>
        public void WriteLine(ShowLevel show, Color foreground, Color background, string line)
        {
            if (show < this.CurrentShowLevel)
                return;
            ColorWriteLine(show, foreground, background, line);
        }

        /// <summary>
        /// Colors the write line.
        /// </summary>
        /// <param name="show">The show.</param>
        /// <param name="foregroud">The foregroud.</param>
        /// <param name="background">The background.</param>
        /// <param name="line">The line.</param>
        void ColorWriteLine(ShowLevel show, Color foregroud, Color background, string line)
        {
            if (line == null)
                return;

            bool stderr = (show >= ShowLevel.Error);
            Win32.StdHandle h = stderr ? Win32.StdHandle.Error : Win32.StdHandle.Output;

            Color backgroundOld;
            Color foregroundOld;
            Win32.GetConsoleTextColor(h, out foregroundOld, out backgroundOld);

            Win32.SetConsoleTextColor(h, foregroud, background);
            MonoWriteLine(show, line);

            Win32.SetConsoleTextColor(h, foregroundOld, backgroundOld);
        }

        void ColorWriteLine(ShowLevel show, string line)
        {
            if (line == null)
                return;

            bool stderr = (show >= ShowLevel.Error);
            Win32.StdHandle h = stderr ? Win32.StdHandle.Error : Win32.StdHandle.Output;

            Color background;
            Color foreground;
            Win32.GetConsoleTextColor(h, out foreground, out background);

            switch (show)
            {
                case ShowLevel.Verbose: Win32.SetConsoleTextColor(h, Color.Gray, background); break;
                case ShowLevel.Normal: Win32.SetConsoleTextColor(h, Color.White, background); break;
                case ShowLevel.Important: Win32.SetConsoleTextColor(h, Color.Yellow, background); break;
                case ShowLevel.Warning: Win32.SetConsoleTextColor(h, Color.Teal, background); break;
                case ShowLevel.Error: Win32.SetConsoleTextColor(h, Color.Red, background); break;
            }

            MonoWriteLine(show, line);

            Win32.SetConsoleTextColor(h, foreground, background);
        }

        void MonoWriteLine(ShowLevel show, string line)
        {
            if (line == null)
                return;

            if (show >= ShowLevel.Error)
                System.Console.Error.WriteLine(line);
            else
                System.Console.Out.WriteLine(line);

            if (_logFileStream != null)
                _logFileStream.WriteLine("[" + show + "] " + line);
        }

        #endregion
    }
}
