using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;


/*
 * IDEA
 * generic teleport comp
 * configure options via properties (wearable, OnlyTeleportSelf, etc)
 */

namespace alaestor_teleporting
{
	public class CompTeleportApparel : ThingComp
	{
		public CompProperties_TeleportApparel Props => (CompProperties_TeleportApparel)this.props;

		public Pawn Wearer
		{
			get
			{
				if (parent is Apparel apparel)
				{
					return apparel.Wearer;
				}
				else
				{
					Logger.Error("CompTeleportApparel::Wearer: isn't apparel");
					return null;
				}
			}
		}

		private bool CanDoShortRangeTeleport => Props.shortRange;
		private bool CanDoLongRangeTeleport => Props.longRange;
		private bool CanTeleportOthers => Props.canTeleportOthers;


		// behavior move to teleportBehavior
		public void StartTeleport_ShortRange()
		{
			if (CanDoShortRangeTeleport)
			{
				if (Wearer != null && Wearer.Map != null)
				{
					Logger.DebugVerbose("TeleportBelt_Local::StartTeleport_ShortRange: called",
						"Wearer: " + Wearer.Label ?? "(no label)"
					);

					if (CanTeleportOthers)
					{
						TeleportBehavior.StartTeleportTargetting(false, Wearer);
					}
					else
					{
						TeleportTargeter.StartChoosingLocal(
							startingFrom: Wearer,
							result_Callback: FinishedChoosing,
							targetParams: TeleportBehavior.targetTeleportDestination,
							mouseAttachment: TeleportBehavior.localTeleportMouseAttachment);

						void FinishedChoosing(LocalTargetInfo destination)
						{
							Logger.DebugVerbose("TeleportBelt_Local::StartTeleport_ShortRange:  Got destination:\t\t" + destination.Label + " at Cell: " + destination.Cell.ToString());
							TeleportBehavior.ExecuteTeleport(Wearer, Wearer.Map, destination.Cell);
						}
					}
				}
				else Logger.Error("TeleportBelt_Local::StartTeleport_ShortRange: invalid wearer");
			}
			else Logger.Error("TeleportBelt_Local::StartTeleport_ShortRange: disallowed, CanDoShortRangeTelePort is false");
		}

		public void StartTeleport_LongRange(bool cheat = false)
		{
			if (CanDoLongRangeTeleport)
			{
				if (Wearer != null && Wearer.Map != null)
				{
					Logger.DebugVerbose("TeleportBelt_Local::StartTeleport_LongRange: called",
						"Wearer: " + Wearer.Label ?? "(no label)"
					);

					if (CanTeleportOthers)
					{
						TeleportBehavior.StartTeleportTargetting(true, Wearer);
					}
					else
					{
						bool fuelDistanceMatters =
							TeleportingMod.settings.enableFuel
							&& TeleportingMod.settings.longRange_FuelDistance > 0;

						GlobalTargetInfo startingHere = CameraJumper.GetWorldTarget(Wearer);

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

						Logger.Warning("Sanity check");

						TeleportTargeter.StartChoosingGlobalThenLocal(
							startingFrom: startingHere,
							result_Callback: FinishedChoosing,
							localTargetParams: TeleportBehavior.targetTeleportDestination,
							localMouseAttachment: TeleportBehavior.localTeleportMouseAttachment,
							globalMouseAttachment: TeleportBehavior.globalTeleportMouseAttachment,
							globalOnUpdate: OnUpdate,
							globalExtraLabelGetter: ExtraLabelGetter,
							globalTargetValidator: TeleportTargeter.TargetHasLoadedMap
						);

						void FinishedChoosing(GlobalTargetInfo destination)
						{
							Logger.DebugVerbose("TeleportBelt_Local::StartTeleport_LongRange:  Got destination:\t\t" + destination.Label + " at Cell: " + destination.Cell.ToString());
							TeleportBehavior.ExecuteTeleport(Wearer, destination.Map, destination.Cell);
						}
					}
				}
				else Logger.Error("TeleportBelt_Local::StartTeleport_LongRange: invalid wearer");
			}
			else Logger.Error("TeleportBelt_Local::StartTeleport_LongRange: disallowed, CanDoLongRangeTelePort is false");
		}

		public override void Notify_KilledPawn(Pawn pawn)
		{
			base.Notify_KilledPawn(pawn);
		}

		public override void CompTick()
		{ // ticks every 1/60th second (1t / 60tps)
			base.CompTick();
		}

		public override void CompTickRare()
		{ // ticks every 4.16 seconds (250t / 60tps)
			base.CompTickRare();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
		}

		public override void PostDraw()
		{
			base.PostDraw();
		}

		public override string CompInspectStringExtra()
		{
			string str = base.CompInspectStringExtra();
			return str;
		}

		/*
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;
		}
		*/

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
				yield return gizmo;

			if (Find.Selector.SingleSelectedThing == Wearer)
			{

				if (CanDoShortRangeTeleport)
				{
					// TODO make a factory for these; simplify by passing name and suffixing _Label and _Desc, etc
					yield return new Command_Action
					{
						defaultLabel = "ShortTeleDebugGizmo_Label".Translate(), //"Tele Local",
						defaultDesc = "ShortTeleDebugGizmo_Desc".Translate(), //"Teleport on map layer",
						activateSound = SoundDef.Named("Click"),
						action = delegate
						{
							Logger.Debug("TeleportBelt_Local: called Gizmo: Short Range Teleport");
							StartTeleport_ShortRange();
						}
					};
				}

				if (CanDoLongRangeTeleport)
				{
					yield return new Command_Action
					{
						defaultLabel = "LongTeleDebugGizmo_Label".Translate(), //"Tele Local",
						defaultDesc = "LongTeleDebugGizmo_Desc".Translate(), //"Teleport on map layer",
						activateSound = SoundDef.Named("Click"),
						action = delegate
						{
							Logger.Debug("TeleportBelt_Local: called Gizmo: Long Range Teleport");
							StartTeleport_LongRange();
						}
					};
				}

				if (DebugSettings.godMode)
				{
					yield return new Command_Action
					{
						defaultLabel = "Cheat_ShortTeleDebugGizmo_Label".Translate(), //"Tele Local",
						defaultDesc = "Cheat_ShortTeleDebugGizmo_Desc".Translate(), //"Teleport on map layer",
						activateSound = SoundDef.Named("Click"),
						action = delegate
						{
							Logger.Debug("TeleportBelt_Local:: called Godmode Gizmo: Short Range Teleport");
							TeleportBehavior.StartTeleportTargetting(false, Wearer, cheat: true);
						}
					};

					yield return new Command_Action
					{
						defaultLabel = "Cheat_LongTeleDebugGizmo_Label".Translate(),
						defaultDesc = "Cheat_LongTeleDebugGizmo_Desc".Translate(),
						activateSound = SoundDef.Named("Click"),
						action = delegate
						{
							Logger.Debug("TeleportBelt_Local:: called Godmode Gizmo: Long Range Teleport");
							TeleportBehavior.StartTeleportTargetting(true, Wearer, cheat: true);
						}
					};
				}
			}
		}
	}

	public class CompProperties_TeleportApparel : CompProperties
	{
		public bool shortRange = false;
		public bool longRange = false;
		public bool canTeleportOthers = false;

		public CompProperties_TeleportApparel()
		{
			this.compClass = typeof(CompTeleportApparel);
		}

		public CompProperties_TeleportApparel(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}// namespace alaestor_teleporting
