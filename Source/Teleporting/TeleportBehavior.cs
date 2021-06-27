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

		public static void StartLongRangeTeleport(Thing originator, Action<int> onSuccess_Callback = null, bool cheat = false)
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

			int fromTile = 0;
			bool choosingDestination = false;

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

						label += "Out of Range"; // TODO translate
					}
					else if (choosingDestination && TeleportingMod.settings.longRange_FuelDistance > 0 && originator is Building_TeleportConsole console)
					{ // replace console with fuel component "originator.TryGetComp...."?
						int fuelCost = console.FuelCostToTravel(Find.WorldGrid.TraversalDistanceBetween(fromTile, target.Tile, true, int.MaxValue));
						if (fuelCost > 0)
						{
							if (label.Length != 0)
								label += "\n";

							label += String.Format("Cost: {0} of {1}", fuelCost, console.RemainingFuel()); // TODO translate
							if (!console.CanConsumeFuelAmount(fuelCost))
							{
								label += "\nNot enough fuel!";
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
						if (choosingDestination
							&& TeleportingMod.settings.longRange_FuelDistance > 0
							&& originator is Building_TeleportConsole console)
						{
							return console.CanConsumeFuelAmount(console.FuelCostToTravel(
								Find.WorldGrid.TraversalDistanceBetween(fromTile, target.Tile, true, int.MaxValue)));
						}
						else return true;
					}
				}
				return false;
			}

			void OnUpdate()
			{
				if (choosingDestination
					&& TeleportingMod.settings.longRange_FuelDistance > 0
					&& originator is Building_TeleportConsole console)
				{
					int fuelRangeLimit =
						((int)Math.Floor(
							((double)console.RemainingFuel()) / TeleportingMod.settings.longRange_FuelCost))
						* TeleportingMod.settings.longRange_FuelDistance;

					GenDraw.DrawWorldRadiusRing(fromTile, fuelRangeLimit);
				}

				if (TeleportingMod.settings.enableGlobalRangeLimit)
				{
					GenDraw.DrawWorldRadiusRing(startingHere.Tile, TeleportingMod.settings.globalRangeLimit);
				}
			}

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
					if (ExecuteTeleport(fromTarget.Thing, toTarget.Map, toTarget.Cell))
					{
						onSuccess_Callback?.Invoke(Find.WorldGrid.TraversalDistanceBetween(fromTarget.Tile, toTarget.Tile, true, int.MaxValue));
					}
				}
			}
		}

		public static void StartLocalTeleport(Thing originator, Action<int> onSuccess_Callback = null)
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
						onSuccess_Callback?.Invoke(0);
				}
			}
		}

		public static void StartTeleportTargetting(bool longRangeFlag, Thing originator, Action<int> onSuccess_Callback = null, bool cheat = false)
		{
			if (longRangeFlag)
			{
				Log.Message("DoTeleport() LR");
				TeleportBehavior.StartLongRangeTeleport(originator, onSuccess_Callback, cheat);
			}
			else
			{
				Log.Message("DoTeleport() SR");
				TeleportBehavior.StartLocalTeleport(originator, onSuccess_Callback);
			}
		}
	}
}// namespace alaestor_teleporting
