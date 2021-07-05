using RimWorld;
using Verse;

namespace alaestor_teleporting
{
	[StaticConstructorOnStartup]
	class MyDefs
	{
		public static readonly ThingDef teleportPlatform = ThingDef.Named("alaestor_teleporting_TeleportPlatform");
		public static readonly ThingDef teleportReceiver = ThingDef.Named("alaestor_teleporting_TeleportReceiver");
		public static readonly ThingDef teleportConsole = ThingDef.Named("alaestor_teleporting_TeleportConsole");
		public static readonly ThingDef teleportCartridge = ThingDef.Named("alaestor_teleporting_TeleportCartridge");
		public static readonly ThingDef apparel_TeleportBelt_Local = ThingDef.Named("alaestor_teleporting_Apparel_TeleportBelt_Local");
		public static readonly ThingDef apparel_TeleportBelt_Link = ThingDef.Named("alaestor_teleporting_Apparel_TeleportBelt_Link");
		public static readonly ThingDef portableTeleportUnit_Local = ThingDef.Named("alaestor_teleporting_PortableTeleportUnit_Local");
		public static readonly ThingDef portableTeleportUnit_Global = ThingDef.Named("alaestor_teleporting_PortableTeleportUnit_Global");
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
