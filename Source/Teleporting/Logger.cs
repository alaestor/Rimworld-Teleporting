using Verse;

namespace alaestor_teleporting
{
	//[StaticConstructorOnStartup]
	class Logger
	{
		private static readonly string prefix_modname = "[" + TeleportingMod.modname + "]";
		private static readonly string prefix_debug = "[DEBUG]";
		private static readonly string prefix_debugVerbose = prefix_debug + prefix_sep + "[VERBOSE]";
		private static readonly string prefix_details = "[DETAILS]";
		private static readonly string prefix_warning = "[WARN]";
		private static readonly string prefix_error = "[ERROR]";
		private static readonly string prefix_indent = "\t";
		private static readonly string prefix_sep = " ";
		private static readonly string prefix_delim = ": ";

		private static readonly string details_start = "\n<DETAILS>";
		private static readonly string details_end = "\n</DETAILS>\n\n";

		private static readonly string details_header = prefix_sep + prefix_details + prefix_delim;
		private static readonly string debug_header = prefix_modname + prefix_sep + prefix_debug + prefix_delim;
		private static readonly string debugVerbose_header = prefix_modname + prefix_sep + prefix_debugVerbose + prefix_delim;
		private static readonly string warning_header = prefix_modname + prefix_sep + prefix_warning + prefix_delim;
		private static readonly string error_header = prefix_modname + prefix_sep + prefix_error + prefix_delim;

		// there have a lot of duplicate checks, but this is most flexible and shouldn't impact normal games
		public static bool IsDebug => TeleportingMod.settings.enableDebugLogging;
		public static bool IsDebugVerbose => TeleportingMod.settings.enableDebugLoggingVerbose;


		// Details handles Verbose detail logging; details provided via variadic infoStrings
		private static void Details(string prefix, params string[] infoStrings)
		{
			if (IsDebug && IsDebugVerbose && infoStrings.Length > 0)
				Details(prefix, false, infoStrings);
		}
		private static string Details(string prefix, bool debugBypass, params string[] infoStrings)
		{
			if ((debugBypass || IsDebugVerbose) && infoStrings.Length > 0) //Details(prefix_error, debugBypass, infoStrings);
			{
				string detailsMsg = "\n" + prefix_indent + prefix + details_header + prefix_indent + infoStrings.Length.ToString() + " details  (click to view all)" + details_start;
				foreach (var str in infoStrings)
				{
					detailsMsg += "\n" + prefix_indent + prefix_indent + str;
				}
				detailsMsg += details_end;
				return detailsMsg;
			}
			else return "";
		}

		// Extremely spammy
		public static void DebugVerbose(string msg, params string[] infoStrings)
		{
			if (IsDebug && IsDebugVerbose)
				DebugVerbose(msg, false, infoStrings);
		}
		public static void DebugVerbose(string msg, bool debugBypass, params string[] infoStrings)
		{
			if (debugBypass || (IsDebug && IsDebugVerbose))
			{
				string logMsg = debugVerbose_header + msg;
				logMsg += Details(prefix_debugVerbose, debugBypass, infoStrings);
				Log.Message(logMsg);
			}
		}

		// Spammy
		public static void Debug(string msg, params string[] infoStrings)
		{
			if (IsDebug)
				Debug(msg, false, infoStrings);
		}
		public static void Debug(string msg, bool debugBypass, params string[] infoStrings)
		{
			if (debugBypass || IsDebug)
			{
				string logMsg = debug_header + msg;
				logMsg += Details(prefix_debug, debugBypass, infoStrings);
				Log.Message(logMsg);
			}
		}

		// Rare
		public static void Warning(string msg, params string[] infoStrings)
		{
			Warning(msg, false, infoStrings);
		}
		public static void Warning(string msg, bool debugBypass, params string[] infoStrings)
		{
			string logMsg = warning_header + msg;
			logMsg += Details(prefix_warning, debugBypass, infoStrings);
			Log.Warning(logMsg);
		}

		// Rarer
		public static void Error(string msg, params string[] infoStrings)
		{
			Error(msg, false, infoStrings);
		}
		public static void Error(string msg, bool debugBypass, params string[] infoStrings)
		{
			string logMsg = error_header + msg;
			logMsg += Details(prefix_error, debugBypass, infoStrings);
			Log.Error(logMsg);
		}

		// Tester; logs example messages
		public static void TestLogger()
		{
			Debug("Singular");
			Debug("With details", "test0", "test1");
			Debug("With details, debug bypass", true, "test2", "test3");
			Warning("Singular");
			Warning("With details", "test4", "test5");
			Warning("With details, debug bypass", true, "test6", "test7");
			Error("Singular");
			Error("With details", "test8", "test9");
			Error("With details, debug bypass", true, "test10", "test11");
		}
	}
}// namespace alaestor_teleporting