using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

// I know low level C/C++
// I don't know C#
// If you're trying to use this code as an example; may god have mercy on your soul.

namespace alaestor_teleporting
{
	public class Building_TeleportConsole : Building
	{
		private CompPowerTrader powerComp;
		private CompRefuelable refuelableComp;
		private CompCooldown cooldownComp;
		// private ??? fuelComp; // for teleport cartridges

		public override void Tick()
		{
			base.Tick();
		}

		public override void TickRare()
		{
			base.TickRare();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = this.GetComp<CompPowerTrader>();
			this.refuelableComp = this.GetComp<CompRefuelable>();
			this.cooldownComp = this.GetComp<CompCooldown>();
			if (this.cooldownComp == null)
			{
				Log.Error("A Building_TeleportConsole class object didn't provide a cooldownComp. Creating a default one.");
				this.cooldownComp = new CompCooldown();
			}
		}

		public bool CanUseConsoleNow
		{
			get
			{
				if (this.Spawned && this.Map.gameConditionManager.ElectricityDisabled)
					return false;
				return this.powerComp == null || this.powerComp.PowerOn;
			}
		}

		private FloatMenuOption GetFailureReason(Pawn myPawn)
		{
			if (!myPawn.CanReach((LocalTargetInfo)(Thing)this, PathEndMode.InteractionCell, Danger.Some))
				return new FloatMenuOption((string)"CannotUseNoPath".Translate(), (Action)null);
			else if (this.Spawned && this.Map.gameConditionManager.ElectricityDisabled)
				return new FloatMenuOption((string)"CannotUseSolarFlare".Translate(), (Action)null);
			else if (this.powerComp != null && !this.powerComp.PowerOn)
				return new FloatMenuOption((string)"CannotUseNoPower".Translate(), (Action)null);
			else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				return new FloatMenuOption((string)"CannotUseReason".Translate((NamedArgument)"IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Manipulation.label, myPawn.Named("PAWN"))), (Action)null);
			else if (this.cooldownComp != null && this.cooldownComp) // on cooldown
				return new FloatMenuOption((string)"CannotUseCooldown".Translate(), (Action)null); // TODO lang
			else if (this.CanUseConsoleNow)
				return (FloatMenuOption)null;
			Log.Error(myPawn.ToString() + " could not use teleport console for unknown reason.");
			return new FloatMenuOption("Cannot use now", (Action)null);
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			Building_TeleportConsole console = this;
			FloatMenuOption failureReason = console.GetFailureReason(myPawn);
			if (failureReason != null)
			{
				yield return failureReason;
			}
			else
			{
				// if fuel > FuelConsumption_ShortRange
				string short_Label = "Teleport Short Range"; // (string)"CallOnRadio".Translate((NamedArgument)this.GetCallLabel());
				Action short_Action = (Action)(() =>
				{
					//GiveUseTeleportJob(myPawn);
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportConsole_ShortRange, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(short_Label, short_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)console);

				// if fuel > FuelConsumption_LongRange
				string long_Label = "Teleport Long Range"; // (string)"CallOnRadio".Translate((NamedArgument)this.GetCallLabel());
				Action long_Action = (Action)(() =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportConsole_LongRange, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(long_Label, long_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)console);
			}
		}

		public void TryStartTeleport(Pawn controllingPawn, bool longRangeFlag)
		{
			if (this.cooldownComp)
				return;

			if (longRangeFlag)
			{
				// if fuel > big gulp, fuel-= big gulp, else return
				this.cooldownComp.SetToLongCooldown();
			}
			else
			{
				// if fuel > small gulp, fuel-= small gulp, else return
				this.cooldownComp.SetToShortCooldown();
			}

			//this.cooldownComp.subtract(controllingPawn intelect * 10);

			TeleportBehavior.DoTeleport(longRangeFlag);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				yield return gizmo;

			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "ShortTeleDebugGizmo_Label".Translate(), //"Tele Local",
					defaultDesc = "ShortTeleDebugGizmo_Desc".Translate(), //"Teleport on map layer",
					activateSound = SoundDef.Named("Click"),
					action = delegate { TeleportBehavior.DoTeleport(false); }
				};

				yield return new Command_Action
				{
					defaultLabel = "LongTeleDebugGizmo_Label".Translate(), //"Tele Far",
					defaultDesc = "LongTeleDebugGizmo_Desc".Translate(), //"Teleport on world layer",
					activateSound = SoundDef.Named("Click"),
					action = delegate { TeleportBehavior.DoTeleport(true); }
				};
			}
		}
	}
}// namespace alaestor_teleporting