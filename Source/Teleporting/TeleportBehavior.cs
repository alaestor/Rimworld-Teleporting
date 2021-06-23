using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class TeleportBehavior
	{
		public static bool ExecuteTeleport(Thing thing, Map destinationMap, IntVec3 destinationCell)
		{
			if (thing != null && destinationMap != null && destinationCell != null && destinationCell.IsValid)
			{
				if (thing.Map == destinationMap && thing.Position == destinationCell)
				{
					return true;
				}
				else if (thing is Pawn pawn)
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
				else // simple item
				{
					thing.DeSpawn(DestroyMode.Vanish);
					GenSpawn.Spawn(thing, destinationCell, destinationMap);
				}
				return true;
			}
			else
			{
				// TODO better logging
				if (thing == null) Log.Error("Teleport recieved invalid parameters: thing was null");
				if (destinationMap == null) Log.Error("Teleport recieved invalid parameters: destinationMap was null");
				if (thing.Position == destinationCell) Log.Error("Teleport tried to move something to where it already is");
				return false;
			}
		}

		private static readonly Texture2D localTeleportMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true); // TODO

		private static readonly Texture2D globalTeleportMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true); // TODO

		private static readonly TargetingParameters targetTeleportSubjects = new TargetingParameters
		{
			canTargetPawns = true,
			canTargetAnimals = true,
			canTargetHumans = true,
			canTargetItems = true, // not working?
			canTargetBuildings = false
		};

		private static readonly TargetingParameters targetTeleportDestination = new TargetingParameters
		{
			canTargetPawns = false,
			canTargetBuildings = false,
			canTargetLocations = true
		};

		public static void StartLongRangeTeleport(Thing originator, Action onSuccess_Callback = null)
		{
			bool TargetHasLoadedMap(GlobalTargetInfo target)
			{
				return target.IsValid
					&& Find.WorldObjects.AnyMapParentAt(target.Tile)
					&& Find.WorldObjects.MapParentAt(target.Tile).Spawned
					&& Find.WorldObjects.MapParentAt(target.Tile).Map != null;
			}

			string ExtraLabelGetter(GlobalTargetInfo target)
			{
				if (!target.IsValid)
					return null;

				return TargetHasLoadedMap(target) ? "Has a map" : "No map"; // TODO
			}

			GlobalTargetInfo startingHere = CameraJumper.GetWorldTarget(originator);

			TeleportTargeter.StartChoosingGlobalThenLocal(
				startingFrom: startingHere,
				result_Callback: GotFrom_Callback,
				localTargetParams: TeleportBehavior.targetTeleportSubjects,
				localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
				//localTargetValidator: null,
				//globalCanTargetTiles: true,
				globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
				//globalCloseWorldTabWhenFinished: true,
				//globalOnUpdate: null,
				globalExtraLabelGetter: ExtraLabelGetter,
				globalTargetValidator: TargetHasLoadedMap);

			void GotFrom_Callback(GlobalTargetInfo fromTarget)
			{
				TeleportTargeter.StartChoosingGlobalThenLocal(
					startingFrom: startingHere,
					result_Callback: GotTo_Callback,
					localTargetParams: TeleportBehavior.targetTeleportDestination,
					localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
					//localTargetValidator: null,
					//globalCanTargetTiles: true,
					globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
					//globalCloseWorldTabWhenFinished: true,
					//globalOnUpdate: null,
					globalExtraLabelGetter: ExtraLabelGetter,
					globalTargetValidator: TargetHasLoadedMap);

				void GotTo_Callback(GlobalTargetInfo toTarget)
				{
					if (ExecuteTeleport(fromTarget.Thing, toTarget.Map, toTarget.Cell))
						onSuccess_Callback?.Invoke();
				}
			}
		}

		public static void StartLocalTeleport(Thing originator, Action onSuccess_Callback = null)
		{
			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);
			Map localMap = originator.Map;

			TeleportTargeter.StartChoosingLocal(globalTarget, GotFrom_Callback, TeleportBehavior.targetTeleportSubjects);

			void GotFrom_Callback(LocalTargetInfo fromTarget)
			{
				TeleportTargeter.StartChoosingLocal(globalTarget, GotTo_Callback, TeleportBehavior.targetTeleportDestination);

				void GotTo_Callback(LocalTargetInfo toTarget)
				{
					if (ExecuteTeleport(fromTarget.Thing, localMap, toTarget.Cell))
						onSuccess_Callback?.Invoke();
				}
			}
		}

		public static void StartTeleportTargetting(bool longRangeFlag, Thing originator, Action onSuccess_Callback = null)
		{
			if (longRangeFlag)
			{
				Log.Message("DoTeleport() LR");
				TeleportBehavior.StartLongRangeTeleport(originator, onSuccess_Callback);
			}
			else
			{
				Log.Message("DoTeleport() SR");
				TeleportBehavior.StartLocalTeleport(originator, onSuccess_Callback);
			}
		}
	}
}// namespace alaestor_teleporting
