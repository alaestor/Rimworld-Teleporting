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
	public class Building_TeleportConsole : Building_WorkTable
	{
		private CompPowerTrader powerComp;
		private CompRefuelableAtHome refuelableComp;
		private CompCooldown cooldownComp;

		private bool UseCooldown => TeleportingMod.settings.enableCooldown && TeleportingMod.settings.enableCooldown_Console;
		private bool UseFuel => TeleportingMod.settings.enableFuel;

		public bool HasResearchFor_Teleport => Find.ResearchManager.GetProgress(MyDefs.research_tier_4_0) >= 1.0;
		public bool HasResearchFor_Teleport_ShortRange => HasResearchFor_Teleport;
		public bool HasResearchFor_Teleport_LongRange => HasResearchFor_Teleport;


		public bool hasStartedTargetting = false;


		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			refuelableComp = GetComp<CompRefuelableAtHome>();
			cooldownComp = GetComp<CompCooldown>();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref hasStartedTargetting, "hasStartedTargetting", false);
		}

		public bool IsDoneTargeting()
		{
			if (hasStartedTargetting && !(Find.Targeter.IsTargeting || Find.WorldTargeter.IsTargeting))
			{
				hasStartedTargetting = false;
				return true;
			}
			else return false;
		}

		public bool CanUseNow
		{
			get
			{
				if (Spawned && Map.gameConditionManager.ElectricityDisabled)
					return false;
				return powerComp == null || powerComp.PowerOn;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			FloatMenuOption failureReason = GetFailureReason();
			FloatMenuOption GetFailureReason()
			{
				if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
					return new FloatMenuOption("CannotUseNoPath".Translate(), null);
				else if (Spawned && Map.gameConditionManager.ElectricityDisabled)
					return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
				else if (powerComp != null && !powerComp.PowerOn)
					return new FloatMenuOption("CannotUseNoPower".Translate(), null);
				else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Manipulation.label, myPawn.Named("PAWN"))), null);
				else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
					return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Moving.label, myPawn.Named("PAWN"))), null);
				else if (TeleportingMod.settings.enableCooldown && cooldownComp != null && cooldownComp.IsOnCooldown)
					return new FloatMenuOption("Teleporting_IsOnCooldown".Translate(), null);
				else if (CanUseNow)
					return null; // allow use
				Logger.Warning(myPawn.ToString() + "Could not use teleport console for unknown reason.");
				return new FloatMenuOption("Cannot use now", null);
			}

			if (failureReason != null)
			{
				yield return failureReason;
			}
			else
			{
				string short_Label = "Teleporting_ShortRangeTeleport_Label".Translate();
				Action short_Action = () =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.alaestor_teleporting_UseTeleportConsole_ShortRange, this);
					myPawn.jobs.TryTakeOrderedJob(job);
				};

				string long_Label = "Teleporting_LongRangeTeleport_Label".Translate();
				Action long_Action = () =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.alaestor_teleporting_UseTeleportConsole_LongRange, this);
					myPawn.jobs.TryTakeOrderedJob(job);
				};

				if (UseFuel)
				{
					if (refuelableComp != null)
					{
						if (HasResearchFor_Teleport_ShortRange)
						{
							if (refuelableComp.Fuel >= TeleportingMod.settings.shortRange_FuelCost)
							{
								yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(short_Label, short_Action, MenuOptionPriority.Default), myPawn, this);
							}
							else yield return new FloatMenuOption("Teleporting_shortRange_NotEnoughFuel".Translate(), null); // restring
						}

						if (HasResearchFor_Teleport_LongRange)
						{
							if (refuelableComp.Fuel >= TeleportingMod.settings.longRange_FuelCost)
							{
								yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(long_Label, long_Action, MenuOptionPriority.Default), myPawn, this);
							}
							else yield return new FloatMenuOption("Teleporting_longRange_NotEnoughFuel".Translate(), null); // restring
						}
					}
					else Logger.Error("Teleporting: fuel is enabled but refuelableComp is null", refuelableComp.ToString());
				}
				else
				{
					if (HasResearchFor_Teleport_ShortRange)
					{
						yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(short_Label, short_Action, MenuOptionPriority.Default), myPawn, this);
					}

					if (HasResearchFor_Teleport_LongRange)
					{
						yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(long_Label, long_Action, MenuOptionPriority.Default), myPawn, this);
					}
				}
			}
		}

		public void TryStartTeleport(Pawn controllingPawn, bool longRangeFlag)
		{
			if (UseCooldown)
			{
				if (cooldownComp != null)
				{
					if (cooldownComp.IsOnCooldown)
					{
						Logger.Warning("Building_TeleportConsole::TryStartTeleport: Tried to start a teleport but console was on cooldown");
						return;
					}
				}
				else
				{
					Logger.Error("Building_TeleportConsole::TryStartTeleport: cooldown is enabled but cooldownComp is null");
					return;
				}
			}

			if ((longRangeFlag && !HasResearchFor_Teleport_LongRange) 
				|| (!longRangeFlag && !HasResearchFor_Teleport_ShortRange))
			{
				Logger.Error(
					"Building_TeleportConsole::TryStartTeleport: tried to teleport but is missing required research",
					"longRangeFlag: " + longRangeFlag.ToString(),
					"HasResearchFor_Teleport_LongRange: " + HasResearchFor_Teleport_LongRange.ToString(),
					"HasResearchFor_Teleport_ShortRange: " + HasResearchFor_Teleport_ShortRange.ToString()
				);
				return;
			}


			TeleportBehavior.StartTeleportTargetting(longRangeFlag, this, onTeleportSuccess, (int)refuelableComp.Fuel);

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
					cooldownComp.Set(cooldownTicks);
				}

				if (UseFuel)
				{
					refuelableComp.ConsumeFuel(TeleportBehavior.FuelCostToTravel(longRangeFlag, teleportData.distance));
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