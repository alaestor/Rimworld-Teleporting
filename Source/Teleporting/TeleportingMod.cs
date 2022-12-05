using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class TeleportingMod : Mod
	{
		public static TeleportingModSettings settings;
		public static readonly string modname = "Teleporting";
		public static readonly string version = "version 0.1.0\t( \"minimal effort\" )";

		public TeleportingMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<TeleportingModSettings>();
			Log.Message("[" + modname + "] " + version);
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			TeleportingModSettings.DoSettingsWindowContents(inRect);
			base.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory() => modname;
	}
}// namespace alaestor_teleporting
