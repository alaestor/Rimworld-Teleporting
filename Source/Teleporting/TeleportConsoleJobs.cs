using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace alaestor_teleporting
{
	public abstract class JobDriver_UseTeleportConsole_Generic : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
			=> pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);

		protected JobCondition IsToilDone()
		{
			if (job?.targetA.Thing is Building_TeleportConsole console
				&& console.IsDoneTargeting())
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
			this.FailOnBurningImmobile<JobDriver_UseTeleportConsole_ShortRange>(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(
				to => !((Building_TeleportConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseNow);

			Toil useTeleporterToil = new Toil();
			useTeleporterToil.defaultCompleteMode = ToilCompleteMode.Never;
			useTeleporterToil.initAction = () =>
			{
				Pawn actor = useTeleporterToil.actor;
				Building_TeleportConsole console = (Building_TeleportConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (!console.CanUseNow)
					return;

				console.hasStartedTargetting = true;
				console.TryStartTeleport(actor, false);
				Logger.DebugVerbose("Pawn " + actor.Label + " began JobDriver_UseTeleportConsole_ShortRange at ThindID " + console.ThingID.ToString());
			};
			useTeleporterToil.AddEndCondition(IsToilDone);
			yield return useTeleporterToil;
		}
	}

	public class JobDriver_UseTeleportConsole_LongRange : JobDriver_UseTeleportConsole_Generic
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull<JobDriver_UseTeleportConsole_LongRange>(TargetIndex.A);
			this.FailOnBurningImmobile<JobDriver_UseTeleportConsole_LongRange>(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(
				to => !((Building_TeleportConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseNow);

			Toil useTeleporterToil = new Toil();
			useTeleporterToil.defaultCompleteMode = ToilCompleteMode.Never;
			useTeleporterToil.initAction = () =>
			{
				Pawn actor = useTeleporterToil.actor;
				Building_TeleportConsole console = (Building_TeleportConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (!console.CanUseNow)
					return;

				console.hasStartedTargetting = true;
				console.TryStartTeleport(actor, true);
				Logger.DebugVerbose("Pawn " + actor.Label + " began JobDriver_UseTeleportConsole_LongRange at ThindID " + console.ThingID.ToString());
			};
			useTeleporterToil.AddEndCondition(IsToilDone);
			yield return useTeleporterToil;
		}
	}
}// namespace alaestor_teleporting