using RimWorld;
using Verse;

namespace alaestor_teleporting
{
	class CompUseEffect_LocalTeleport : CompUseEffect
	{
		private bool CanTeleportOthers => false;

		public override void PostExposeData()
		{
			base.PostExposeData();
		}

		public void SelfDestruct()
		{
			Logger.DebugVerbose(parent.Label + " self destructed");
			this.parent.SplitOff(1).Destroy();
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			Logger.Debug(
				"CompUseEffect_LocalTeleport: DoEffect called",
				"Item: " + parent.Label,
				"Pawn: " + (usedBy != null ? usedBy.ToString() + " - " + usedBy.Label : "null")
			);

			if (CanTeleportOthers)
			{
				TeleportBehavior.StartTeleportTargetting(false, usedBy, delegate { SelfDestruct(); });
			}
			else
			{
				TeleportBehavior.StartShortRangeTeleportPawn(usedBy, delegate { SelfDestruct(); });
			}
		}
	}
}
