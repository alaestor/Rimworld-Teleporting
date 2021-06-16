
using Verse;

namespace alaestor_teleporting
{
	class TeleportingModSettings : ModSettings
	{
		// Cooldowns
		private static readonly bool enableCooldown_Default = true;
		public bool enableCooldown = enableCooldown_Default;

		private static readonly int shortRange_CooldownDuration_Default = 250;
		public int shortRange_CooldownDuration = shortRange_CooldownDuration_Default;
		public string shortRange_CooldownDuration_Buffer = shortRange_CooldownDuration_Default.ToString();

		private static readonly int longRange_CooldownDuration_Default = 1800;
		public int longRange_CooldownDuration = longRange_CooldownDuration_Default;
		public string longRange_CooldownDuration_Buffer = longRange_CooldownDuration_Default.ToString();



		// Fuel
		private static readonly bool enableFuel_Default = true;
		public bool enableFuel = enableFuel_Default;

		private static readonly int shortRange_FuelCost_Default = 1;
		public int shortRange_FuelCost = shortRange_FuelCost_Default;
		public string shortRange_FuelCost_Buffer = shortRange_FuelCost_Default.ToString();

		private static readonly int longRange_FuelCost_Default = 5;
		public int longRange_FuelCost = longRange_FuelCost_Default;
		public string longRange_FuelCost_Buffer = longRange_FuelCost_Default.ToString();



		// Debug options & cheats
		private static readonly bool enableDebugGizmosInGodmode_Default = true;
		public bool enableDebugGizmosInGodmode = enableDebugGizmosInGodmode_Default;



		public void RefreshStringBuffers()
		{
			this.shortRange_CooldownDuration_Buffer = shortRange_CooldownDuration.ToString();
			this.longRange_CooldownDuration_Buffer = longRange_CooldownDuration.ToString();
			this.shortRange_FuelCost_Buffer = shortRange_FuelCost.ToString();
			this.longRange_FuelCost_Buffer = longRange_FuelCost.ToString();
		}

		public void ResetToDefaults()
		{
			this.enableCooldown = enableCooldown_Default;
			this.shortRange_CooldownDuration = shortRange_CooldownDuration_Default;
			this.longRange_CooldownDuration = longRange_CooldownDuration_Default;
			this.enableFuel = enableFuel_Default;
			this.shortRange_FuelCost = shortRange_FuelCost_Default;
			this.longRange_FuelCost = longRange_FuelCost_Default;
			this.enableDebugGizmosInGodmode = enableDebugGizmosInGodmode_Default;

			this.RefreshStringBuffers();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref this.enableCooldown, "enableCooldown", enableCooldown_Default);
			Scribe_Values.Look(ref this.shortRange_CooldownDuration, "shortRange_CooldownDuration", shortRange_CooldownDuration_Default);
			Scribe_Values.Look(ref this.longRange_CooldownDuration, "longRange_CooldownDuration", longRange_CooldownDuration_Default);
			Scribe_Values.Look(ref this.enableFuel, "enableFuel", enableFuel_Default);
			Scribe_Values.Look(ref this.shortRange_FuelCost, "shortRange_FuelCost", shortRange_FuelCost_Default);
			Scribe_Values.Look(ref this.longRange_FuelCost, "longRange_FuelCost", longRange_FuelCost_Default);
			Scribe_Values.Look(ref this.enableDebugGizmosInGodmode, "enableDebugGizmosInGodmode", enableDebugGizmosInGodmode_Default);

			this.RefreshStringBuffers();

			base.ExposeData();
		}
	}
}// namespace alaestor_teleporting
