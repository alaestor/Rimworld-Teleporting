using RimWorld;
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

		private CompNameLinkable NameLinkableComp => parent.GetComp<CompNameLinkable>() ?? null;
		private bool HasNameLinkable => NameLinkableComp != null;

		public bool CanDoTeleport_ShortRange => Props.shortRange;
		public bool CanDoTeleport_LongRange => Props.longRange;
		public bool UseNameLinkable => Props.useNameLinkable;
		public bool CanTeleportOthers => Props.canTeleportOthers;

		//public bool WearerCanUse => ;

		// TODO check if wearer alive, capable of manipulation, etc
		public void StartTeleport_ShortRange(bool cheat = false)
		{
			if (CanDoTeleport_ShortRange)
			{
				if (Wearer != null && Wearer.Map != null)
				{
					if (CanTeleportOthers)
					{
						TeleportBehavior.StartTeleportTargetting(false, Wearer, cheat: cheat);
					}
					else
					{
						TeleportBehavior.StartTeleportPawn(false, Wearer, cheat: cheat);
					}
				}
				else Logger.Error("CompTeleportApparel::StartTeleport_ShortRange: invalid wearer");
			}
			else Logger.Error("CompTeleportApparel::StartTeleport_ShortRange: disallowed, CanDoShortRangeTelePort is false");
		}

		public void StartTeleport_LongRange(bool cheat = false)
		{
			if (CanDoTeleport_LongRange)
			{
				if (Wearer != null && Wearer.Map != null)
				{
					Logger.DebugVerbose("CompTeleportApparel::StartTeleport_LongRange: called",
						"Wearer: " + Wearer.Label ?? "(no label)"
					);

					if (CanTeleportOthers)
					{
						TeleportBehavior.StartTeleportTargetting(true, Wearer, cheat: cheat);
					}
					else
					{
						TeleportBehavior.StartTeleportPawn(true, Wearer, cheat: cheat);
					}
				}
				else Logger.Error("CompTeleportApparel::StartTeleport_LongRange: invalid wearer");
			}
			else Logger.Error("CompTeleportApparel::StartTeleport_LongRange: disallowed, CanDoLongRangeTelePort is false");
		}

		public void StartTeleport_LinkedThing(bool cheat = false)
		{
			if (UseNameLinkable)
			{
				if (HasNameLinkable)
				{
					CompNameLinkable nameLinkable = NameLinkableComp;
					if (nameLinkable.IsLinkedToSomething)
					{
						if (nameLinkable.HasValidLinkedThing)
						{
							Thing destination = nameLinkable.LinkedThing;
							if (destination.Map != null && destination.InteractionCell.IsValid)
							{
								if (TeleportBehavior.ExecuteTeleport(Wearer, destination.Map, destination.InteractionCell))
								{
									Logger.Debug(
										"CompTeleportApparel::StartTeleport_LinkedThing: Teleported "
											+ Wearer.Label
											+ " from \"" + nameLinkable.Name
											+ "\" to \"" + nameLinkable.GetNameOfLinkedLinkedThing + "\"",
										"Destination Map: " + destination.Map.ToString(),
										"Destination Cell: " + destination.InteractionCell.ToString()
									);
								}
								else
								{
									Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: ExecuteTeleport failed.");
								}
							}
							else
							{
								Logger.Error(
									"CompTeleportApparel::StartTeleport_LinkedThing: destination map or cell was invalid!",
									"Map: " + destination.Map.ToString(),
									"Cell: " + destination.InteractionCell.ToString()
								);
							}

						}
						else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: nameLinkable is linked to invalid thing");
					}
					else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: nameLinkable isn't linked");
				}
				else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: UseNameLinkable is true but NameLinkable is null",
					"Parent: " + parent.Label
				);
			}
			else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: UseNameLinkable is false");
		}

		public void SelfDestruct()
		{
			Logger.DebugVerbose(parent.Label + " self destructed");
			this.parent.SplitOff(1).Destroy();
		}

		public override string CompInspectStringExtra()
		{
			string str = base.CompInspectStringExtra();
			return str;
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
				yield return gizmo;

			if (Find.Selector.SingleSelectedThing == Wearer)
			{
				if (CanDoTeleport_ShortRange)
				{
					yield return GizmoHelper.MakeCommandAction(
						"TeleportApparel_ShortRange",
						delegate
						{
							Logger.Debug("CompTeleportApparel: called Gizmo: Short Range Teleport");
							StartTeleport_ShortRange();
						}
					);
				}

				if (CanDoTeleport_LongRange)
				{
					yield return GizmoHelper.MakeCommandAction(
						"TeleportApparel_LongRange",
						delegate
						{
							Logger.Debug("CompTeleportApparel: called Gizmo: Long Range Teleport");
							StartTeleport_LongRange();
						}
					);
				}

				if (UseNameLinkable)
				{
					if (HasNameLinkable)
					{
						var nameLinkable = NameLinkableComp;
						if (nameLinkable.IsLinkedToSomething)
						{
							yield return GizmoHelper.MakeCommandAction(
								"TeleportApparel_TeleportToLink",
								delegate
								{
									Logger.Debug("CompTeleportApparel: called Gizmo: Teleport to Link");
									StartTeleport_LinkedThing();
								},
								disabled: nameLinkable.HasInvalidLinkedThing
							);

							yield return GizmoHelper.MakeCommandAction(
								"TeleportApparel_Unlink",
								delegate
								{
									Logger.Debug("CompTeleportApparel: called Gizmo: Unlink");
									// make confirmation window warning that it will be destroyed?
									nameLinkable.Unlink();
									SelfDestruct();
								}
							);
						}
						else
						{
							yield return GizmoHelper.MakeCommandAction(
								"TeleportApparel_MakeLink",
								delegate
								{
									Logger.Debug("CompTeleportApparel: called Gizmo: Make Link");
									nameLinkable.BeginMakeLink();
								}
							);
						}
					}
					else Logger.Error("CompTeleportApparel: UseNameLinkable is true but NameLinkable is null",
						"Parent: " + parent.Label
					);
				}

				/*
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
				*/
			}
		}
	}

	public class CompProperties_TeleportApparel : CompProperties
	{
		public bool shortRange = false;
		public bool longRange = false;
		public bool useNameLinkable = false;
		public bool canTeleportOthers = false;

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
				yield return configError;

			if (!(shortRange || longRange || useNameLinkable))
			{
				yield return "All teleport types are false. At least one should be true: shortRange, longRange, useNameLinkable";
			}
		}

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
