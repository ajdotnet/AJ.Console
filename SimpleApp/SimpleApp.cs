using System;
using AJ.Console;

namespace SimpleApp
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class SimpleApp: ConsoleApp
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			SimpleApp app= new SimpleApp();
			return app.Run(args);
		}

		protected override void ApplyArguments(string[] values)
		{
			ThrowInvalidArguments("No arguments supported!");
		}

		protected override void ApplySwitch(string name, string[] values)
		{
			ThrowUnknownSwitch(name);		
		}

		protected override void Process()
		{
			WriteLine(ShowLevel.Important, "Current show level is "+this.CurrentShowLevel);
			WriteLine(ShowLevel.Verbose, "Verbose output");
			WriteLine(ShowLevel.Normal, "Normal output");
			WriteLine(ShowLevel.Important, "Important output");
			WriteLine(ShowLevel.Warning, "Warning output");
			WriteLine(ShowLevel.Error, "Error output");
		}

	}
}
