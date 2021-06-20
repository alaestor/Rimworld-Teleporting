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

		public static void ExecuteTeleport(TeleportSelectionData tsd)
		{
			if (tsd.isValid)
			{
				Log.Message( // TODO remove, testing
					"target: " + ((Pawn)tsd.target).Name.ToString() + "\n"
					+ "map: " + tsd.destinationMap.ToString() + " is home: "
						+ tsd.destinationMap.IsPlayerHome.ToString() + "\n"
					+ "cell:" + tsd.destinationCell.ToString() + "\n"
				);

				TeleportBehavior.ExecuteTeleport(tsd.target, tsd.destinationMap, tsd.destinationCell);
			}
			else
			{
				// TODO debug log
			}
		}

		public static void ExecuteTeleport(Thing thing, Map destinationMap, IntVec3 destinationCell)
		{
			if (thing != null && destinationMap != null && thing.Position != destinationCell)
			{
				if (thing is Pawn pawn)
				{
					if (pawn.RaceProps.Animal)
					{
						pawn.DeSpawn(DestroyMode.Vanish);
						GenSpawn.Spawn(pawn, destinationCell, destinationMap);
					}
					else
					{
						if (pawn.IsColonist)
						{
							bool drafted = pawn.Drafted;
							pawn.drafter.Drafted = false;
							pawn.DeSpawn(DestroyMode.Vanish);
							GenSpawn.Spawn(pawn, destinationCell, destinationMap);
							pawn.drafter.Drafted = drafted;
						}
						else
						{
							pawn.DeSpawn(DestroyMode.Vanish);
							GenSpawn.Spawn(pawn, destinationCell, destinationMap);
						}
					}
				}
				/*
				else
				{
					thing.DeSpawn(DestroyMode.Vanish);
					GenSpawn.Spawn(thing, destination, destinationMap);
				}
				*/
			}
			else
			{
				if (thing == null) Log.Error("Teleport recieved invalid parameters: thing was null");
				if (destinationMap == null) Log.Error("Teleport recieved invalid parameters: destinationMap was null");
				if (thing.Position == destinationCell) Log.Error("Teleport tried to move something to where it already is");
			}
		}

		public static bool DoTeleport(bool longRangeFlag, Thing from)
		{
			if (longRangeFlag)
			{
				Log.Message("DoTeleport() LR");
				TeleportTargeter.GlobalTeleport(from, ExecuteTeleport);
			}
			else
			{
				Log.Message("DoTeleport() SR");
				TeleportTargeter.LocalTeleport(from, ExecuteTeleport);
			}

			// actually teleport

			return true;
		}
	}
}// namespace alaestor_teleporting
