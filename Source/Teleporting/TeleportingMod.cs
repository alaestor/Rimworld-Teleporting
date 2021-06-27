using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class TeleportingMod : Mod
	{
		public static TeleportingModSettings settings;

		public TeleportingMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<TeleportingModSettings>();
		}

		private static void AddSettingsSection_Cooldown(ref Listing_Standard ls)
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

		private static void AddSettingsSection_Fuel(ref Listing_Standard ls)
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
					leftLabel: ("longRange_FuelDistance_Msg".Translate()),
					rightLabel: settings.longRange_FuelDistance == 0 ?
						String.Format("always consume {0} fuel"/*"longRange_FuelDistance_FixedMsg".Translate()*/, settings.longRange_FuelCost)
						: String.Format("Consumes {0} fuel every {1} tiles"/*"longRange_FuelDistance_ScalesMsg".Translate()*/, settings.longRange_FuelCost, settings.longRange_FuelDistance),
					tip: null);
				ls.Gap(10);
			}
		}

		private static void AddSettingsSection_RangeLimit(ref Listing_Standard ls)
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

		private static void AddSettingsSection_DebugAndCheats(ref Listing_Standard ls)
		{
			ls.GapLine();
			ls.CheckboxLabeled("enableDebugGizmosInGodmode".Translate(), ref settings.enableDebugGizmosInGodmode, tooltip: "enableDebugGizmosInGodmode_tooltip".Translate());
			ls.CheckboxLabeled("enableDebugLogging".Translate(), ref settings.enableDebugLogging, tooltip: "enableDebugLogging_tooltip".Translate());
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard ls = new Listing_Standard();
			ls.Begin(inRect);

			AddSettingsSection_Cooldown(ref ls);
			AddSettingsSection_Fuel(ref ls);
			AddSettingsSection_RangeLimit(ref ls);
			AddSettingsSection_DebugAndCheats(ref ls);

			// Settings reset
			ls.Gap(100);
			if (ls.ButtonTextLabeled("", "resetTheseSettings".Translate()))
			{
				settings.ResetToDefaults();
			}

			ls.End();
			base.DoSettingsWindowContents(inRect);
			//settings.Write();
		}

		public override string SettingsCategory() => "Teleporting".Translate();
	}
}// namespace alaestor_teleporting
