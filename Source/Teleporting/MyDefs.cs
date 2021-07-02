using RimWorld;
using Verse;

namespace alaestor_teleporting
{
	class MyDefs
	{
		public static readonly ThingDef teleportPlatform = ThingDef.Named("alaestor_teleporting_TeleportPlatform");
		public static readonly ThingDef teleportConsole = ThingDef.Named("alaestor_teleporting_TeleportConsole");
		public static readonly ThingDef teleportCartridge = ThingDef.Named("alaestor_teleporting_TeleportCartridge");
	}

	[DefOf]
	public static class TeleporterDefOf
	{ // I'm 99% sure there's a better way to do this
		public static readonly JobDef UseTeleportPlatform_TeleportToLink;
		public static readonly JobDef UseTeleportPlatform_MakeLink;
		public static readonly JobDef UseTeleportConsole_ShortRange;
		public static readonly JobDef UseTeleportConsole_LongRange;
	}
}
