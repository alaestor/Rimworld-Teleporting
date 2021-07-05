using RimWorld;
using Verse;

namespace alaestor_teleporting
{
	abstract class CompUseEffect_Teleport : CompUseEffect
	{
		protected bool canTeleportOthers = false;
		protected bool longRangeFlag = false;

		public bool CanTeleportOthers => canTeleportOthers;
		public bool LongRangeFlag => longRangeFlag;

		public CompUseEffect_Teleport()
		{
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
		}

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
				"CompUseEffect_Teleport: DoEffect called",
				"Item: " + parent.Label,
				"Pawn: " + (usedBy != null ? usedBy.ToString() + " - " + usedBy.Label : "null")
			);

			if (CanTeleportOthers)
			{
				TeleportBehavior.StartTeleportTargetting(LongRangeFlag, usedBy, delegate { SelfDestruct(); });
			}
			else
			{
				TeleportBehavior.StartTeleportPawn(LongRangeFlag, usedBy, delegate { SelfDestruct(); });
			}
		}
	}

	class CompUseEffect_Teleport_Local : CompUseEffect_Teleport
	{
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			base.longRangeFlag = false;
		}
	}

	class CompUseEffect_Teleport_Global : CompUseEffect_Teleport
	{
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			base.longRangeFlag = true;
		}
	}

		
}
