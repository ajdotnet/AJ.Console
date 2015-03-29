// Source: https://github.com/ajdotnet/AJ.Console

namespace AJ.Console
{
    /// <summary>
    /// showlevels
    /// </summary>
	public enum ShowLevel
	{
		/// <summary>normal and verbose output</summary>
		Verbose,	
		/// <summary>normal output</summary>
		Normal,		
		/// <summary>important output</summary>
		Important,	
		/// <summary>warnings</summary>
		Warning,
		/// <summary>errors</summary>
		Error
	}

	/// <summary>
	/// special return values
	/// </summary>
	internal struct SpecialReturnValue
	{
		/// <summary>all ok</summary>
		public const byte OK						= 0;
		/// <summary>help requested</summary>
		public const byte HelpRequested				= 253;
		/// <summary>an exception has been handled, processing stopped</summary>
		public const byte HandledException			= 254;
		/// <summary>an unhandled exception has occured</summary>
		public const byte UnHandledException		= 255;
	}

	/// <summary>
	/// names of mmessages
	/// </summary>
	internal struct ResKey
	{
		/// <summary></summary>
		public const string Syntax	= "Syntax";
		/// <summary></summary>
        public const string SystemParameters = "SystemParameters";
		/// <summary></summary>
		public const string Logo	= "Logo";
		/// <summary></summary>
		public const string Error	= "Error";
		/// <summary></summary>
		public const string Help	= "Help";
	}

	/// <summary>
	/// exception messages
	/// </summary>
	internal struct ResEx
	{
		public const string CouldNotReadParameterFile	= "Ex.CouldNotReadParameterFile";
		public const string InvalidArgumentsNull		= "Ex.InvalidArgumentsNull";
		public const string InvalidArgumentsMin			= "Ex.InvalidArgumentsMin";
		public const string InvalidArgumentsMax			= "Ex.InvalidArgumentsMax";
		public const string UnknownSwitch				= "Ex.UnknownSwitch";
		public const string InvalidArguments			= "Ex.InvalidArguments";
		public const string LogfileCouldNotBeOpened		= "Ex.LogfileCouldNotBeOpened";
	}
}
