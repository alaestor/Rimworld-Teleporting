using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class TeleportTargeter
	{
		public static void StartChoosingLocal(
			GlobalTargetInfo startingFrom,
			Action<LocalTargetInfo> callback,
			TargetingParameters targetParams,
			Func<LocalTargetInfo, bool> CanTargetValidator = null,
			Texture2D mouseAttachment = null)
		{
			Map targetMap = Find.WorldObjects.MapParentAt(startingFrom.Tile).Map;
			Current.Game.CurrentMap = targetMap;
			//CameraJumper.TryJump(startingFrom);
			CameraJumper.TryHideWorld();

			Find.Targeter.BeginTargeting(
				targetParams: targetParams,
				action: ChoseLocalTarget_Callback,
				highlightAction: null,
				targetValidator: CanTargetValidator,
				mouseAttachment: mouseAttachment
			);


			void ChoseLocalTarget_Callback(LocalTargetInfo localTarget)
			{
				Find.Targeter.StopTargeting();
				if (localTarget.IsValid)
				{
					callback(localTarget);
				}
				else
				{
					// TODO better message
					Messages.Message(
						"MessageTransportPodsDestinationIsInvalid".Translate(),
						MessageTypeDefOf.RejectInput,
						false
					);
				}
			}
		}

		public static void StartChoosingGlobal(
			GlobalTargetInfo startingFrom,
			Action<GlobalTargetInfo> callback,
			bool canTargetTiles = true,
			Texture2D mouseAttachment = null,
			bool closeWorldTabWhenFinished = true,
			Action onUpdate = null,
			Func<GlobalTargetInfo, string> ExtraLabelGetter = null,
			Func<GlobalTargetInfo, bool> CanTargetValidator = null)
		{
			CameraJumper.TryJump(startingFrom);
			Find.WorldSelector.ClearSelection();

			Find.WorldTargeter.BeginTargeting_NewTemp(
				action: ChoseGlobalTarget_Callback,
				canTargetTiles: canTargetTiles,
				mouseAttachment: mouseAttachment,
				closeWorldTabWhenFinished: closeWorldTabWhenFinished,
				onUpdate: onUpdate, //(() => GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance)),
				extraLabelGetter: ExtraLabelGetter,
				canSelectTarget: CanTargetValidator
			);

			bool ChoseGlobalTarget_Callback(GlobalTargetInfo globalTarget)
			{
				Find.WorldTargeter.StopTargeting();
				if (globalTarget.IsValid)
				{
					callback(globalTarget);
					return true;
				}
				else
				{
					// TODO better message
					Messages.Message(
						"MessageTransportPodsDestinationIsInvalid".Translate(),
						MessageTypeDefOf.RejectInput,
						false
					);
					return false;
				}
			}
		}

		public static void StartChoosingGlobalThenLocal(
			GlobalTargetInfo startingFrom,
			Action<GlobalTargetInfo> result_Callback,
			TargetingParameters localTargetParams,
			Func<LocalTargetInfo, bool> localTargetValidator = null,
			Texture2D localMouseAttachment = null,
			bool globalCanTargetTiles = true,
			Action globalOnUpdate = null,
			bool globalCloseWorldTabWhenFinished = true,
			Texture2D globalMouseAttachment = null,
			Func<GlobalTargetInfo, string> globalExtraLabelGetter = null,
			Func<GlobalTargetInfo, bool> globalTargetValidator = null)
		{
			TeleportTargeter.StartChoosingGlobal(
				startingFrom: startingFrom,
				callback: GotGlobalTarget_Callback,
				canTargetTiles: globalCanTargetTiles,
				mouseAttachment: globalMouseAttachment,
				closeWorldTabWhenFinished: globalCloseWorldTabWhenFinished,
				onUpdate: globalOnUpdate,
				ExtraLabelGetter: globalExtraLabelGetter,
				CanTargetValidator: globalTargetValidator);

			void GotGlobalTarget_Callback(GlobalTargetInfo globalTarget)
			{
				if (globalTarget.IsValid)
				{
					TeleportTargeter.StartChoosingLocal(
						startingFrom: globalTarget,
						callback: GotLocalTarget_Callback,
						targetParams: localTargetParams,
						CanTargetValidator: localTargetValidator,
						mouseAttachment: localMouseAttachment);

					void GotLocalTarget_Callback(LocalTargetInfo localTarget)
					{
						if (localTarget.IsValid)
						{
							result_Callback(localTarget.ToGlobalTargetInfo(
								Find.WorldObjects.MapParentAt(globalTarget.Tile).Map));
						}
					}
				}
				else
				{
					Log.Error("Teleporting: StartChoosingGlobalThenLocal: invalid global target");
				}
			}
		}
	}
}
