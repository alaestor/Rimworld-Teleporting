using Verse;

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
	}
}// namespace alaestor_teleporting
