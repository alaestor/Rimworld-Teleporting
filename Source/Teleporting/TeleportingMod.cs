﻿using UnityEngine;
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

		public override void DoSettingsWindowContents(Rect inRect)
		{

			Listing_Standard l = new Listing_Standard();
			l.Begin(inRect);
			l.Label("NoteTicksPerSecond".Translate()); // Note: there are 60 ticks in a second

			// Cooldowns
			l.GapLine();
			l.CheckboxLabeled("enableCooldown".Translate(), ref settings.enableCooldown, tooltip: "enableCooldown_tooltip".Translate());
			if (settings.enableCooldown)
			{
				l.CheckboxLabeled("enableIntelectDivisor".Translate(), ref settings.enableIntelectDivisor, tooltip: "intelectDivisor_tooltip".Translate());
				if (settings.enableIntelectDivisor)
				{
					l.TextFieldNumericLabeled<int>("intelectDivisor".Translate(), ref settings.intelectDivisor, ref settings.intelectDivisor_Buffer, min: 1, max: 100);
					l.Gap();
				}
				l.TextFieldNumericLabeled<int>("shortRange_CooldownDuration".Translate(), ref settings.shortRange_CooldownDuration, ref settings.shortRange_CooldownDuration_Buffer);
				l.TextFieldNumericLabeled<int>("longRange_CooldownDuration".Translate(), ref settings.longRange_CooldownDuration, ref settings.longRange_CooldownDuration_Buffer);
			}

			// Fuel
			l.GapLine();
			l.CheckboxLabeled("enableFuel".Translate(), ref settings.enableFuel, tooltip: "enableFuel_tooltip".Translate());
			if (settings.enableFuel)
			{
				l.Gap(10);
				l.TextFieldNumericLabeled<int>("shortRange_FuelCost".Translate(), ref settings.shortRange_FuelCost, ref settings.shortRange_FuelCost_Buffer);
				l.TextFieldNumericLabeled<int>("longRange_FuelCost".Translate(), ref settings.longRange_FuelCost, ref settings.longRange_FuelCost_Buffer);
				l.Gap(10);
			}

			// Debug options & Cheats
			l.GapLine();
			l.CheckboxLabeled("enableDebugGizmosInGodmode".Translate(), ref settings.enableDebugGizmosInGodmode, tooltip: "enableDebugGizmosInGodmode_tooltip".Translate());

			// Settings reset
			l.Gap(100);

			if (l.ButtonTextLabeled("", "resetTheseSettings".Translate()))
			{
				settings.ResetToDefaults();
			}

			l.End();
			base.DoSettingsWindowContents(inRect);
			//settings.Write();
		}

		public override string SettingsCategory() => "Teleporting".Translate();
	}
}// namespace alaestor_teleporting
