using Verse;

namespace alaestor_teleporting
{
	class TeleportBehavior
	{
		// protected static ??? selectMap()
		// protected static ??? selectPawn()
		// protected static ??? selectPos()

		/*
		struct TeleportSolution
		{
			from_map;
			from_pawn;
			to_map;
			to_pos;
		};
		*/

		protected static void LongRangeSequence()
		{
			// ?? from_map = selectMap();
			// from_pawn = selectPawn(); // selectPawn(map)?
			// to_map = selectMap();
			// to_pos = selectPos(); // selectTile(map)?
			// move from, to
		}

		protected static void ShortRangeSequence()
		{
			// from_pawn = selectPawn();
			// to_pos = selectPos();
			// move from, to
		}

		public static bool DoTeleport(bool longRangeFlag)
		{
			if (longRangeFlag)
			{
				Log.Message("DoTeleport() LR");
				LongRangeSequence();
			}
			else
			{
				Log.Message("DoTeleport() SR");
				ShortRangeSequence();
			}

			// actually teleport

			return true;
		}
	}
}// namespace alaestor_teleporting
