using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	/*
	 * 
	 * 
	 *  TEMPORARY PLACEHOLDER WORKS FOR NOW BUT OH GOD THIS IS SHIT REMAKE THIS ENTIRELY
	 *  copy pasted from vanilla
	 *  
	 * 
	 */


	[StaticConstructorOnStartup]
	class CompRefuelableAtHome : ThingComp
	{
		private float fuel;
		private float configuredTargetFuelLevel = -1f;
		public bool allowAutoRefuel = true;
		private CompFlickable flickComp;
		public const string RefueledSignal = "Refueled";
		public const string RanOutOfFuelSignal = "RanOutOfFuel";
		private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel");
		private static readonly Vector2 FuelBarSize = new Vector2(1f, 0.2f);
		private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));
		private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

		public float TargetFuelLevel
		{
			get
			{
				if (configuredTargetFuelLevel >= 0.0)
					return configuredTargetFuelLevel;
				return Props.targetFuelLevelConfigurable ? Props.initialConfigurableTargetFuelLevel : Props.fuelCapacity;
			}
			set => configuredTargetFuelLevel = Mathf.Clamp(value, 0.0f, Props.fuelCapacity);
		}

		public CompProperties_RefuelableAtHome Props => (CompProperties_RefuelableAtHome)props;

		public float Fuel => fuel;

		public float FuelPercentOfTarget => fuel / TargetFuelLevel;

		public float FuelPercentOfMax => fuel / Props.fuelCapacity;

		public bool IsFull => TargetFuelLevel - (double)fuel < 1.0;

		public bool HasFuel => fuel > 0.0 && fuel >= (double)Props.minimumFueledThreshold;

		private float ConsumptionRatePerTick => Props.fuelConsumptionRate / 60000f;

		public bool ShouldAutoRefuelNow => FuelPercentOfTarget <= (double)Props.autoRefuelPercent && !IsFull && TargetFuelLevel > 0.0 && ShouldAutoRefuelNowIgnoringFuelPct;

		public bool ShouldAutoRefuelNowIgnoringFuelPct => !parent.IsBurning() && (flickComp == null || flickComp.SwitchIsOn) && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Flick) == null && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) == null;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			allowAutoRefuel = Props.initialAllowAutoRefuel;
			fuel = Props.fuelCapacity * Props.initialFuelPercent;
			flickComp = parent.GetComp<CompFlickable>();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref fuel, "fuel");
			Scribe_Values.Look<float>(ref configuredTargetFuelLevel, "configuredTargetFuelLevel", -1f);
			Scribe_Values.Look<bool>(ref allowAutoRefuel, "allowAutoRefuel");
			if (Scribe.mode != LoadSaveMode.PostLoadInit || Props.showAllowAutoRefuelToggle)
				return;
			allowAutoRefuel = Props.initialAllowAutoRefuel;
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (!allowAutoRefuel)
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenRefuel);
			else if (!HasFuel && Props.drawOutOfFuelOverlay)
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.OutOfFuel);
			if (!Props.drawFuelGaugeInMap)
				return;
			GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest();
			r.center = parent.DrawPos + Vector3.up * 0.1f;
			r.size = FuelBarSize;
			r.fillPercent = FuelPercentOfMax;
			r.filledMat = FuelBarFilledMat;
			r.unfilledMat = FuelBarUnfilledMat;
			r.margin = 0.15f;
			Rot4 rotation = parent.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (previousMap == null || Props.fuelFilter.AllowedDefCount != 1 || Props.initialFuelPercent != 0.0)
				return;
			ThingDef def = Props.fuelFilter.AllowedThingDefs.First<ThingDef>();
			int a = GenMath.RoundRandom(1f * fuel);
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
			string str = Props.FuelLabel + ": " + fuel.ToStringDecimalIfSmall() + " / " + Props.fuelCapacity.ToStringDecimalIfSmall();
			if (!Props.consumeFuelOnlyWhenUsed && HasFuel)
			{
				int numTicks = (int)(fuel / (double)Props.fuelConsumptionRate * 60000.0);
				str = str + " (" + numTicks.ToStringTicksToPeriod() + ")";
			}
			if (!HasFuel && !Props.outOfFuelMessage.NullOrEmpty())
				str += string.Format("\n{0} ({1}x {2})", Props.outOfFuelMessage, GetFuelCountToFullyRefuel(), Props.fuelFilter.AnyAllowedDef.label);
			if (Props.targetFuelLevelConfigurable)
				str = str + ("\n" + "ConfiguredTargetFuelLevel".Translate(TargetFuelLevel.ToStringDecimalIfSmall()));
			return str;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (!Props.consumeFuelOnlyWhenUsed && (flickComp == null || flickComp.SwitchIsOn))
				ConsumeFuel(ConsumptionRatePerTick);
			if (Props.fuelConsumptionPerTickInRain <= 0.0 || !parent.Spawned || (parent.Map.weatherManager.RainRate <= 0.400000005960464 || parent.Map.roofGrid.Roofed(parent.Position)))
				return;
			ConsumeFuel(Props.fuelConsumptionPerTickInRain);
		}

		public void ConsumeFuel(float amount)
		{
			if (fuel <= 0.0)
				return;
			fuel -= amount;
			if (fuel > 0.0)
				return;
			fuel = 0.0f;
			if (Props.destroyOnNoFuel)
				parent.Destroy(DestroyMode.Vanish);
			parent.BroadcastCompSignal("RanOutOfFuel");
		}

		public void Refuel(List<Thing> fuelThings)
		{
			int count;
			if (Props.atomicFueling && fuelThings.Sum<Thing>(t => t.stackCount) < GetFuelCountToFullyRefuel())
			{
				Log.ErrorOnce("Error refueling; not enough fuel available for proper atomic refuel", 19586442);
			}
			else
			{
				for (int countToFullyRefuel = GetFuelCountToFullyRefuel(); countToFullyRefuel > 0 && fuelThings.Count > 0; countToFullyRefuel -= count)
				{
					Thing thing = fuelThings.Pop<Thing>();
					count = Mathf.Min(countToFullyRefuel, thing.stackCount);
					Refuel(count);
					thing.SplitOff(count).Destroy();
				}
			}
		}

		public void Refuel(float amount)
		{
			fuel += amount * Props.FuelMultiplierCurrentDifficulty;
			if (fuel > (double)Props.fuelCapacity)
				fuel = Props.fuelCapacity;
			parent.BroadcastCompSignal("Refueled");
		}

		public void Notify_UsedThisTick() => ConsumeFuel(ConsumptionRatePerTick);

		public int GetFuelCountToFullyRefuel() => Props.atomicFueling ? Mathf.CeilToInt(Props.fuelCapacity / Props.FuelMultiplierCurrentDifficulty) : Mathf.Max(Mathf.CeilToInt((TargetFuelLevel - fuel) / Props.FuelMultiplierCurrentDifficulty), 1);

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			if (DebugSettings.godMode)
			{
				// cheats
			}
		}
	}

	public class CompProperties_RefuelableAtHome : CompProperties
	{
		public float fuelConsumptionRate = 1f;
		public float fuelCapacity = 2f;
		public float initialFuelPercent;
		public float autoRefuelPercent = 0.3f;
		public float fuelConsumptionPerTickInRain;
		public ThingFilter fuelFilter;
		public bool destroyOnNoFuel;
		public bool consumeFuelOnlyWhenUsed;
		public bool showFuelGizmo;
		public bool initialAllowAutoRefuel = true;
		public bool showAllowAutoRefuelToggle;
		public bool targetFuelLevelConfigurable;
		public float initialConfigurableTargetFuelLevel;
		public bool drawOutOfFuelOverlay = true;
		public float minimumFueledThreshold;
		public bool drawFuelGaugeInMap;
		public bool atomicFueling;
		private float fuelMultiplier = 1f;
		public bool factorByDifficulty;
		public string fuelLabel;
		public string fuelGizmoLabel;
		public string outOfFuelMessage;
		public string fuelIconPath;
		private Texture2D fuelIcon;

		public string FuelLabel => fuelLabel.NullOrEmpty() ? "Fuel".TranslateSimple() : fuelLabel;

		public string FuelGizmoLabel => fuelGizmoLabel.NullOrEmpty() ? "Fuel".TranslateSimple() : fuelGizmoLabel;

		public Texture2D FuelIcon
		{
			get
			{
				if (fuelIcon == null)
					fuelIcon = fuelIconPath.NullOrEmpty() ? (fuelFilter.AnyAllowedDef == null ? ThingDefOf.Chemfuel : fuelFilter.AnyAllowedDef).uiIcon : ContentFinder<Texture2D>.Get(fuelIconPath);
				return fuelIcon;
			}
		}

		public float FuelMultiplierCurrentDifficulty => factorByDifficulty ? fuelMultiplier / Find.Storyteller.difficultyValues.maintenanceCostFactor : fuelMultiplier;

		public CompProperties_RefuelableAtHome() => compClass = typeof(CompRefuelableAtHome);

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			fuelFilter.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
				yield return configError;
			if (destroyOnNoFuel && initialFuelPercent <= 0.0)
				yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
			if ((!consumeFuelOnlyWhenUsed || fuelConsumptionPerTickInRain > 0.0) && parentDef.tickerType != TickerType.Normal)
				yield return string.Format("Refuelable component set to consume fuel per tick, but parent tickertype is {0} instead of {1}", parentDef.tickerType, TickerType.Normal);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(
		  StatRequest req)
		{
			foreach (StatDrawEntry specialDisplayStat in base.SpecialDisplayStats(req))
				yield return specialDisplayStat;
			if (((ThingDef)req.Def).building.IsTurret)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Building, "RearmCost".Translate(), GenLabel.ThingLabel(fuelFilter.AnyAllowedDef, null, (int)(fuelCapacity / (double)FuelMultiplierCurrentDifficulty)).CapitalizeFirst(), "RearmCostExplanation".Translate(), 3171);
				yield return new StatDrawEntry(StatCategoryDefOf.Building, "ShotsBeforeRearm".Translate(), ((int)fuelCapacity).ToString(), "ShotsBeforeRearmExplanation".Translate(), 3171);
			}
		}
	}
}
