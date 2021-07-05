using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class TeleportTargeter
	{
		public static bool TargetHasLoadedMap(GlobalTargetInfo target)
		{
			return target.IsValid
				&& Find.WorldObjects.AnyMapParentAt(target.Tile)
				&& Find.WorldObjects.MapParentAt(target.Tile).Spawned
				&& Find.WorldObjects.MapParentAt(target.Tile).Map != null;
		}

		public static bool TargetIsWithinGlobalRangeLimit(int origin, int target)
		{
			if (TeleportingMod.settings.enableGlobalRangeLimit)
			{
				int distanceToTarget = Find.WorldGrid.TraversalDistanceBetween(origin, target, true, int.MaxValue);
				return distanceToTarget <= TeleportingMod.settings.globalRangeLimit;
			}
			else return true;
		}

		public static bool TargetIsWithinGlobalRangeLimit(GlobalTargetInfo origin, GlobalTargetInfo target)
		{
			return TargetIsWithinGlobalRangeLimit(origin.Tile, target.Tile);
		}

		public static void StartChoosingLocal(
			GlobalTargetInfo startingFrom,
			Action<LocalTargetInfo> result_Callback,
			TargetingParameters targetParams,
			Func<LocalTargetInfo, bool> canTargetValidator = null,
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
				targetValidator: canTargetValidator,
				mouseAttachment: mouseAttachment
			);

			void ChoseLocalTarget_Callback(LocalTargetInfo localTarget)
			{
				Find.Targeter.StopTargeting();
				if (localTarget.IsValid)
				{
					result_Callback(localTarget);
				}
				else Logger.Error("TeleportTargeter::StartChoosingLocal: invalid local target");
			}
		}

		public static void StartChoosingGlobal(
			GlobalTargetInfo startingFrom,
			Action<GlobalTargetInfo> result_Callback,
			bool canTargetTiles = true,
			Texture2D mouseAttachment = null,
			bool closeWorldTabWhenFinished = true,
			Action onUpdate = null,
			Func<GlobalTargetInfo, string> extraLabelGetter = null,
			Func<GlobalTargetInfo, bool> canTargetValidator = null)
		{
			CameraJumper.TryJump(startingFrom);
			Find.WorldSelector.ClearSelection();

			Find.WorldTargeter.BeginTargeting_NewTemp(
				action: ChoseGlobalTarget_Callback,
				canTargetTiles: canTargetTiles,
				mouseAttachment: mouseAttachment,
				closeWorldTabWhenFinished: closeWorldTabWhenFinished,
				onUpdate: onUpdate,
				extraLabelGetter: extraLabelGetter,
				canSelectTarget: canTargetValidator
			);

			bool ChoseGlobalTarget_Callback(GlobalTargetInfo globalTarget)
			{
				Find.WorldTargeter.StopTargeting();
				if (globalTarget.IsValid)
				{
					result_Callback(globalTarget);
					return true;
				}
				else
				{
					Logger.Error("TeleportTargeter::StartChoosingGlobal: invalid global target");
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
				result_Callback: GotGlobalTarget_Callback,
				canTargetTiles: globalCanTargetTiles,
				mouseAttachment: globalMouseAttachment,
				closeWorldTabWhenFinished: globalCloseWorldTabWhenFinished,
				onUpdate: globalOnUpdate,
				extraLabelGetter: globalExtraLabelGetter,
				canTargetValidator: globalTargetValidator);

			void GotGlobalTarget_Callback(GlobalTargetInfo globalTarget)
			{
				if (globalTarget.IsValid)
				{
					TeleportTargeter.StartChoosingLocal(
						startingFrom: globalTarget,
						result_Callback: GotLocalTarget_Callback,
						targetParams: localTargetParams,
						canTargetValidator: localTargetValidator,
						mouseAttachment: localMouseAttachment);

					void GotLocalTarget_Callback(LocalTargetInfo localTarget)
					{
						if (localTarget.IsValid)
						{
							result_Callback(localTarget.ToGlobalTargetInfo(
								Find.WorldObjects.MapParentAt(globalTarget.Tile).Map));
						}
						else Logger.Error("TeleportTargeter::StartChoosingGlobalThenLocal: invalid local target");
					}
				}
				else Logger.Error("TeleportTargeter::StartChoosingGlobalThenLocal: invalid global target");
			}
		}
	}
}
