using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace alaestor_teleporting
{
	public class Building_TeleportPlatform : Building
	{
		private CompCooldown cooldownComp;
		private CompNameLinkable nameLinkableComp;
		private CompRefuelable refuelableComp;
		private CompPowerTrader powerComp;

		private bool HasCooldownComp => cooldownComp != null;
		private bool UseCooldown => TeleportingMod.settings.enableCooldown && TeleportingMod.settings.enableCooldown_Platform;
		private bool HasRefuelableComp => refuelableComp != null;
		private bool UseFuel => TeleportingMod.settings.enableFuel && TeleportingMod.settings.enablePlatformUnlinkFuelCost;

		public override void ExposeData()
		{
			base.ExposeData();
			//Scribe_Values.Look<bool>(ref this.hasStartedTargetting, "hasStartedTargetting", false);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.cooldownComp = this.GetComp<CompCooldown>();
			this.nameLinkableComp = this.GetComp<CompNameLinkable>();
			this.refuelableComp = this.GetComp<CompRefuelable>();
			this.powerComp = this.GetComp<CompPowerTrader>();
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

		public bool HasEnoughFuel => !UseFuel || (((int)refuelableComp.Fuel) >= 1);

		public void ConsumeFuel()
		{
			if (UseFuel)
				refuelableComp.ConsumeFuel(1);
		}

		public void Rename()
		{
			nameLinkableComp.BeginRename();
		}

		public void MakeLink()
		{
			nameLinkableComp.BeginMakeLink();
		}

		public bool Unlink()
		{
			if (nameLinkableComp.IsLinkedToSomething)
			{
				if (HasEnoughFuel || !UseFuel)
				{
					ConsumeFuel();
					nameLinkableComp.Unlink();
					return true;
				}
				else
				{
					Logger.Debug("Building_TeleportPlatform::Unlink: couldn't unlink",
						"IsLinkedToSomething: " + nameLinkableComp.IsLinkedToSomething.ToString(),
						"HasEnoughFuel: " + HasEnoughFuel.ToString()
					);
					return false;
				}
			}
			else
			{
				Logger.Warning("Building_TeleportPlatform::Unlink: Not linked to anything");
				return false;
			}
		}

		public void TryStartTeleport(Pawn usingPawn)
		{
			// fuel check
			if (UseFuel && HasRefuelableComp && !HasEnoughFuel)
			{
				Logger.Warning(
					"Building_TeleportPlatform::TryStartTeleport: " + usingPawn.Label + " tried to teleport but fuel isnt full",
					"From: \"" + nameLinkableComp.Name + "\"",
					"Fuel: " + refuelableComp.Fuel
				);
				return;
			}

			// cooldown check
			if (UseCooldown)
			{
				if (HasCooldownComp)
				{
					if (cooldownComp.IsOnCooldown)
					{
						Logger.Warning("Building_TeleportPlatform::TryStartTeleport: Tried to teleport but was on cooldown");
						return;
					}
				}
				else Logger.Error("Building_TeleportPlatform::TryStartTeleport: UseCooldown is true but cooldownComp is null");
			}

			if (nameLinkableComp.HasValidLinkedThing)
			{
				Thing destination = nameLinkableComp.LinkedThing;
				if (destination.Map != null && destination.InteractionCell.IsValid)
				{
					if (TeleportBehavior.ExecuteTeleport(usingPawn, destination.Map, destination.InteractionCell))
					{
						Logger.Debug(
							"Building_TeleportPlatform::TryStartTeleport: Teleported "
								+ usingPawn.Label
								+ " from \"" + nameLinkableComp.Name
								+ "\" to \"" + nameLinkableComp.GetNameOfLinkedLinkedThing + "\"",
							"Destination Map: " + destination.Map.ToString(),
							"Destination Cell: " + destination.InteractionCell.ToString()
						);

						// set cooldown
						if (UseCooldown)
						{
							if (HasCooldownComp)
							{
								cooldownComp.SetSeconds(TeleportingMod.settings.nameLinkable_CooldownDuration);
							}
							else Logger.Error("Building_TeleportPlatform::TryStartTeleport: UseCooldown is true but cooldownComp is null");
						}
					}
					else
					{
						Logger.Error("Building_TeleportPlatform::TryStartTeleport: ExecuteTeleport failed.");
					}
				}
				else
				{
					Logger.Error(
						"Building_TeleportPlatform::TryStartTeleport: destination map or cell was invalid!",
						"Map: " + destination.Map.ToString(),
						"Cell: " + destination.InteractionCell.ToString()
					);
				}
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
				else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
					return new FloatMenuOption((string)"CannotUseReason".Translate((NamedArgument)"IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Moving.label, myPawn.Named("PAWN"))), (Action)null);
				else if (UseCooldown && HasCooldownComp && cooldownComp.IsOnCooldown)
					return new FloatMenuOption(string.Format("On cooldown for {0} more second(s)", cooldownComp.SecondsRemaining), (Action)null); // TODO translate
				else if (UseFuel && HasRefuelableComp && !HasEnoughFuel)
					return new FloatMenuOption((string)"out of fuel".Translate(), (Action)null); // TODO translate
				else if (nameLinkableComp.IsLinkedToSomething && nameLinkableComp.HasInvalidLinkedThing)
					return new FloatMenuOption("Link broken: cannot find target", (Action)null);
				else if (this.CanUseNow)
					return (FloatMenuOption)null; // allow use
				Logger.Warning(myPawn.ToString() + "Could not use teleport pad for unknown reason.");
				return new FloatMenuOption("Cannot use now", (Action)null);
			}

			if (failureReason != null)
			{
				yield return failureReason;
			}
			else if (nameLinkableComp.HasValidLinkedThing)
			{

				string use_Label = "UseTeleportPlatform_Label".Translate();
				Action use_Action = (Action)(() =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportPlatform_TeleportToLink, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});

				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
					use_Label, use_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
			}
			else if (!nameLinkableComp.IsLinkedToSomething && nameLinkableComp.CanBeLinked)
			{
				string makeLink_Label = "LinkTeleportPlatform_Label".Translate();
				Action makeLink_Action = (Action)(() =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportPlatform_MakeLink, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});

				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
					makeLink_Label, makeLink_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				yield return gizmo;

			if (nameLinkableComp.CanBeNamed)
			{
				yield return GizmoHelper.MakeCommandAction(
					"TeleportPlatform_Rename",
					delegate
					{
						Rename();
						Logger.Debug("TeleportPlatform: called Gizmo: rename");
					}
					//icon: ContentFinder<Texture2D>.Get("UI/Commands/..."),
				);
			}

			if (nameLinkableComp.IsLinkedToSomething)
			{
				yield return GizmoHelper.MakeCommandAction(
					"TeleportPlatform_Unlink",
					delegate
					{
						Unlink();
						Logger.Debug("TeleportPlatform: called Gizmo: unlink");
					}
					//icon: ContentFinder<Texture2D>.Get("UI/Commands/..."),
				);

			}
		}
	}
}
