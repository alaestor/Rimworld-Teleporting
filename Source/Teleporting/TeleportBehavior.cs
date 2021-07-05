using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace alaestor_teleporting
{
	[StaticConstructorOnStartup]
	class TeleportBehavior
	{
		public static readonly Texture2D localTeleportMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true); // TODO

		public static readonly Texture2D globalTeleportMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true); // TODO

		public static readonly TargetingParameters targetTeleportSubjects = new TargetingParameters
		{
			canTargetPawns = true,
			canTargetAnimals = true,
			canTargetHumans = true,
			canTargetItems = true, // not working?
			canTargetBuildings = false
		};

		public static readonly TargetingParameters targetTeleportDestination = new TargetingParameters
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
					"TeleportBehavior::ExecuteTeleport: called",
					"From Target: " + thing.Label,
					"\tFrom Tile: " + thing.Tile.ToString(),
					"\tFrom Cell: " + thing.Position.ToString(),
					"To Tile: " + destinationMap.Tile.ToString(),
					"To Cell: " + destinationCell.ToString()
				);

				if (thing.Map == destinationMap && thing.Position == destinationCell)
				{
					Logger.Debug("TeleportBehavior::ExecuteTeleport: tried to move thing to it's own location");
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
							var curJobDef = pawn.CurJobDef;
							if (curJobDef == TeleporterDefOf.UseTeleportConsole_LongRange
								|| curJobDef == TeleporterDefOf.UseTeleportConsole_ShortRange)
							{
								if (pawn.jobs.curJob?.targetA.Thing is Building_TeleportConsole console)
								{
									//console.hasStartedTargetting = false;
									console.IsDoneTargeting();
								}

								pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
								Logger.DebugVerbose("TeleportBehavior::ExecuteTeleport: Target was doing teleport job toil, now terminated");
							}

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
					"TeleportBehavior::ExecuteTeleport: failed",
					"Thing null:" + (thing == null ? true : false).ToString(),
					"destinationMap null:" + (destinationMap == null ? true : false).ToString(),
					"destinationCell null:" + (destinationCell == null ? true : false).ToString(),
					"destinationCell invalid:" + (destinationCell == null ? "null" : destinationCell.IsValid.ToString())
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

				if (TeleportTargeter.TargetHasLoadedMap(target))
					label += target.Label;

				if (!cheat)
				{
					if (!TeleportTargeter.TargetIsWithinGlobalRangeLimit(startingHere.Tile, target.Tile))
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
				if (TeleportTargeter.TargetHasLoadedMap(target))
				{
					if (cheat) // ignore range and fuel limits
					{
						return true;
					}
					else if (TeleportTargeter.TargetIsWithinGlobalRangeLimit(startingHere.Tile, target.Tile))
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

			Logger.DebugVerbose("TeleportBehavior::StartLongRangeTeleport: called",
				"originator: " + (originator != null ? originator.Label : "null"),
				"onSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"),
				"remainingFuel: " + (refuelableComp != null ? refuelableComp.ToString() : "null"),
				"Cheat: " + cheat.ToString()
			);

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
				Logger.DebugVerbose(
					"TeleportBehavior::StartChoosingGlobalThenLocal: finished choosing \"from\" target",
					"Target: " + fromTarget.Label,
					"Tile: " + fromTarget.Tile.ToString(),
					"Cell: " + fromTarget.Cell.ToString()
				);

				fromTile = fromTarget.Tile;
				choosingDestination = true;
				TeleportTargeter.StartChoosingGlobalThenLocal(
					startingFrom: startingHere,
					result_Callback: FinishedChoosing_To,
					localTargetParams: TeleportBehavior.targetTeleportDestination,
					localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
					//localTargetValidator: null,
					//globalCanTargetTiles: true,
					globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
					//globalCloseWorldTabWhenFinished: true,
					globalOnUpdate: OnUpdate,
					globalExtraLabelGetter: ExtraLabelGetter,
					globalTargetValidator: TeleportTargeter.TargetHasLoadedMap);

				void FinishedChoosing_To(GlobalTargetInfo toTarget)
				{
					Logger.DebugVerbose(
						"TeleportBehavior::StartChoosingGlobalThenLocal: finished choosing \"to\" target",
						"Target: " + toTarget.Label,
						"Tile: " + toTarget.Tile.ToString(),
						"Cell: " + toTarget.Cell.ToString()
					);

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

			Logger.DebugVerbose("TeleportBehavior::StartShortRangeTeleport: called",
				"originator: " + (originator != null ? originator.Label : "null"),
				"onSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null")
			);
			
			TeleportTargeter.StartChoosingLocal(globalTarget, FinishedChoosing_From, TeleportBehavior.targetTeleportSubjects);

			void FinishedChoosing_From(LocalTargetInfo fromTarget)
			{
				Logger.DebugVerbose(
					"TeleportBehavior::StartShortRangeTeleport: finished choosing \"from\" target",
					"Target: " + fromTarget.Label,
					"Cell: " + fromTarget.Cell.ToString()
				);
				
				TeleportTargeter.StartChoosingLocal(globalTarget, FinishedChoosing_To, TeleportBehavior.targetTeleportDestination);

				void FinishedChoosing_To(LocalTargetInfo toTarget)
				{
					Logger.DebugVerbose(
						"TeleportBehavior::StartShortRangeTeleport: finished choosing \"to\" target",
						"Target: " + toTarget.Label,
						"Cell: " + toTarget.Cell.ToString()
					);

					if (ExecuteTeleport(fromTarget.Thing, localMap, toTarget.Cell))
						onSuccess_Callback?.Invoke(TeleportingMod.settings.shortRange_FuelCost);
				}
			}
		}

		public static void StartShortRangeTeleportPawn(Pawn pawn, Action onSuccess_Callback = null, bool cheat = false)
		{
			Logger.DebugVerbose("TeleportBehavior::StartShortRangeTeleportPawn: called",
				"Pawn: " + (pawn != null ? pawn.Label : "null"),
				"onSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"),
				"Cheat: " + cheat.ToString()
			);

			if (pawn != null && pawn.Map != null)
			{
				TeleportTargeter.StartChoosingLocal(
					startingFrom: pawn,
					result_Callback: FinishedChoosing_To,
					targetParams: targetTeleportDestination,
					mouseAttachment: localTeleportMouseAttachment);

				void FinishedChoosing_To(LocalTargetInfo destination)
				{
					Logger.DebugVerbose(
						"TeleportBehavior::StartShortRangeTeleportPawn: finished choosing \"to\" target",
						"Target: " + destination.Label,
						"Cell: " + destination.Cell.ToString()
					);

					if (ExecuteTeleport(pawn, pawn.Map, destination.Cell))
						onSuccess_Callback?.Invoke();
				}
			}
			else Logger.Error("TeleportBehavior::StartShortRangeTeleportPawn: invalid pawn");
		}

		public static void StartLongRangeTeleportPawn(Pawn pawn, Action onSuccess_Callback = null, bool cheat = false)
		{
			Logger.DebugVerbose("TeleportBehavior::StartLongRangeTeleportPawn: called",
				"Pawn: " + (pawn != null ? pawn.Label : "null"),
				"onSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"),
				"Cheat: " + cheat.ToString()
			);

			bool fuelDistanceMatters =
							TeleportingMod.settings.enableFuel
							&& TeleportingMod.settings.longRange_FuelDistance > 0;

			GlobalTargetInfo startingHere = CameraJumper.GetWorldTarget(pawn);

			string ExtraLabelGetter(GlobalTargetInfo target)
			{
				if (!target.IsValid)
					return null;

				string label = "";

				if (TeleportTargeter.TargetHasLoadedMap(target))
					label += target.Label;

				if (!cheat)
				{
					if (!TeleportTargeter.TargetIsWithinGlobalRangeLimit(startingHere.Tile, target.Tile))
					{
						if (label.Length != 0)
							label += "\n";

						label += "TeleportBehavior_Global_OutofRange".Translate();
					}
					else if (fuelDistanceMatters)
					{
						int distance = Find.WorldGrid.TraversalDistanceBetween(startingHere.Tile, target.Tile, true, int.MaxValue);
						if (distance > TeleportingMod.settings.longRange_FuelDistance)
						{
							if (label.Length != 0)
								label += "\n";
							label += "TeleportBehavior_Global_NotEnoughFuel".Translate();
						}
					}
				}

				return label;
			}

			void OnUpdate()
			{
				if (!cheat)
				{
					if (fuelDistanceMatters)
					{
						int fuelRangeLimit = TeleportingMod.settings.longRange_FuelDistance;
						GenDraw.DrawWorldRadiusRing(startingHere.Tile, fuelRangeLimit);
					}

					if (TeleportingMod.settings.enableGlobalRangeLimit)
					{
						GenDraw.DrawWorldRadiusRing(startingHere.Tile, TeleportingMod.settings.globalRangeLimit);
					}
				}
			}

			TeleportTargeter.StartChoosingGlobalThenLocal(
				startingFrom: startingHere,
				result_Callback: FinishedChoosing_To,
				localTargetParams: TeleportBehavior.targetTeleportDestination,
				localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
				globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
				globalOnUpdate: OnUpdate,
				globalExtraLabelGetter: ExtraLabelGetter,
				globalTargetValidator: TeleportTargeter.TargetHasLoadedMap
			);

			void FinishedChoosing_To(GlobalTargetInfo destination)
			{
				Logger.DebugVerbose(
					"TeleportBehavior::StartLongRangeTeleportPawn: finished choosing \"to\" target",
					"Target: " + destination.Label,
					"Tile: " + destination.Tile.ToString(),
					"Cell: " + destination.Cell.ToString()
				);

				if (ExecuteTeleport(pawn, destination.Map, destination.Cell))
					onSuccess_Callback?.Invoke();
			}
		}

		public static void StartTeleportPawn(
			bool longRangeFlag,
			Pawn pawn,
			Action onSuccess_Callback = null,
			bool cheat = false)
		{
			Logger.Debug(
				"TeleportBehavior::StartTeleportPawn: called",
				(longRangeFlag ? "Long Range (global targeting)" : "Short Range (local targeting)"),
				"Pawn: " + (pawn != null ? pawn.Label : "null"),
				"onSuccess_Callback: " + (onSuccess_Callback != null ? onSuccess_Callback.Method.Name : "null"),
				"Cheat: " + cheat.ToString()
			);

			if (longRangeFlag)
			{
				StartLongRangeTeleportPawn(pawn, onSuccess_Callback, cheat);
			}
			else
			{
				StartShortRangeTeleportPawn(pawn, onSuccess_Callback, cheat);
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
				"TeleportBehavior::StartTeleportTargetting: called",
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
