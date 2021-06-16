using System;
using System.Collections.Generic;

using Verse;

namespace alaestor_teleporting
{
	public class CompCooldown : ThingComp
	{
		public CompProperties_Cooldown Props => (CompProperties_Cooldown)this.props;
		public int remaining;

		// TODO display seconds remaining (see refuelable fuel burn time)

		public bool IsOnCooldown => this.remaining > 0;
		public static implicit operator bool(CompCooldown c) => c.IsOnCooldown;

		// TODO mote / cooldown icon

		public void Reset()
		{
			this.remaining = 0;
		}

		public void Set(int ticks)
		{
			if (ticks > 0) this.remaining = ticks;
			else this.remaining = 0;
		}

		public void SetSeconds(int seconds)
		{
			this.Set(seconds * 60);
		}

		public void Add(int ticks)
		{
			this.remaining += ticks;
		}

		public void AddSeconds(int seconds)
		{
			this.remaining += seconds * 60;
		}

		public void Subtract(int ticks)
		{
			if (this.remaining > 0)
			{
				if (this.remaining - ticks >= 0) this.remaining -= ticks;
				else this.remaining = 0;
			}
		}

		public void SubtractSeconds(int seconds)
		{
			if (this.remaining > 0)
			{
				if (this.remaining - (seconds * 60) >= 0) this.remaining -= seconds * 60;
				else this.remaining = 0;
			}
		}

		public override void CompTick()
		{ // ticks every 1/60th second (1t / 60tps)
			base.CompTick();
			if (this.remaining > 0) --this.remaining;
		}

		public override void CompTickRare()
		{ // ticks every 4.16 seconds (250t / 60tps)
			base.CompTickRare();
			if (this.remaining > 0)
			{
				if (this.remaining - 250 >= 0) this.remaining -= 250;
				else this.remaining = 0;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.remaining, "Remaining", 0);
		}
		
		public override string CompInspectStringExtra()
		{
			if (this.IsOnCooldown)
			{
				return "IsOnCooldown_Label".Translate() + ": " + ((int)(remaining / 60)).ToString() + " seconds remaining";
			}
			else return "";
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "MakeCoolDebugGizmo_Label".Translate(),
					defaultDesc = "MakeCoolDebugGizmo_Desc".Translate(), //"Reset cooldown to 0",
					activateSound = SoundDef.Named("Click"),
					action = delegate { this.Reset(); }
				};
			}
		}
	}

	public class CompProperties_Cooldown : CompProperties
	{
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
