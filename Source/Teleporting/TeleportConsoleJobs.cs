using RimWorld;
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

	public class JobDriver_UseTeleportConsole_ShortRange : JobDriver
	{

		public override bool TryMakePreToilReservations(bool errorOnFailed)
			=> this.pawn.Reserve(this.job.targetA, this.job, errorOnFailed: errorOnFailed);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull<JobDriver_UseTeleportConsole_ShortRange>(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Func<Toil, bool>)
				(to => !((Building_TeleportConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseConsoleNow));

			Toil useTeleporterToil = new Toil();
			useTeleporterToil.initAction = (Action)(() =>
			{
				Pawn actor = useTeleporterToil.actor;
				Building_TeleportConsole console = (Building_TeleportConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (!console.CanUseConsoleNow)
					return;
				// actor.jobs.curJob.commTarget.TryOpenComms(actor);
				console.TryStartTeleport(actor, false);
			});
			yield return useTeleporterToil;
		}
	}

	public class JobDriver_UseTeleportConsole_LongRange : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
			=> this.pawn.Reserve(this.job.targetA, this.job, errorOnFailed: errorOnFailed);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull<JobDriver_UseTeleportConsole_LongRange>(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Func<Toil, bool>)
				(to => !((Building_TeleportConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseConsoleNow));

			Toil useTeleporterToil = new Toil();
			useTeleporterToil.initAction = (Action)(() =>
			{
				Pawn actor = useTeleporterToil.actor;
				Building_TeleportConsole console = (Building_TeleportConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (!console.CanUseConsoleNow)
					return;

				// actor.jobs.curJob.commTarget.TryOpenComms(actor);
				console.TryStartTeleport(actor, true);
			});
			yield return useTeleporterToil;
		}
	}
}// namespace alaestor_teleporting