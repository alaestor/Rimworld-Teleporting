using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class TeleportingModSettings : ModSettings
	{
		// Cooldowns
		private static readonly bool enableCooldown_Default = true;
		public bool enableCooldown = enableCooldown_Default;

		private static readonly bool enableCooldown_Console_Default = true;
		public bool enableCooldown_Console = enableCooldown_Console_Default;

		private static readonly bool enableCooldown_ApparelComp_Default = true;
		public bool enableCooldown_ApparelComp = enableCooldown_ApparelComp_Default;

		private static readonly bool enableCooldown_Platform_Default = true;
		public bool enableCooldown_Platform = enableCooldown_Platform_Default;

		private static readonly int nameLinkable_CooldownDuration_Default = 10;
		public int nameLinkable_CooldownDuration = nameLinkable_CooldownDuration_Default;
		public string nameLinkable_CooldownDuration_Buffer = nameLinkable_CooldownDuration_Default.ToString();

		private static readonly int shortRange_CooldownDuration_Default = 30;
		public int shortRange_CooldownDuration = shortRange_CooldownDuration_Default;
		public string shortRange_CooldownDuration_Buffer = shortRange_CooldownDuration_Default.ToString();

		private static readonly int longRange_CooldownDuration_Default = 120;
		public int longRange_CooldownDuration = longRange_CooldownDuration_Default;
		public string longRange_CooldownDuration_Buffer = longRange_CooldownDuration_Default.ToString();



		// Console cooldown intelect modifier
		private static readonly bool enableConsoleIntelectDivisor_Default = true;
		public bool enableConsoleIntelectDivisor = enableConsoleIntelectDivisor_Default;

		private static readonly int consoleIntelectDivisor_Default = 21;
		public int consoleIntelectDivisor = consoleIntelectDivisor_Default;
		public string consoleIntelectDivisor_Buffer = consoleIntelectDivisor_Default.ToString();



		// Fuel
		private static readonly bool enableFuel_Default = true;
		public bool enableFuel = enableFuel_Default;

		private static readonly bool enableApparelFuel_Default = true;
		public bool enableApparelFuel = enableApparelFuel_Default;

		private static readonly bool enablePlatformUnlinkFuelCost_Default = true;
		public bool enablePlatformUnlinkFuelCost = enablePlatformUnlinkFuelCost_Default;

		private static readonly int shortRange_FuelCost_Default = 1;
		public int shortRange_FuelCost = shortRange_FuelCost_Default;
		public string shortRange_FuelCost_Buffer = shortRange_FuelCost_Default.ToString();

		private static readonly int longRange_FuelCost_Default = 5;
		public int longRange_FuelCost = longRange_FuelCost_Default;
		public string longRange_FuelCost_Buffer = longRange_FuelCost_Default.ToString();

		private static readonly int longRange_FuelDistance_Default = 10;
		public int longRange_FuelDistance = longRange_FuelDistance_Default;
		public string longRange_FuelDistance_Buffer = longRange_FuelDistance_Default.ToString();



		// global teleport range limit
		private static readonly bool enableGlobalRangeLimit_Default = true;
		public bool enableGlobalRangeLimit = enableGlobalRangeLimit_Default;

		private static readonly int globalRangeLimit_Default = 50;
		public int globalRangeLimit = globalRangeLimit_Default;
		public string globalRangeLimit_Buffer = globalRangeLimit_Default.ToString();



		// Debug options & cheats
		private static readonly bool enableDebugGizmosInGodmode_Default = true;
		public bool enableDebugGizmosInGodmode = enableDebugGizmosInGodmode_Default;

		private static readonly bool enableDebugLogging_Default = false;
		public bool enableDebugLogging = enableDebugLogging_Default;

		private static readonly bool enableDebugLoggingVerbose_Default = false;
		public bool enableDebugLoggingVerbose = enableDebugLoggingVerbose_Default;



		public void RefreshStringBuffers()
		{
			// cooldown
			nameLinkable_CooldownDuration_Buffer = nameLinkable_CooldownDuration.ToString();
			shortRange_CooldownDuration_Buffer = shortRange_CooldownDuration.ToString();
			longRange_CooldownDuration_Buffer = longRange_CooldownDuration.ToString();
			consoleIntelectDivisor_Buffer = consoleIntelectDivisor.ToString();

			// fuel
			shortRange_FuelCost_Buffer = shortRange_FuelCost.ToString();
			longRange_FuelCost_Buffer = longRange_FuelCost.ToString();
			longRange_FuelDistance_Buffer = longRange_FuelDistance.ToString();

			// range limit
			globalRangeLimit_Buffer = globalRangeLimit_Default.ToString();
		}

		public void ResetToDefaults()
		{
			// cooldown
			enableCooldown = enableCooldown_Default;
			enableCooldown_Console = enableCooldown_Console_Default;
			enableCooldown_ApparelComp = enableCooldown_ApparelComp_Default;
			enableCooldown_Platform = enableCooldown_Platform_Default;
			nameLinkable_CooldownDuration = nameLinkable_CooldownDuration_Default;
			shortRange_CooldownDuration = shortRange_CooldownDuration_Default;
			longRange_CooldownDuration = longRange_CooldownDuration_Default;
			enableConsoleIntelectDivisor = enableConsoleIntelectDivisor_Default;
			consoleIntelectDivisor = consoleIntelectDivisor_Default;

			// fuel
			enableFuel = enableFuel_Default;
			enableApparelFuel = enableApparelFuel_Default;
			enablePlatformUnlinkFuelCost = enablePlatformUnlinkFuelCost_Default;
			shortRange_FuelCost = shortRange_FuelCost_Default;
			longRange_FuelCost = longRange_FuelCost_Default;
			longRange_FuelDistance = longRange_FuelDistance_Default;

			// range limit
			enableGlobalRangeLimit = enableGlobalRangeLimit_Default;

			// debug and cheats
			enableDebugGizmosInGodmode = enableDebugGizmosInGodmode_Default;
			enableDebugLogging = enableDebugLogging_Default;
			enableDebugLoggingVerbose = enableDebugLoggingVerbose_Default;

			RefreshStringBuffers();
		}

		public override void ExposeData()
		{
			// cooldown
			Scribe_Values.Look(ref enableCooldown, "enableCooldown", enableCooldown_Default);
			Scribe_Values.Look(ref enableCooldown_Console, "enableCooldown_Console", enableCooldown_Console_Default);
			Scribe_Values.Look(ref enableCooldown_ApparelComp, "enableCooldown_ApparelComp", enableCooldown_ApparelComp_Default);
			Scribe_Values.Look(ref enableCooldown_Platform, "enableCooldown_Platform", enableCooldown_Platform_Default);
			Scribe_Values.Look(ref nameLinkable_CooldownDuration, "nameLinkable_CooldownDuration", nameLinkable_CooldownDuration_Default);
			Scribe_Values.Look(ref shortRange_CooldownDuration, "shortRange_CooldownDuration", shortRange_CooldownDuration_Default);
			Scribe_Values.Look(ref longRange_CooldownDuration, "longRange_CooldownDuration", longRange_CooldownDuration_Default);
			Scribe_Values.Look(ref enableConsoleIntelectDivisor, "enableConsoleIntelectDivisor", enableConsoleIntelectDivisor_Default);
			Scribe_Values.Look(ref consoleIntelectDivisor, "consoleIntelectDivisor", consoleIntelectDivisor_Default);

			// fuel
			Scribe_Values.Look(ref enableFuel, "enableFuel", enableFuel_Default);
			Scribe_Values.Look(ref enableApparelFuel, "enableApparelFuel", enableApparelFuel_Default);
			Scribe_Values.Look(ref enablePlatformUnlinkFuelCost, "enablePlatformUnlinkFuelCost", enablePlatformUnlinkFuelCost_Default);
			Scribe_Values.Look(ref shortRange_FuelCost, "shortRange_FuelCost", shortRange_FuelCost_Default);
			Scribe_Values.Look(ref longRange_FuelCost, "longRange_FuelCost", longRange_FuelCost_Default);
			Scribe_Values.Look(ref longRange_FuelDistance, "longRange_FuelDistance", longRange_FuelDistance_Default);

			// debug
			Scribe_Values.Look(ref enableDebugGizmosInGodmode, "enableDebugGizmosInGodmode", enableDebugGizmosInGodmode_Default);
			Scribe_Values.Look(ref enableDebugLogging, "enableDebugLogging", enableDebugLogging_Default);
			Scribe_Values.Look(ref enableDebugLoggingVerbose, "enableDebugLoggingVerbose", enableDebugLoggingVerbose_Default);

			RefreshStringBuffers();

			base.ExposeData();
		}



		/////////////////////////////
		///  Settings Mod Window  ///
		/////////////////////////////



		public static void DoSettingsWindowContents(Rect inRect)
		{
			TeleportingModSettings settings = TeleportingMod.settings;

			Listing_Standard ls = new Listing_Standard();
			ls.ColumnWidth = (float)((inRect.width - 40.0) / 2.0);
			ls.Begin(inRect);
			AddSettings_Cooldown_Options();
			AddSettings_Fuel_Options();
			AddSettings_RangeLimit_Options();
			ls.NewColumn();
			AddSettings_DebugAndCheats_Options();
			ls.Gap(100);
			if (ls.ButtonTextLabeled("", "resetTheseSettings".Translate()))
			{
				settings.ResetToDefaults();
			}
			ls.End();

			// associated option groups:

			void AddSettings_Cooldown_Options()
			{
				ls.GapLine();
				ls.CheckboxLabeled("enableCooldown".Translate(), ref settings.enableCooldown, tooltip: "enableCooldown_tooltip".Translate()); // Note: there are 60 ticks in a second
				if (settings.enableCooldown)
				{
					ls.CheckboxLabeled("enableCooldown_Console".Translate(), ref settings.enableCooldown_Console, tooltip: "enableCooldown_Console_tooltip".Translate());
					ls.CheckboxLabeled("enableCooldown_ApparelComp".Translate(), ref settings.enableCooldown_ApparelComp, tooltip: "enableCooldown_ApparelComp_tooltip".Translate());
					ls.CheckboxLabeled("enableCooldown_Platform".Translate(), ref settings.enableCooldown_Platform, tooltip: "enableCooldown_Platform_tooltip".Translate());
					ls.Gap();
					ls.TextFieldNumericLabeled<int>("nameLinkable_CooldownDuration ".Translate(), ref settings.nameLinkable_CooldownDuration, ref settings.nameLinkable_CooldownDuration_Buffer);
					ls.TextFieldNumericLabeled<int>("shortRange_CooldownDuration".Translate(), ref settings.shortRange_CooldownDuration, ref settings.shortRange_CooldownDuration_Buffer);
					ls.TextFieldNumericLabeled<int>("longRange_CooldownDuration".Translate(), ref settings.longRange_CooldownDuration, ref settings.longRange_CooldownDuration_Buffer);
					ls.Gap();
					ls.CheckboxLabeled("enableConsoleIntelectDivisor".Translate(), ref settings.enableConsoleIntelectDivisor, tooltip: "consoleIntelectDivisor_tooltip".Translate());
					if (settings.enableConsoleIntelectDivisor)
					{
						ls.TextFieldNumericLabeled<int>("consoleIntelectDivisor".Translate(), ref settings.consoleIntelectDivisor, ref settings.consoleIntelectDivisor_Buffer, min: 1, max: 100);
						ls.Gap();
					}
				}
			}

			void AddSettings_Fuel_Options()
			{
				ls.GapLine();
				ls.CheckboxLabeled("enableFuel".Translate(), ref settings.enableFuel, tooltip: "enableFuel_tooltip".Translate());
				if (settings.enableFuel)
				{
					ls.Gap(10);
					ls.CheckboxLabeled("enableApparelFuel".Translate(), ref settings.enableApparelFuel, tooltip: "enableApparelFuel_tooltip".Translate());
					ls.CheckboxLabeled("enablePlatformUnlinkFuelCost".Translate(), ref settings.enablePlatformUnlinkFuelCost, tooltip: "enablePlatformUnlinkFuelCost_tooltip".Translate());
					ls.Gap(10);
					ls.TextFieldNumericLabeled<int>("shortRange_FuelCost".Translate(), ref settings.shortRange_FuelCost, ref settings.shortRange_FuelCost_Buffer);
					ls.TextFieldNumericLabeled<int>("longRange_FuelCost".Translate(), ref settings.longRange_FuelCost, ref settings.longRange_FuelCost_Buffer);
					ls.TextFieldNumericLabeled<int>("longRange_FuelDistance".Translate(), ref settings.longRange_FuelDistance, ref settings.longRange_FuelDistance_Buffer);
					ls.LabelDouble(
						leftLabel: (""),
						rightLabel: settings.longRange_FuelDistance == 0 ?
							String.Format("always consume {0} fuel"/*"longRange_FuelDistance_FixedMsg".Translate()*/, settings.longRange_FuelCost)
							: String.Format("Consumes {0} cartridges\nfor every {1} tiles"/*"longRange_FuelDistance_ScalesMsg".Translate()*/, settings.longRange_FuelCost, settings.longRange_FuelDistance),
						tip: null);
					ls.Gap(10);
				}
			}

			void AddSettings_RangeLimit_Options()
			{
				ls.GapLine();
				ls.CheckboxLabeled("enableGlobalRangeLimit".Translate(), ref settings.enableGlobalRangeLimit, tooltip: "enableGlobalRangeLimit_tooltip".Translate());
				if (settings.enableFuel)
				{
					ls.Gap(10);
					ls.TextFieldNumericLabeled<int>("globalRangeLimit".Translate(), ref settings.globalRangeLimit, ref settings.globalRangeLimit_Buffer);
					ls.Gap(10);
				}
			}

			void AddSettings_DebugAndCheats_Options()
			{
				ls.GapLine();
				ls.CheckboxLabeled("enableDebugGizmosInGodmode".Translate(), ref settings.enableDebugGizmosInGodmode, tooltip: "enableDebugGizmosInGodmode_tooltip".Translate());
				ls.CheckboxLabeled("enableDebugLogging".Translate(), ref settings.enableDebugLogging, tooltip: "enableDebugLogging_tooltip".Translate());
				if (settings.enableDebugLogging)
				{
					ls.CheckboxLabeled("enableDebugLoggingVerbose".Translate(), ref settings.enableDebugLoggingVerbose, tooltip: "enableDebugLoggingVerbose_tooltip".Translate());
					if (ls.ButtonTextLabeled("", "testDebugLogging".Translate()))
					{
						Logger.TestLogger();
					}
				}
				else if (settings.enableDebugLoggingVerbose) settings.enableDebugLoggingVerbose = false;
			}
		}
	}
}// namespace alaestor_teleporting
