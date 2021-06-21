using System;
using Verse;

namespace alaestor_teleporting
{
	class TeleportBehavior
	{
		public static void ExecuteTeleport(TeleportTargeterData targeterData)
		{
			if (targeterData.isValid)
			{
				Log.Message( // TODO remove, testing
					"target: " + ((Pawn)targeterData.target).Name.ToString() + "\n"
					+ "map: " + targeterData.destinationMap.ToString() + " is home: "
						+ targeterData.destinationMap.IsPlayerHome.ToString() + "\n"
					+ "cell:" + targeterData.destinationCell.ToString() + "\n"
				);

				TeleportBehavior.ExecuteTeleport(targeterData.target, targeterData.destinationMap, targeterData.destinationCell);
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

		// TODO implement callback for cooldown
		// will probably need to make this class non-static
		// also, rename DoTeleport and void return

		public static void StartTeleportTargetting(bool longRangeFlag, Thing from, Action onSuccessCallback = null)
		{
			if (longRangeFlag)
			{
				Log.Message("DoTeleport() LR");
				TeleportTargeter.GlobalTeleport(from, GotTeleportTargets_Callback);
			}
			else
			{
				Log.Message("DoTeleport() SR");
				TeleportTargeter.LocalTeleport(from, GotTeleportTargets_Callback);
			}

			void GotTeleportTargets_Callback(TeleportTargeterData targeterData)
			{
				ExecuteTeleport(targeterData);

				onSuccessCallback?.Invoke();
			}
		}
	}
}// namespace alaestor_teleporting
