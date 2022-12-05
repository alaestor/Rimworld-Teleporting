using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System;

namespace alaestor_teleporting
{
	using Quantity = System.Int32;

	[StaticConstructorOnStartup]
	public class CompItemStock : ThingComp
	{
		public static int Clamp(int n, int min, int max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public CompProperties_ItemStock Props => (CompProperties_ItemStock)this.props;
		private CompFlickable flickComp;

		private Quantity quantity;
		private Quantity configuredTarget = -1;
		public Quantity Fuel => quantity;

		public bool HasStock => quantity > 0;
		public bool IsFull => quantity == Props.capacity;
		public bool allowAutoRestock = true;
		public bool ShouldAutoRestockNow => quantity < Props.capacity && flickComp == null || flickComp.SwitchIsOn;
		public Quantity TargetQuantity
		{
			get
			{
				if (configuredTarget >= 0)
					return configuredTarget;
				return Props.targetConfigurable ? Props.initialConfigurableTarget : Props.capacity;
			}
			set => configuredTarget = Clamp(value, 0, Props.capacity);
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			allowAutoRestock = Props.initialAllowAutoRestock;
			quantity = Props.initialQuantity;
			flickComp = this.parent.GetComp<CompFlickable>();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<Quantity>(ref quantity, "quantity");
			Scribe_Values.Look<Quantity>(ref configuredTarget, "configuredTarget", -1);
			Scribe_Values.Look<bool>(ref allowAutoRestock, "allowAutoRestock");
			if (Scribe.mode != LoadSaveMode.PostLoadInit || Props.showAutoRestockToggle)
				return;
			allowAutoRestock = Props.initialAllowAutoRestock;
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (!allowAutoRestock)
				parent.Map.overlayDrawer.DrawOverlay((Thing)parent, OverlayTypes.ForbiddenRefuel);
			else if (!HasStock && Props.drawOutOfStockOverlay)
				parent.Map.overlayDrawer.DrawOverlay((Thing)parent, OverlayTypes.OutOfFuel);
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (previousMap == null)
				return;
			ThingDef def = Props.item;
			int a = GenMath.RoundRandom(quantity);
			while (a > 0)
			{
				Thing thing = ThingMaker.MakeThing(def);
				thing.stackCount = Mathf.Min(a, def.stackLimit);
				a -= thing.stackCount;
				GenPlace.TryPlaceThing(thing, parent.Position, previousMap, ThingPlaceMode.Near);
			}
		}

		public override string CompInspectStringExtra()
		{
			string str = Props.stockLabel + ": " + quantity.ToString() + " / " + Props.capacity.ToString();
			if (!HasStock && !Props.emptyMessage.NullOrEmpty())
				str += string.Format("\n{0} ({1}x {2})", Props.emptyMessage, Props.capacity - quantity, Props.item.label);
			if (Props.targetConfigurable)
				str = str + ("\n" + "ConfiguredTargetFuelLevel".Translate(configuredTarget.ToString()));
			return str;
		}

		public void Consume(Quantity amount)
		{
			quantity -= amount;
			if (quantity >= 0)
				return;
			quantity = 0;
			this.parent.BroadcastCompSignal("OutOfStock");
		}

		public void Refuel(Quantity amount)
		{
			quantity += amount;
			if (quantity > Props.capacity)
				quantity = Props.capacity;
			this.parent.BroadcastCompSignal("Restocked");
		}

		public void Refuel(List<Thing> things)
		{
			Quantity count;
			for (Quantity countToFullyRefuel = Props.capacity - quantity;
				countToFullyRefuel > 0 && things.Count > 0;
				countToFullyRefuel -= count)
			{
				Thing thing = things.Pop<Thing>();
				count = Mathf.Min(countToFullyRefuel, thing.stackCount);
				this.Refuel(count);
				thing.SplitOff(count).Destroy();
			}
		}

		// public override IEnumerable<Gizmo> CompGetGizmosExtra()
		// {
		// 	foreach (Gizmo gizmo in base.CompGetGizmosExtra())
		// 		yield return gizmo;

		// 	if (DebugSettings.godMode)
		// 	{
		// 		// cheats
		// 	}
		// }
	}

	public class CompProperties_ItemStock : CompProperties
	{
		public bool targetConfigurable;
		public Quantity initialConfigurableTarget;
		public Quantity initialQuantity;
		public Quantity capacity;
		public bool initialAllowAutoRestock;
		public bool showAutoRestockToggle;
		public bool drawOutOfStockOverlay;
		public ThingDef item;

		public string stockLabel;
		public string emptyMessage;

		public string itemIconPath;

		public CompProperties_ItemStock() => this.compClass = typeof(CompProperties_ItemStock);

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			item.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
				yield return configError;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry specialDisplayStat in base.SpecialDisplayStats(req))
				yield return specialDisplayStat;
			// if (((ThingDef)req.Def).building.IsTurret)
			// 	yield return new StatDrawEntry(StatCategoryDefOf.Building, (string)"ShotsBeforeRearm".Translate(), ((int)this.capacity).ToString(), (string)"ShotsBeforeRearmExplanation".Translate(), 3171);
		}
	}

}