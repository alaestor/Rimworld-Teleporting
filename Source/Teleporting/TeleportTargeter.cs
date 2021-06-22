using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	struct TeleportTargeterData
	{
		public Map destinationMap;
		public IntVec3 destinationCell;
		public Thing target;
		public bool isValid
		{
			get
			{
				return target != null
					&& destinationMap != null
					&& destinationCell != null
					&& destinationCell.IsValid;
			}
		}
	}

	class TeleportTargeter
	{
		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);

		private static void StartChoosingLocal(
			GlobalTargetInfo globalTarget,
			Action<GlobalTargetInfo> callback,
			TargetingParameters targetParams,
			Func<LocalTargetInfo, bool> CanTargetValidator = null)
		{
			Map selectedMap = Find.WorldObjects.MapParentAt(globalTarget.Tile).Map;
			//Map Originator = targetOriginator.Map;
			Current.Game.CurrentMap = selectedMap;
			CameraJumper.TryHideWorld();

			Find.Targeter.BeginTargeting(
				targetParams: targetParams,
				action: ChoseLocalTarget_Callback,
				highlightAction: null,
				targetValidator: CanTargetValidator,
				mouseAttachment: TeleportTargeter.TargeterMouseAttachment
			);

			void ChoseLocalTarget_Callback(LocalTargetInfo localTarget)
			{
				if (localTarget.IsValid)
				{
					GlobalTargetInfo targetOut = localTarget.ToGlobalTargetInfo(selectedMap);
					Find.WorldTargeter.StopTargeting();
					callback(targetOut);
				}
			}
		}

		private static void StartChoosingMap(
			GlobalTargetInfo startingFrom,
			Action<GlobalTargetInfo> callback,
			TargetingParameters localTargetParams,
			Func<GlobalTargetInfo, string> ExtraLabelGetter = null,
			Func<GlobalTargetInfo, bool> CanTargetValidator = null)
		{
			CameraJumper.TryJump(startingFrom);
			Find.WorldSelector.ClearSelection();

			Find.WorldTargeter.BeginTargeting_NewTemp(
				action: ChoseGlobalTarget_Callback,
				canTargetTiles: true,
				mouseAttachment: TeleportTargeter.TargeterMouseAttachment,
				closeWorldTabWhenFinished: true,
				onUpdate: null, //(() => GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance)),
				extraLabelGetter: ExtraLabelGetter,
				canSelectTarget: CanTargetValidator
			);

			bool ChoseGlobalTarget_Callback(GlobalTargetInfo globalTarget)
			{
				if (globalTarget.IsValid)
				{
					TeleportTargeter.StartChoosingLocal(globalTarget, callback, localTargetParams);
					return true;
				}
				else
				{
					// TODO
					Messages.Message(
						"MessageTransportPodsDestinationIsInvalid".Translate(),
						MessageTypeDefOf.RejectInput,
						false
					);
				}
				return false;
			}
		}

		public static void GlobalTeleport(Thing originator, Action<TeleportTargeterData> callback)
		{
			// select map -> select target pawn -> select map -> select target cell
			TeleportTargeterData targeterData_out = new TeleportTargeterData();
			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);

			TargetingParameters targetTeleportSubjects = new TargetingParameters
			{
				canTargetPawns = true,
				canTargetAnimals = true,
				canTargetHumans = true,
				canTargetItems = true, // not working?
				canTargetBuildings = false
			};

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

				return TargetHasLoadedMap(target) ? "Has a map" : "No map";
			}

			// select pawn
			TeleportTargeter.StartChoosingMap(globalTarget, GotFrom_Callback, targetTeleportSubjects, ExtraLabelGetter, TargetHasLoadedMap);

			void GotFrom_Callback(GlobalTargetInfo targetFrom)
			{
				if (targetFrom.IsValid && targetFrom.HasThing && targetFrom.Thing is Pawn pawn)
				{
					targeterData_out.target = pawn;

					TargetingParameters targetTeleportDestination = new TargetingParameters
					{
						canTargetPawns = false,
						canTargetBuildings = false,
						canTargetLocations = true
					};

					// select destination
					TeleportTargeter.StartChoosingMap(globalTarget, GotTo_Callback, targetTeleportDestination, ExtraLabelGetter, TargetHasLoadedMap);

					void GotTo_Callback(GlobalTargetInfo targetTo)
					{
						if (targetTo.IsValid && targetTo.IsMapTarget && targetTo.Cell.IsValid)
						{
							targeterData_out.destinationCell = targetTo.Cell;
							targeterData_out.destinationMap = targetTo.Map;
							callback(targeterData_out); // this is how we return the selected targets to the caller
						}
						else
						{
							// TODO debug log
						}
					}
				}
				else
				{
					// TODO debug log
				}
			}
		}

		public static void LocalTeleport(Thing originator, Action<TeleportTargeterData> callback)
		{
			TeleportTargeterData targeterData_out = new TeleportTargeterData();
			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);

			// select pawn
			TargetingParameters targetTeleportSubjects = new TargetingParameters
			{
				canTargetPawns = true,
				canTargetAnimals = true,
				canTargetHumans = true,
				canTargetItems = true,
				canTargetBuildings = false
			};

			TeleportTargeter.StartChoosingLocal(globalTarget, GotFrom_Callback, targetTeleportSubjects);

			void GotFrom_Callback(GlobalTargetInfo targetFrom)
			{
				if (targetFrom.IsValid && targetFrom.HasThing && targetFrom.Thing is Pawn pawn)
				{
					targeterData_out.target = pawn;

					TargetingParameters targetTeleportDestination = new TargetingParameters
					{
						canTargetPawns = false,
						canTargetBuildings = false,
						canTargetLocations = true
					};

					// select destination
					TeleportTargeter.StartChoosingLocal(globalTarget, GotTo_Callback, targetTeleportDestination);

					void GotTo_Callback(GlobalTargetInfo targetTo)
					{
						if (targetTo.IsValid && targetTo.IsMapTarget && targetTo.Cell.IsValid)
						{
							targeterData_out.destinationCell = targetTo.Cell;
							targeterData_out.destinationMap = targetTo.Map;
							callback(targeterData_out); // this is how we return the selected targets to the caller
						}
						else
						{
							// TODO debug log
						}
					}
				}
				else
				{
					// TODO debug log
				}
			}
		}
	}
}
