using Verse;

namespace alaestor_teleporting
{
	//[StaticConstructorOnStartup]
	class Logger
	{
		private static readonly string prefix_modname = "[" + TeleportingMod.modname + "]";
		private static readonly string prefix_debug = "[DEBUG]";
		private static readonly string prefix_debugVerbose = "[DEBUG]";
		private static readonly string prefix_details = "[DETAILS]";
		private static readonly string prefix_warning = "[WARN]";
		private static readonly string prefix_error = "[ERROR]";
		private static readonly string prefix_indent = "\t";
		private static readonly string prefix_sep = " ";
		private static readonly string prefix_delim = ": ";

		private static readonly string debug_header = prefix_modname + prefix_sep + prefix_debug + prefix_delim;
		private static readonly string debugVerbose_header = prefix_modname + prefix_sep + prefix_debugVerbose + prefix_delim;
		private static readonly string details_header = prefix_sep + prefix_details + prefix_delim;
		private static readonly string warning_header = prefix_modname + prefix_sep + prefix_warning + prefix_delim;
		private static readonly string error_header = prefix_modname + prefix_sep + prefix_error + prefix_delim;

		// there have a lot of duplicate checks, but this is most flexible and shouldn't impact normal games
		public static bool IsDebug => TeleportingMod.settings.enableDebugLogging;
		public static bool IsDebugVerbose => TeleportingMod.settings.enableDebugLoggingVerbose;


		// Details handles Verbose detail logging; details provided via variadic infoStrings
		private static void Details(string prefix, params string[] infoStrings)
		{
			Details(prefix, false, infoStrings);
		}
		private static void Details(string prefix, bool debugBypass, params string[] infoStrings)
		{
			if (debugBypass || (IsDebug && IsDebugVerbose && infoStrings.Length > 0))
			{
				Log.Message(prefix_indent + prefix + prefix_sep + details_header);
				foreach (var str in infoStrings)
				{
					Log.Message(prefix_indent + prefix_indent + str);
				}
			}
		}

		public static void DebugVerbose(string msg, params string[] infoStrings)
		{
			DebugVerbose(msg, false, infoStrings);
		}
		public static void DebugVerbose(string msg, bool debugBypass, params string[] infoStrings)
		{
			if (debugBypass || (IsDebug && IsDebugVerbose))
			{
				Log.Message(debugVerbose_header + msg);
				if (debugBypass && infoStrings.Length > 0) Details(prefix_debugVerbose, debugBypass, infoStrings);
			}
		}

		public static void Debug(string msg, params string[] infoStrings)
		{
			Debug(msg, false, infoStrings);
		}
		public static void Debug(string msg, bool debugBypass, params string[] infoStrings)
		{
			if (debugBypass || IsDebug)
			{
				Log.Message(debug_header + msg);
				if (debugBypass || (IsDebugVerbose && infoStrings.Length > 0)) Details(prefix_debug, debugBypass, infoStrings);
			}
		}



		public static void Warning(string msg, params string[] infoStrings)
		{
			Warning(msg, false, infoStrings);
		}
		public static void Warning(string msg, bool debugBypass, params string[] infoStrings)
		{
			Log.Warning(warning_header + msg);
			if (debugBypass || (IsDebugVerbose && infoStrings.Length > 0)) Details(prefix_warning, debugBypass, infoStrings);
		}



		public static void Error(string msg, params string[] infoStrings)
		{
			Error(msg, false, infoStrings);
		}
		public static void Error(string msg, bool debugBypass, params string[] infoStrings)
		{
			Log.Error(error_header + msg);
			if (debugBypass || (IsDebugVerbose && infoStrings.Length > 0)) Details(prefix_error, debugBypass, infoStrings);
		}



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