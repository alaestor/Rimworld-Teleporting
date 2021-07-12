using RimWorld;
using Verse;
using UnityEngine;

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

		public static readonly ResearchProjectDef research_tier_1_0 = ResearchProjectDef.Named("alaestor_teleporting_Research_tier_1_0");
		public static readonly ResearchProjectDef research_tier_2_0 = ResearchProjectDef.Named("alaestor_teleporting_Research_tier_2_0");
		public static readonly ResearchProjectDef research_tier_3_0 = ResearchProjectDef.Named("alaestor_teleporting_Research_tier_3_0");
		public static readonly ResearchProjectDef research_tier_4_0 = ResearchProjectDef.Named("alaestor_teleporting_Research_tier_4_0");
	}

	[StaticConstructorOnStartup]
	class MyTextures
	{
		public static readonly Texture2D MouseAttachment_SelectDestination = ContentFinder<Texture2D>.Get("UI/Overlays/alaestor_teleporting_select_destination", true);
		public static readonly Texture2D MouseAttachment_SelectPawn = ContentFinder<Texture2D>.Get("UI/Overlays/alaestor_teleporting_select_pawn", true);
		public static readonly Texture2D MouseAttachment_SelectPlatform = ContentFinder<Texture2D>.Get("UI/Overlays/alaestor_teleporting_select_platform", true);
		
		public static readonly Texture2D Gizmo_Rename = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_rename", true);
		public static readonly Texture2D Gizmo_Unlink = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_unlink", true);
		public static readonly Texture2D Gizmo_Link = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_link", true);
		public static readonly Texture2D Gizmo_Link_Broken = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_linkbroken", true);
		public static readonly Texture2D Gizmo_Link_Make_Target = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_link_target", true);
		public static readonly Texture2D Gizmo_Link_Make_Name = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_link_name", true);
		public static readonly Texture2D Gizmo_Teleport_ShortRange = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_tele_shortrange", true);
		public static readonly Texture2D Gizmo_Teleport_LongRange = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_tele_longrange", true);
		public static readonly Texture2D Gizmo_Teleport_Link = ContentFinder<Texture2D>.Get("UI/Gizmos/alaestor_teleporting_tele_link", true);
	}

	[DefOf]
	public static class TeleporterDefOf
	{ // I'm 99% sure there's a better way to do this
		public static readonly JobDef alaestor_teleporting_UseTeleportPlatform_TeleportToLink;
		public static readonly JobDef alaestor_teleporting_UseTeleportPlatform_MakeLinkName;
		public static readonly JobDef alaestor_teleporting_UseTeleportPlatform_MakeLinkTarget;
		public static readonly JobDef alaestor_teleporting_UseTeleportConsole_ShortRange;
		public static readonly JobDef alaestor_teleporting_UseTeleportConsole_LongRange;
	}
}
