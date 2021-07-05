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

		private bool CanDoShortRangeTeleport => Props.shortRange;
		private bool CanDoLongRangeTeleport => Props.longRange;
		private bool CanTeleportOthers => Props.canTeleportOthers;


		// TODO check if wearer alive, capable of manipulation, etc
		public void StartTeleport_ShortRange(bool cheat = false)
		{
			if (CanDoShortRangeTeleport)
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
						TeleportBehavior.StartTeleportTargetting(true, Wearer, cheat: cheat);
					}
					else
					{
						TeleportBehavior.StartTeleportPawn(true, Wearer, cheat: cheat);
					}
				}
				else Logger.Error("TeleportBelt_Local::StartTeleport_LongRange: invalid wearer");
			}
			else Logger.Error("TeleportBelt_Local::StartTeleport_LongRange: disallowed, CanDoLongRangeTelePort is false");
		}

		public void SelfDestruct()
		{
			Logger.Debug("SelfDestruct!");
			this.parent.SplitOff(1).Destroy();
		}

		public override void CompTick()
		{ // ticks every 1/60th second (1t / 60tps)
			base.CompTick();
			Logger.Debug("CompTick");
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

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
				yield return gizmo;

			if (Find.Selector.SingleSelectedThing == Wearer)
			{
				if (CanDoShortRangeTeleport)
				{
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
