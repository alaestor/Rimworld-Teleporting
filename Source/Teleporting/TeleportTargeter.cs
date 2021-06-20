using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	struct TeleportTargetSolution
	{
		public struct Targets
		{
			public GlobalTargetInfo global;
			public LocalTargetInfo local;
		}

		public TeleportTargetSolution.Targets from;
		public TeleportTargetSolution.Targets to;
	}

	class TeleportTargeter
	{
		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);

		private static void StartChoosingLocal(
			GlobalTargetInfo globalTarget,
			Action<TeleportTargetSolution.Targets> callback,
			bool canChoosePawn = false,
			bool canChooseLocation = false)
		{
			MapParent selectedMap = Find.WorldObjects.MapParentAt(globalTarget.Tile);
			//Map Originator = targetOriginator.Map;
			Current.Game.CurrentMap = selectedMap.Map;
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

				TeleportTargetSolution.Targets ti = new TeleportTargetSolution.Targets();
				ti.global = globalTarget;
				ti.local = localTarget;

				callback(ti);
			}
		}

		private static void StartChoosingMap(
			GlobalTargetInfo startingFrom,
			Action<TeleportTargetSolution.Targets> callback,
			bool canChoosePawn = false,
			bool canChooseLocation = false)
		{
			CameraJumper.TryJump(startingFrom);
			Find.WorldSelector.ClearSelection();
			//int tile = this.targetOriginator.Map.Tile;

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

		public static void GlobalTeleport(Thing originator, Action<TeleportTargetSolution> callback)
		{
			// select map -> select target pawn -> select map -> select target cell
			TeleportTargetSolution tts = new TeleportTargetSolution();
			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);
			TeleportTargeter.StartChoosingMap(globalTarget, gotFrom, canChoosePawn: true);

			void gotFrom(TeleportTargetSolution.Targets tiFrom)
			{
				tts.from = tiFrom;
				TeleportTargeter.StartChoosingMap(globalTarget, gotTo, canChooseLocation: true);

				void gotTo(TeleportTargetSolution.Targets tiTo)
				{
					tts.to = tiTo;
					callback(tts); // this is how we return the selected targets to the caller
				}
			}

		}

		public static void LocalTeleport(Thing originator, Action<TeleportTargetSolution> callback)
		{
			TeleportTargetSolution tts = new TeleportTargetSolution();
			GlobalTargetInfo globalTarget = CameraJumper.GetWorldTarget(originator);

			TeleportTargeter.StartChoosingLocal(globalTarget, gotFrom, canChoosePawn: true);

			void gotFrom(TeleportTargetSolution.Targets tiFrom)
			{
				tts.from = tiFrom;
				TeleportTargeter.StartChoosingLocal(globalTarget, gotTo, canChooseLocation: true);

				void gotTo(TeleportTargetSolution.Targets tiTo)
				{
					tts.to = tiTo;
					callback(tts); // this is how we return the selected targets to the caller
				}
			}
		}
	}
}
