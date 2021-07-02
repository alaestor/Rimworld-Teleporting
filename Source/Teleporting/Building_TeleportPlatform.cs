using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace alaestor_teleporting
{
	public class Building_TeleportPlatform : Building
	{
		private CompPowerTrader powerComp;
		private CompNameLinkable nameLinkableComp;

		public override void ExposeData()
		{
			base.ExposeData();
			//Scribe_Values.Look<bool>(ref this.hasStartedTargetting, "hasStartedTargetting", false);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = this.GetComp<CompPowerTrader>();
			this.nameLinkableComp = this.GetComp<CompNameLinkable>();
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
				else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
					return new FloatMenuOption((string)"CannotUseReason".Translate((NamedArgument)"IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Moving.label, myPawn.Named("PAWN"))), (Action)null);
				/*
				else if (TeleportingMod.settings.enableCooldown && this.cooldownComp != null && this.cooldownComp.IsOnCooldown)
					return new FloatMenuOption("IsOnCooldown".Translate(), (Action)null);
				*/
				else if (this.CanUseNow)
					return (FloatMenuOption)null; // allow use
				Logger.Warning(myPawn.ToString() + "Could not use teleport pad for unknown reason.");
				return new FloatMenuOption("Cannot use now", (Action)null);
			}

			if (failureReason != null)
			{
				yield return failureReason;
			}
			else
			{
				//if valid link
				string use_Label = "UseTeleportPlatform_Label".Translate();
				Action use_Action = (Action)(() =>
				{
					Job job = JobMaker.MakeJob(TeleporterDefOf.UseTeleportPlatform_TeleportToLink, (LocalTargetInfo)(Thing)this);
					myPawn.jobs.TryTakeOrderedJob(job);
				});

				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
					use_Label, use_Action, MenuOptionPriority.Default), myPawn, (LocalTargetInfo)(Thing)this);
			}
		}

		public void TryStartTeleport(Pawn usingPawn)
		{
			/*
			if (TeleportingMod.settings.enableCooldown)
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
			*/

			if (nameLinkableComp.IsLinkedToSomething)
			{
				Thing destination = nameLinkableComp.LinkedThing;
				if (destination.Map != null && destination.InteractionCell.IsValid)
				{
					if (TeleportBehavior.ExecuteTeleport(usingPawn, destination.Map, destination.InteractionCell))
					{
						Logger.Debug(
							"Building_TeleportPlatform::TryStartTeleport: Teleported "
								+ usingPawn.Label
								+ " from \"" + nameLinkableComp.Name ?? "(unnamed)"
								+ "\" to \"" + nameLinkableComp.LinkedName ?? "(unnamed)" + "\"",
							"Destination Map: " + destination.Map.ToString(),
							"Destination Cell: " + destination.InteractionCell.ToString()
						);
						/*
						if (TeleportingMod.settings.enableCooldown)
						{
							int cooldownTicks = TeleportingMod.settings.shortRange_CooldownDuration * 60;
						}
						*/
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
	}
}
