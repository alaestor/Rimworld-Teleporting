﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace alaestor_teleporting
{
	[DefOf]
	public static class TeleporterDefOf
	{ // I'm 99% sure there's a better way to do this
		public static JobDef UseTeleportConsole_ShortRange;
		public static JobDef UseTeleportConsole_LongRange;
	}

	public abstract class JobDriver_UseTeleportConsole_Generic : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
			=> this.pawn.Reserve(this.job.targetA, this.job, errorOnFailed: errorOnFailed);

		protected JobCondition IsToilDone()
		{
			if (this.job.targetA != null
				&& this.job.targetA.IsValid
				&& this.job.targetA.Thing != null
				&& this.job.targetA.Thing is Building_TeleportConsole console
				&& console.IsDoneTargeting()) // is this enough checks?
			{
				Logger.DebugVerbose("useTeleporterToil finished");
				return JobCondition.Succeeded;
			}
			else
			{
				return JobCondition.Ongoing;
			}
		}
	}

	public class JobDriver_UseTeleportConsole_ShortRange : JobDriver_UseTeleportConsole_Generic
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull<JobDriver_UseTeleportConsole_ShortRange>(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Func<Toil, bool>)
				(to => !((Building_TeleportConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseConsoleNow));

			Toil useTeleporterToil = new Toil();
			useTeleporterToil.defaultCompleteMode = ToilCompleteMode.Never;
			useTeleporterToil.initAction = (Action)(() =>
			{
				Pawn actor = useTeleporterToil.actor;
				Building_TeleportConsole console = (Building_TeleportConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (!console.CanUseConsoleNow)
					return;

				console.TryStartTeleport(actor, false);
				console.hasStartedTargetting = true;
				Logger.DebugVerbose("Pawn " + actor.Label + " began JobDriver_UseTeleportConsole_ShortRange at ThindID " + console.ThingID.ToString());
			});
			useTeleporterToil.AddEndCondition(IsToilDone);
			yield return useTeleporterToil;
		}
	}

	public class JobDriver_UseTeleportConsole_LongRange : JobDriver_UseTeleportConsole_Generic
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull<JobDriver_UseTeleportConsole_LongRange>(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Func<Toil, bool>)
				(to => !((Building_TeleportConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseConsoleNow));

			Toil useTeleporterToil = new Toil();
			useTeleporterToil.defaultCompleteMode = ToilCompleteMode.Never;
			useTeleporterToil.initAction = (Action)(() =>
			{
				Pawn actor = useTeleporterToil.actor;
				Building_TeleportConsole console = (Building_TeleportConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (!console.CanUseConsoleNow)
					return;

				console.TryStartTeleport(actor, true);
				console.hasStartedTargetting = true;
				Logger.DebugVerbose("Pawn " + actor.Label + " began JobDriver_UseTeleportConsole_LongRange at ThindID " + console.ThingID.ToString());
			});
			useTeleporterToil.AddEndCondition(IsToilDone);
			yield return useTeleporterToil;
		}
	}
}// namespace alaestor_teleporting