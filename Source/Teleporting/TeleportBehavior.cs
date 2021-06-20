using Verse;

namespace alaestor_teleporting
{
	class TeleportBehavior
	{
		// protected static ??? selectMap()
		// protected static ??? selectPawn()
		// protected static ??? selectPos()

		//static private bool ChoseWorldTarget(GlobalTargetInfo target) => ;

		/*
		struct TeleportSolution
		{
			from_map;
			from_pawn;
			to_map;
			to_pos;
		};
		*/

		protected static void DoLongRangeTeleport(TeleportTargetSolution tts)
		{
			// ?? from_map = selectMap();
			// from_pawn = selectPawn(); // selectPawn(map)?
			// to_map = selectMap();
			// to_pos = selectPos(); // selectTile(map)?
			// move from, to
			Log.Message("From: tile " + tts.from.global.Tile.ToString() + " cell " + tts.from.local.Cell.ToString());
			Log.Message("From: tile " + tts.to.global.Tile.ToString() + " cell " + tts.to.local.Cell.ToString());
		}

		protected static void DoShortRangeTeleport(TeleportTargetSolution tts)
		{
			// from_pawn = selectPawn();
			// to_pos = selectPos();
			// move from, to
			Log.Message("From: tile " + tts.from.global.Tile.ToString() + " cell " + tts.from.local.Cell.ToString());
			Log.Message("From: tile " + tts.to.global.Tile.ToString() + " cell " + tts.to.local.Cell.ToString());
		}

		public static bool DoTeleport(bool longRangeFlag, Thing from)
		{
			if (longRangeFlag)
			{
				Log.Message("DoTeleport() LR");
				TeleportTargeter.GlobalTeleport(from, DoLongRangeTeleport);
			}
			else
			{
				Log.Message("DoTeleport() SR");
				TeleportTargeter.LocalTeleport(from, DoShortRangeTeleport);
			}

			// actually teleport

			return true;
		}
	}
}// namespace alaestor_teleporting
