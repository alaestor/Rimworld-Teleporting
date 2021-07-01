using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	[StaticConstructorOnStartup]
	class TeleportBehavior
	{
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

		public static bool ExecuteTeleport(Thing thing, Map destinationMap, IntVec3 destinationCell)
		{
			if (thing != null && destinationMap != null && destinationCell != null && destinationCell.IsValid)
			{
				Logger.Debug(
					"TeleportBehavior::ExecuteTeleport called",
					"From " + thing.Label + " at Tile,Cell: " + thing.Tile.ToString() + "," + thing.Position.ToString(),
					"To Tile,Cell: " + destinationMap.Tile.ToString() + "," + destinationCell.ToString()
				);

				if (thing.Map == destinationMap && thing.Position == destinationCell)
				{
					Logger.Debug(
						"ExecuteTeleport tried to move thing to it's own location",
						"Thing: " + thing.Label,
						"destinationMap: " + destinationMap.ToString(),
						"destinationCell: " + destinationCell.ToString()
					);
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
				Logger.Warning(
					"ExecuteTeleport failed",
					"thing: " + thing.ToString(),
					"destinationMap: " + destinationMap.ToString(),
					"destinationCell: " + destinationCell.ToString()
				);
				return false;
			}
		}

		public static void StartLongRangeTeleport(
			Thing originator,
			Action<int> onSuccess_Callback = null,
			CompRefuelable refuelableComp = null,
			bool cheat = false)
		{
			GlobalTargetInfo startingHere = CameraJumper.GetWorldTarget(originator);

			bool TargetHasLoadedMap(GlobalTargetInfo target)
			{
				return target.IsValid
					&& Find.WorldObjects.AnyMapParentAt(target.Tile)
					&& Find.WorldObjects.MapParentAt(target.Tile).Spawned
					&& Find.WorldObjects.MapParentAt(target.Tile).Map != null;
			}

			bool TargetIsWithinGlobalRangeLimit(GlobalTargetInfo target)
			{
				if (TeleportingMod.settings.enableGlobalRangeLimit)
				{
					int distanceToTarget = Find.WorldGrid.TraversalDistanceBetween(startingHere.Tile, target.Tile, true, int.MaxValue);
					return distanceToTarget <= TeleportingMod.settings.globalRangeLimit;
				}
				else return true;
			}

			int FuelCostToTravel(int tileDistance)
			{
				if (cheat)
				{
					return 0;
				}
				else if (tileDistance == 0)
				{
					return TeleportingMod.settings.longRange_FuelCost;
				}
				else
				{
					return ((int)Math.Ceiling(((double)tileDistance) / TeleportingMod.settings.longRange_FuelDistance)) * TeleportingMod.settings.longRange_FuelCost;
				}
			}

			int fromTile = 0;
			bool choosingDestination = false;

			// stuff for distance
			bool fuelMatters = TeleportingMod.settings.enableFuel && refuelableComp != null;
			bool fuelDistanceMatters = fuelMatters && TeleportingMod.settings.longRange_FuelDistance > 0;
			int remainingFuel = refuelableComp != null ? (int)Math.Floor(refuelableComp.Fuel) : 0;

			string ExtraLabelGetter(GlobalTargetInfo target)
			{
				if (!target.IsValid)
					return null;

				string label = "";

				if (TargetHasLoadedMap(target))
					label += target.Label;

				if (!cheat)
				{
					if (!TargetIsWithinGlobalRangeLimit(target))
					{
						if (label.Length != 0)
							label += "\n";

						label += "TeleportBehavior_Global_OutofRange".Translate();
					}
					else if (fuelDistanceMatters && choosingDestination)
					{ // replace console with fuel component "originator.TryGetComp...."?
						int fuelCost = FuelCostToTravel(Find.WorldGrid.TraversalDistanceBetween(fromTile, target.Tile, true, int.MaxValue));
						if (fuelCost > 0)
						{
							if (label.Length != 0)
								label += "\n";

							label += String.Format("Cost: {0} of {1}", fuelCost, remainingFuel); // TODO translate
							if (remainingFuel < fuelCost)
							{
								label += "\n" + "TeleportBehavior_Global_NotEnoughFuel".Translate();
							}
						}
					}
				}

				return label;
			}

			bool CanTargetTile(GlobalTargetInfo target)
			{
				if (TargetHasLoadedMap(target))
				{
					if (cheat) // ignore range and fuel limits
					{
						return true;
					}
					else if (TargetIsWithinGlobalRangeLimit(target))
					{
						if (fuelDistanceMatters && choosingDestination)
						{
							return remainingFuel >= FuelCostToTravel(
								Find.WorldGrid.TraversalDistanceBetween(fromTile, target.Tile, true, int.MaxValue));
						}
						else return true;
					}
				}
				return false;
			}

			void OnUpdate()
			{
				if (fuelDistanceMatters && choosingDestination)
				{
					int fuelRangeLimit =
						((int)Math.Floor(((double)remainingFuel) / TeleportingMod.settings.longRange_FuelCost))
						* TeleportingMod.settings.longRange_FuelDistance;

					GenDraw.DrawWorldRadiusRing(fromTile, fuelRangeLimit);
				}

				if (TeleportingMod.settings.enableGlobalRangeLimit)
				{
					GenDraw.DrawWorldRadiusRing(startingHere.Tile, TeleportingMod.settings.globalRangeLimit);
				}
			}

			Logger.DebugVerbose("TeleportBehavior::StartLongRangeTeleport \n\tonSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"));

			TeleportTargeter.StartChoosingGlobalThenLocal(
				startingFrom: startingHere,
				result_Callback: GotFrom_Callback,
				localTargetParams: TeleportBehavior.targetTeleportSubjects,
				localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
				//localTargetValidator: null,
				//globalCanTargetTiles: true,
				globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
				//globalCloseWorldTabWhenFinished: true,
				globalOnUpdate: OnUpdate,
				globalExtraLabelGetter: ExtraLabelGetter,
				globalTargetValidator: CanTargetTile);

			void GotFrom_Callback(GlobalTargetInfo fromTarget)
			{
				Logger.DebugVerbose("StartLongRangeTeleport Got \"from\" target:\t" + fromTarget.Label + " at Tile,Cell: " + fromTarget.Tile.ToString() + "," + fromTarget.Cell.ToString());
				fromTile = fromTarget.Tile;
				choosingDestination = true;
				TeleportTargeter.StartChoosingGlobalThenLocal(
					startingFrom: startingHere,
					result_Callback: GotTo_Callback,
					localTargetParams: TeleportBehavior.targetTeleportDestination,
					localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
					//localTargetValidator: null,
					//globalCanTargetTiles: true,
					globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
					//globalCloseWorldTabWhenFinished: true,
					globalOnUpdate: OnUpdate,
					globalExtraLabelGetter: ExtraLabelGetter,
					globalTargetValidator: TargetHasLoadedMap);

				void GotTo_Callback(GlobalTargetInfo toTarget)
				{
					Logger.DebugVerbose("StartLongRangeTeleport Got \"to\" target:\t\t" + toTarget.Label + " at Tile,Cell: " + toTarget.Tile.ToString() + "," + toTarget.Cell.ToString());
					if (ExecuteTeleport(fromTarget.Thing, toTarget.Map, toTarget.Cell))
					{
						int fuelCost = 0;

						if (fuelDistanceMatters)
						{
							fuelCost = FuelCostToTravel(Find.WorldGrid.TraversalDistanceBetween(fromTarget.Tile, toTarget.Tile, true, int.MaxValue));
						}
						else if (fuelMatters)
						{
							fuelCost = TeleportingMod.settings.longRange_FuelCost;
						}

						onSuccess_Callback?.Invoke(fuelCost);
					}
				}
			}
		}

		public static void StartShortRangeTeleport(Thing originator, Action<int> onSuccess_Callback = null)
		{

			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);
			Map localMap = originator.Map;

			Logger.DebugVerbose("TeleportBehavior::StartShortRangeTeleport \n\tonSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"));
			TeleportTargeter.StartChoosingLocal(globalTarget, GotFrom_Callback, TeleportBehavior.targetTeleportSubjects);

			void GotFrom_Callback(LocalTargetInfo fromTarget)
			{
				Logger.DebugVerbose("StartShortRangeTeleport Got \"from\" target:\t" + fromTarget.Label + " at Cell: " + fromTarget.Cell.ToString());
				TeleportTargeter.StartChoosingLocal(globalTarget, GotTo_Callback, TeleportBehavior.targetTeleportDestination);

				void GotTo_Callback(LocalTargetInfo toTarget)
				{
					Logger.DebugVerbose("StartShortRangeTeleport Got \"to\" target:\t\t" + toTarget.Label + " at Cell: " + toTarget.Cell.ToString());
					if (ExecuteTeleport(fromTarget.Thing, localMap, toTarget.Cell))
						onSuccess_Callback?.Invoke(TeleportingMod.settings.shortRange_FuelCost);
				}
			}
		}

		public static void StartTeleportTargetting(
			bool longRangeFlag,
			Thing originator,
			Action<int> onSuccess_Callback = null,
			CompRefuelable refuelableComp = null,
			bool cheat = false)
		{
			Logger.Debug(
				"TeleportBehavior::DoTeleport called",
				(longRangeFlag ? "Long Range (global targeting)" : "Short Range (local targeting)"),
				"Originator: " + (originator != null ? originator.ToString() : "null"),
				"onSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"),
				"Cheat: " + cheat.ToString()
			);

			if (longRangeFlag)
			{
				TeleportBehavior.StartLongRangeTeleport(originator, onSuccess_Callback, refuelableComp, cheat);
			}
			else
			{
				TeleportBehavior.StartShortRangeTeleport(originator, onSuccess_Callback); // to take refuelableComp?
			}
		}
	}
}// namespace alaestor_teleporting
