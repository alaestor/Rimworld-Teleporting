using Verse;
using System;
using UnityEngine;

namespace alaestor_teleporting
{
	class TeleportingModSettings : ModSettings
	{
		// Cooldowns
		private static readonly bool enableCooldown_Default = true;
		public bool enableCooldown = enableCooldown_Default;

		private static readonly int shortRange_CooldownDuration_Default = 15;
		public int shortRange_CooldownDuration = shortRange_CooldownDuration_Default;
		public string shortRange_CooldownDuration_Buffer = shortRange_CooldownDuration_Default.ToString();

		private static readonly int longRange_CooldownDuration_Default = 60;
		public int longRange_CooldownDuration = longRange_CooldownDuration_Default;
		public string longRange_CooldownDuration_Buffer = longRange_CooldownDuration_Default.ToString();

		// Cooldowns intelect modifier
		private static readonly bool enableIntelectDivisor_Default = true;
		public bool enableIntelectDivisor = enableIntelectDivisor_Default;

		private static readonly int intelectDivisor_Default = 21;
		public int intelectDivisor = intelectDivisor_Default;
		public string intelectDivisor_Buffer = intelectDivisor_Default.ToString();



		// Fuel
		private static readonly bool enableFuel_Default = true;
		public bool enableFuel = enableFuel_Default;

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



		public void RefreshStringBuffers()
		{
			// cooldown
			this.shortRange_CooldownDuration_Buffer = shortRange_CooldownDuration.ToString();
			this.longRange_CooldownDuration_Buffer = longRange_CooldownDuration.ToString();
			this.intelectDivisor_Buffer = intelectDivisor.ToString();

			// fuel
			this.shortRange_FuelCost_Buffer = shortRange_FuelCost.ToString();
			this.longRange_FuelCost_Buffer = longRange_FuelCost.ToString();
			this.longRange_FuelDistance_Buffer = longRange_FuelDistance.ToString();

			// range limit
			this.globalRangeLimit_Buffer = globalRangeLimit_Default.ToString();
		}

		public void ResetToDefaults()
		{
			// cooldown
			this.enableCooldown = enableCooldown_Default;
			this.shortRange_CooldownDuration = shortRange_CooldownDuration_Default;
			this.longRange_CooldownDuration = longRange_CooldownDuration_Default;
			this.enableIntelectDivisor = enableIntelectDivisor_Default;
			this.intelectDivisor = intelectDivisor_Default;

			// fuel
			this.enableFuel = enableFuel_Default;
			this.shortRange_FuelCost = shortRange_FuelCost_Default;
			this.longRange_FuelCost = longRange_FuelCost_Default;
			this.longRange_FuelDistance = longRange_FuelDistance_Default;

			// range limit
			enableGlobalRangeLimit = enableGlobalRangeLimit_Default;

			// debug and cheats
			this.enableDebugGizmosInGodmode = enableDebugGizmosInGodmode_Default;
			this.enableDebugLogging = enableDebugLogging_Default;

			this.RefreshStringBuffers();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref this.enableCooldown, "enableCooldown", enableCooldown_Default);
			Scribe_Values.Look(ref this.shortRange_CooldownDuration, "shortRange_CooldownDuration", shortRange_CooldownDuration_Default);
			Scribe_Values.Look(ref this.longRange_CooldownDuration, "longRange_CooldownDuration", longRange_CooldownDuration_Default);
			Scribe_Values.Look(ref this.enableIntelectDivisor, "enableIntelectDivisor", enableIntelectDivisor_Default);
			Scribe_Values.Look(ref this.intelectDivisor, "intelectDivisor", intelectDivisor_Default);
			Scribe_Values.Look(ref this.enableFuel, "enableFuel", enableFuel_Default);
			Scribe_Values.Look(ref this.shortRange_FuelCost, "shortRange_FuelCost", shortRange_FuelCost_Default);
			Scribe_Values.Look(ref this.longRange_FuelCost, "longRange_FuelCost", longRange_FuelCost_Default);
			Scribe_Values.Look(ref this.longRange_FuelDistance, "longRange_FuelDistance", longRange_FuelDistance_Default);
			Scribe_Values.Look(ref this.enableDebugGizmosInGodmode, "enableDebugGizmosInGodmode", enableDebugGizmosInGodmode_Default);
			Scribe_Values.Look(ref this.enableDebugLogging, "enableDebugLogging", enableDebugLogging_Default);

			this.RefreshStringBuffers();

			base.ExposeData();
		}



		/////////////////////////////
		///  Settings Mod Window  ///
		/////////////////////////////
		


		public static void DoSettingsWindowContents(Rect inRect)
		{
			TeleportingModSettings settings = TeleportingMod.settings;

			Listing_Standard ls = new Listing_Standard();
			ls.ColumnWidth = (float)(((double)inRect.width - 40.0) / 2.0);
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
					ls.CheckboxLabeled("enableIntelectDivisor".Translate(), ref settings.enableIntelectDivisor, tooltip: "intelectDivisor_tooltip".Translate());
					if (settings.enableIntelectDivisor)
					{
						ls.TextFieldNumericLabeled<int>("intelectDivisor".Translate(), ref settings.intelectDivisor, ref settings.intelectDivisor_Buffer, min: 1, max: 100);
						ls.Gap();
					}
					ls.TextFieldNumericLabeled<int>("shortRange_CooldownDuration".Translate(), ref settings.shortRange_CooldownDuration, ref settings.shortRange_CooldownDuration_Buffer);
					ls.TextFieldNumericLabeled<int>("longRange_CooldownDuration".Translate(), ref settings.longRange_CooldownDuration, ref settings.longRange_CooldownDuration_Buffer);
				}
			}

			void AddSettings_Fuel_Options()
			{
				ls.GapLine();
				ls.CheckboxLabeled("enableFuel".Translate(), ref settings.enableFuel, tooltip: "enableFuel_tooltip".Translate());
				if (settings.enableFuel)
				{
					ls.Gap(10);
					ls.TextFieldNumericLabeled<int>("shortRange_FuelCost".Translate(), ref settings.shortRange_FuelCost, ref settings.shortRange_FuelCost_Buffer);
					ls.TextFieldNumericLabeled<int>("longRange_FuelCost".Translate(), ref settings.longRange_FuelCost, ref settings.longRange_FuelCost_Buffer);
					ls.TextFieldNumericLabeled<int>("longRange_FuelDistance".Translate(), ref settings.longRange_FuelDistance, ref settings.longRange_FuelDistance_Buffer);
					ls.LabelDouble(
						leftLabel: (""),
						rightLabel: settings.longRange_FuelDistance == 0 ?
							String.Format("always consume {0} fuel"/*"longRange_FuelDistance_FixedMsg".Translate()*/, settings.longRange_FuelCost)
							: String.Format("Consumes {0} cartridges every {1} tiles"/*"longRange_FuelDistance_ScalesMsg".Translate()*/, settings.longRange_FuelCost, settings.longRange_FuelDistance),
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
					if (ls.ButtonTextLabeled("", "testDebugLogging".Translate()))
					{
						Logger.TestLogger();
					}
				}
			}
		}
	}
}// namespace alaestor_teleporting
