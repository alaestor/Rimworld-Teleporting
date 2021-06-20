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
			bool canChoosePawn = false,
			bool canChooseLocation = false)
		{
			Map selectedMap = Find.WorldObjects.MapParentAt(globalTarget.Tile).Map;
			//Map Originator = targetOriginator.Map;
			Current.Game.CurrentMap = selectedMap;
			CameraJumper.TryHideWorld();

			TargetingParameters targetParams = new TargetingParameters
			{
				canTargetPawns = canChoosePawn,
				canTargetLocations = canChooseLocation
			};

			Find.Targeter.BeginTargeting(
				targetParams: targetParams,
				action: ChoseLocalTarget,
				mouseAttachment: TeleportTargeter.TargeterMouseAttachment
			);

			void ChoseLocalTarget(LocalTargetInfo localTarget)
			{
				Find.WorldTargeter.StopTargeting();

				if (localTarget.IsValid)
				{
					GlobalTargetInfo targetOut = localTarget.ToGlobalTargetInfo(selectedMap);
					callback(targetOut);
				}
			}
		}

		private static void StartChoosingMap(
			GlobalTargetInfo startingFrom,
			Action<GlobalTargetInfo> callback,
			bool canChoosePawn = false,
			bool canChooseLocation = false)
		{
			CameraJumper.TryJump(startingFrom);
			Find.WorldSelector.ClearSelection();

			Find.WorldTargeter.BeginTargeting_NewTemp(
				action: ChoseGlobalTarget,
				canTargetTiles: true,
				mouseAttachment: TeleportTargeter.TargeterMouseAttachment,
				closeWorldTabWhenFinished: true,
				onUpdate: null, //(() => GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance)),
				extraLabelGetter: ExtraLabelGetter, // null
				canSelectTarget: CanSelectTarget // null
			);

			bool ChoseGlobalTarget(GlobalTargetInfo globalTarget)
			{
				if (globalTarget.IsValid)
				{
					TeleportTargeter.StartChoosingLocal(globalTarget, callback, canChoosePawn, canChooseLocation);
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


			bool CanSelectTarget(GlobalTargetInfo target)
			{
				return target.IsValid && Find.WorldObjects.AnyMapParentAt(target.Tile);
			}

			string ExtraLabelGetter(GlobalTargetInfo target)
			{
				if (!target.IsValid)
					return null;

				return Find.WorldObjects.AnyMapParentAt(target.Tile) ? "Has a map" : "No map";
			}
		}

		public static void GlobalTeleport(Thing originator, Action<TeleportTargeterData> callback)
		{
			// select map -> select target pawn -> select map -> select target cell
			TeleportTargeterData targeterData_out = new TeleportTargeterData();
			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);

			// select pawn
			TeleportTargeter.StartChoosingMap(globalTarget, gotFrom, canChoosePawn: true);

			void gotFrom(GlobalTargetInfo targetFrom)
			{
				if (targetFrom.IsValid && targetFrom.HasThing && targetFrom.Thing is Pawn pawn)
				{
					targeterData_out.target = pawn;

					// select destination
					TeleportTargeter.StartChoosingMap(globalTarget, gotTo, canChooseLocation: true);

					void gotTo(GlobalTargetInfo targetTo)
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
			TeleportTargeter.StartChoosingLocal(globalTarget, gotFrom, canChoosePawn: true);

			void gotFrom(GlobalTargetInfo targetFrom)
			{
				if (targetFrom.IsValid && targetFrom.HasThing && targetFrom.Thing is Pawn pawn)
				{
					targeterData_out.target = pawn;

					// select destination
					TeleportTargeter.StartChoosingLocal(globalTarget, gotTo, canChooseLocation: true);

					void gotTo(GlobalTargetInfo targetTo)
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
