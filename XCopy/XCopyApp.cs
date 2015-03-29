using AJ.Common;
using AJ.Console;
using System;
using System.Globalization;

namespace XCopy
{
    class XCopyApp : ConsoleApp
    {
        [STAThread]
        static int Main(string[] args)
        {
            XCopyApp app = new XCopyApp();
            return app.Run(args);
        }

        protected override void ApplyArguments(string[] values)
        {
            // on argument is mandatory, the second optional
            EnsureLength(values, 1, 2, null);
        }

        protected override void ApplySwitch(string name, string[] values)
        {
            Guard.AssertNotNull(name, "name");

            switch (name.ToUpperInvariant())
            {
                case "/A":
                case "/M":
                    // no additional arguments
                    EnsureLength(values, 0, 0, name);
                    break;
                case "/EXCLUDE":
                    // at least one additional argument
                    EnsureLength(values, 1, int.MaxValue, name);
                    break;
                default:
                    // we don't like what we don't know
                    ThrowUnknownSwitch(name);
                    break;
            }
        }

        protected override void Process()
        {
            // ust print out some of the arguments intention
            string[] args = GetArguments();

            WriteLine("about to copy the following files: " + args[0]);

            if (args.Length > 1)
                WriteLine("target is: " + args[1]);

            if (HasSwitch("/a"))
                WriteLine("only if archive bit is set, leaves the bit as is.");

            if (HasSwitch("/m"))
                WriteLine("only if archive bit is set, clears the bit afterwards.");

            WriteLine("Note: no har is done ;-)");
        }
    }
}
