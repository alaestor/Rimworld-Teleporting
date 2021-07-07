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

		private bool UseCooldown => TeleportingMod.settings.enableCooldown && TeleportingMod.settings.enableCooldown_Console;
		private bool UseFuel => TeleportingMod.settings.enableFuel;

		// if this isn't set, the job toil finishes before targetting begins
		public bool hasStartedTargetting = false;

		/*
		public override void Tick()
		{
			base.Tick();
		}

		public override void TickRare()
		{
			base.TickRare();
		}
		*/

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.hasStartedTargetting, "hasStartedTargetting", false);
		}

		public bool IsDoneTargeting()
		{
			if (this.hasStartedTargetting && !(Find.Targeter.IsTargeting || Find.WorldTargeter.IsTargeting))
			{
				this.hasStartedTargetting = false;
				return true;
			}
			else return false;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = this.GetComp<CompPowerTrader>();
			this.refuelableComp = this.GetComp<CompRefuelable>();
			this.cooldownComp = this.GetComp<CompCooldown>();
		}

		public bool CanUseNow
		{
			get
			{
				if (this.Spawned && this.Map.gameConditionManager.ElectricityDisabled)
					return false;
				return this.powerComp == null || this.powerComp.PowerOn;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			FloatMenuOption failureReason = GetFailureReason();
			FloatMenuOption GetFailureReason()
			{
				if (!myPawn.CanReach((LocalTargetInfo)(Thing)this, PathEndMode.InteractionCell, Danger.Some))
					return new FloatMenuOption((string)"CannotUseNoPath".Translate(), (Action)null);
				else if (this.Spawned && this.Map.gameConditionManager.ElectricityDisabled)
					return new FloatMenuOption((string)"CannotUseSolarFlare".Translate(), (Action)null);
				else if (this.powerComp != null && !this.powerComp.PowerOn)
					return new FloatMenuOption((string)"CannotUseNoPower".Translate(), (Action)null);
				else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					return new FloatMenuOption((string)"CannotUseReason".Translate((NamedArgument)"IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Manipulation.label, myPawn.Named("PAWN"))), (Action)null);
				else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
					return new FloatMenuOption((string)"CannotUseReason".Translate((NamedArgument)"IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Moving.label, myPawn.Named("PAWN"))), (Action)null);
				else if (TeleportingMod.settings.enableCooldown && this.cooldownComp != null && this.cooldownComp.IsOnCooldown)
					return new FloatMenuOption("IsOnCooldown".Translate(), (Action)null);
				else if (this.CanUseNow)
					return (FloatMenuOption)null; // allow use
				Logger.Warning(myPawn.ToString() + "Could not use teleport console for unknown reason.");
				return new FloatMenuOption("Cannot use now", (Action)null);
			}

			if (failureReason != null)
			{
				yield return failureReason;
			}
			else
			{
				string short_Label = "ShortRangeTeleport_Label".Translate();
				Action short_Action = (Action)(() =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportConsole_ShortRange, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});

				string long_Label = "LongRangeTeleport_Label".Translate();
				Action long_Action = (Action)(() =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportConsole_LongRange, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});

				// TODO move these to failure reasons
				if (UseFuel)
				{
					if (this.refuelableComp != null)
					{
						if (this.refuelableComp.Fuel >= TeleportingMod.settings.shortRange_FuelCost)
						{
							yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(short_Label, short_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
						}
						else yield return new FloatMenuOption("shortRange_NotEnoughFuel".Translate(), (Action)null); // restring

						if (this.refuelableComp.Fuel >= TeleportingMod.settings.longRange_FuelCost)
						{
							yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(long_Label, long_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
						}
						else yield return new FloatMenuOption("longRange_NotEnoughFuel".Translate(), (Action)null); // restring
					}
					else Logger.Error("Teleporting: fuel is enabled but refuelableComp is null", refuelableComp.ToString());
				}
				else
				{
					yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(short_Label, short_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
					yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(long_Label, long_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
				}
			}
		}

		public void TryStartTeleport(Pawn controllingPawn, bool longRangeFlag)
		{
			if (UseCooldown)
			{
				if (this.cooldownComp != null)
				{
					if (this.cooldownComp.IsOnCooldown)
					{
						Logger.Warning("Tried to start a teleport but console was on cooldown");
						return;
					}
				}
				else Logger.Error("Teleporting: cooldown is enabled but cooldownComp is null");
			}

			TeleportBehavior.StartTeleportTargetting(longRangeFlag, this, onTeleportSuccess, refuelableComp);

			void onTeleportSuccess(TeleportData teleportData)
			{
				if (UseCooldown)
				{
					int cooldownTicks = (longRangeFlag ?
							TeleportingMod.settings.longRange_CooldownDuration
							: TeleportingMod.settings.shortRange_CooldownDuration) * 60;

					if (TeleportingMod.settings.enableConsoleIntelectDivisor)
					{
						int intelect = controllingPawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
						double multiplier = (double)intelect / TeleportingMod.settings.consoleIntelectDivisor;


						if (multiplier > 0.0)
						{
							if (multiplier >= 1.0)
							{
								cooldownTicks = 0; // no cooldown, >= 100% reduction
							}
							else cooldownTicks -= (int)(cooldownTicks * multiplier);
						}

						if (Logger.IsDebugVerbose)
						{
							Logger.DebugVerbose("onTeleportSuccess :: cooldown :: Intelect divisor",
								"pawn name:  \t" + controllingPawn.Name,
								"intelect:   \t" + intelect.ToString(),
								"divisor:    \t" + TeleportingMod.settings.consoleIntelectDivisor.ToString(),
								"multiplier: \t" + multiplier.ToString() + " (" + ((double)intelect / TeleportingMod.settings.consoleIntelectDivisor).ToString() + ")",
								"reduction:  \t" + (cooldownTicks * multiplier).ToString(),
								"Cooldown:   \t" + cooldownTicks.ToString() + " (" + (cooldownTicks / 60).ToString() + " seconds) from " + ((longRangeFlag ? TeleportingMod.settings.longRange_CooldownDuration : TeleportingMod.settings.shortRange_CooldownDuration) * 60).ToString()
							);
						}
					}
					this.cooldownComp.Set(cooldownTicks);
				}

				if (UseFuel)
				{
					this.refuelableComp.ConsumeFuel(TeleportBehavior.FuelCostToTravel(longRangeFlag, teleportData.distance));
				}
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				yield return gizmo;

			if (DebugSettings.godMode)
			{

				yield return GizmoHelper.MakeCommandAction(
					"TeleportConsole_Local_Debug",
					delegate
					{
						Logger.Debug("TeleportConsole:: called Godmode Gizmo: Short Range Teleport");
						TeleportBehavior.StartTeleportTargetting(false, this, cheat: true);
					}
				);

				yield return GizmoHelper.MakeCommandAction(
					"TeleportConsole_Global_Debug",
					delegate
					{
						Logger.Debug("TeleportConsole:: called Godmode Gizmo: Long Range Teleport");
						TeleportBehavior.StartTeleportTargetting(true, this, cheat: true);
					}
				);
			}
		}
	}
}// namespace alaestor_teleporting