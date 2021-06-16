using System;
using System.Collections.Generic;

using Verse;

namespace alaestor_teleporting
{
	public class CompCooldown : ThingComp
	{
		public CompProperties_Cooldown Props => (CompProperties_Cooldown)this.props;
		public int ShortCooldownTicks => Props != null ? Props.ShortCooldownTicks : 0;
		public int LongCooldownTicks => Props != null ? Props.LongCooldownTicks : 0;
		public int Remaining;

		public bool IsOnCooldown => this.Remaining > 0;
		public static implicit operator bool(CompCooldown c) => c.IsOnCooldown;

		// TODO mote / cooldown icon

		public void Reset()
		{
			this.Remaining = 0;
		}

		public void Add(int ticks)
		{
			this.Remaining += ticks;
		}

		public void Subtract(int ticks)
		{
			if (this.Remaining > 0)
			{
				if (this.Remaining - ticks >= 0) this.Remaining -= ticks;
				else this.Remaining = 0;

			}
		}

		public void SetToShortCooldown(bool force = false)
		{
			if (this.Remaining == 0 || force) this.Remaining = this.ShortCooldownTicks;
		}

		public void SetToLongCooldown(bool force = false)
		{
			if (this.Remaining == 0 || force) this.Remaining = this.LongCooldownTicks;
		}

		public override void CompTick()
		{ // ticks every 1/60th second (1t / 60tps)
			base.CompTick();
			if (this.Remaining > 0) --this.Remaining;
		}

		public override void CompTickRare()
		{ // ticks every 4.16 seconds (250t / 60tps)
			base.CompTickRare();
			if (this.Remaining > 0)
			{
				if (this.Remaining - 250 >= 0) this.Remaining -= 250;
				else this.Remaining = 0;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Cooldown",
					defaultDesc = "Reset cooldown to 0",
					activateSound = SoundDef.Named("Click"),
					action = delegate { this.Reset(); }
				};

				/*
				// Key-Binding 1
				Command_Action opt1;
				opt1 = new Command_Action();
				opt1.icon = X2_Building_AIRobotRechargeStation.UI_ButtonGoLeft;
				opt1.defaultLabel = "";// "LEFT";
				opt1.defaultDesc = "Go 4 left";
				opt1.hotKey = KeyBindingDefOf.Misc5;
				opt1.activateSound = SoundDef.Named("Click");
				opt1.action = delegate { Debug_ForceGotoDistance(-4, 0); };
				opt1.disabled = false;
				opt1.disabledReason = "";
				opt1.groupKey = 1234567 + 1;
				yield return opt1;
				*/
			}
		}

	}

	public class CompProperties_Cooldown : CompProperties
	{
		public int ShortCooldownTicks;
		public int LongCooldownTicks;

		public CompProperties_Cooldown()
		{
			this.compClass = typeof(CompCooldown);
		}

		public CompProperties_Cooldown(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}// namespace alaestor_teleporting
