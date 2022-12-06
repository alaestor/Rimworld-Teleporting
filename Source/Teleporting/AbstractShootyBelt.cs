using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace alaestor_teleporting
{
	[StaticConstructorOnStartup]
	public class Gizmo_EnergyShieldStatus_Cheaty : Gizmo
	{
		public AbstractShootyBelt shield;
		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public Gizmo_EnergyShieldStatus_Cheaty() => Order = -100f;

		public override float GetWidth(float maxWidth) => 140f;

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect1 = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect1.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect1);
			Rect rect3 = rect2;
			rect3.height = rect1.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect3, shield.LabelCap);
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
			Widgets.FillableBar(rect4, fillPercent, Gizmo_EnergyShieldStatus_Cheaty.FullShieldBarTex, Gizmo_EnergyShieldStatus_Cheaty.EmptyShieldBarTex, false);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Rect rect5 = rect4;
			float num = shield.Energy * 100f;
			string str1 = num.ToString("F0");
			num = shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f;
			string str2 = num.ToString("F0");
			string label = str1 + " / " + str2;
			Widgets.Label(rect5, label);
			Text.Anchor = TextAnchor.UpperLeft;
			return new GizmoResult(GizmoState.Clear);
		}
	}

	[StaticConstructorOnStartup]
	abstract public class AbstractShootyBelt : Apparel
	{
		private float energy;
		private int ticksToReset = -1;
		private int lastKeepDisplayTick = -9999;
		private Vector3 impactAngleVect;
		private int lastAbsorbDamageTick = -9999;
		private const float MinDrawSize = 1.2f;
		private const float MaxDrawSize = 1.55f;
		private const float MaxDamagedJitterDist = 0.05f;
		private const int JitterDurationTicks = 8;
		private int StartingTicksToReset = 3200;
		private float EnergyOnReset = 0.2f;
		private float EnergyLossPerDamage = 0.033f;
		private int KeepDisplayingTicks = 1000;
		private float ApparelScorePerEnergyMax = 0.25f;

		protected bool AdvancedVersion = false;

		private static readonly Material Bubble_Background = MaterialPool.MatFrom("Other/alaestor_teleporting_belt_CheatBelt_background", ShaderDatabase.Transparent);
		private static readonly Material Bubble_Foreground = MaterialPool.MatFrom("Other/alaestor_teleporting_belt_CheatBelt_foreground", ShaderDatabase.Transparent);
		private static readonly Material Bubble_Simple = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

		private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

		private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

		public float Energy => energy;

		public ShieldState ShieldState => ticksToReset > 0 ? ShieldState.Resetting : ShieldState.Active;

		private bool ShouldDisplay
		{
			get
			{
				Pawn wearer = Wearer;
				return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner || Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref energy, "energy");
			Scribe_Values.Look<int>(ref ticksToReset, "ticksToReset", -1);
			Scribe_Values.Look<int>(ref lastKeepDisplayTick, "lastKeepDisplayTick");
		}

		public override IEnumerable<Gizmo> GetWornGizmos()
		{
			AbstractShootyBelt shieldBelt = this;
			foreach (Gizmo gizmo in base.GetWornGizmos())
				yield return gizmo;

			if (Find.Selector.SingleSelectedThing == shieldBelt.Wearer)
				yield return new Gizmo_EnergyShieldStatus_Cheaty()
				{
					shield = shieldBelt
				};
		}

		public override float GetSpecialApparelScoreOffset() => EnergyMax * ApparelScorePerEnergyMax;

		public override void Tick()
		{
			base.Tick();
			if (Wearer == null)
				energy = 0.0f;
			else if (ShieldState == ShieldState.Resetting)
			{
				--ticksToReset;
				if (ticksToReset > 0)
					return;
				Reset();
			}
			else
			{
				if (ShieldState != ShieldState.Active)
					return;
				energy += EnergyGainPerTick;
				if (energy <= (double)EnergyMax)
					return;
				energy = EnergyMax;
			}
		}

		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (ShieldState != ShieldState.Active)
				return false;
			if (dinfo.Def == DamageDefOf.EMP)
			{
				energy = 0.0f;
				Break();
				return false;
			}
			if (!dinfo.Def.isRanged && !dinfo.Def.isExplosive)
				return false;
			energy -= dinfo.Amount * EnergyLossPerDamage;
			if (energy < 0.0)
				Break();
			else
				AbsorbedDamage(dinfo);
			return true;
		}

		public void KeepDisplaying() => lastKeepDisplayTick = Find.TickManager.TicksGame;

		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = Wearer.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float scale = Mathf.Min(10f, (float)(2.0 + dinfo.Amount / 10.0));
			FleckMaker.Static(loc, Wearer.Map, FleckDefOf.ExplosionFlash, scale);
			int num = (int)scale;
			for (int index = 0; index < num; ++index)
				FleckMaker.ThrowDustPuff(loc, Wearer.Map, Rand.Range(0.8f, 1.2f));
			lastAbsorbDamageTick = Find.TickManager.TicksGame;
			KeepDisplaying();
		}

		private void Break()
		{
			SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
			FleckMaker.Static(Wearer.TrueCenter(), Wearer.Map, FleckDefOf.ExplosionFlash, 12f);
			for (int index = 0; index < 6; ++index)
				FleckMaker.ThrowDustPuff(Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), Wearer.Map, Rand.Range(0.8f, 1.2f));
			energy = 0.0f;
			ticksToReset = StartingTicksToReset;
		}

		private void Reset()
		{
			if (Wearer.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
				FleckMaker.ThrowLightningGlow(Wearer.TrueCenter(), Wearer.Map, 3f);
			}
			ticksToReset = -1;
			energy = EnergyOnReset;
		}

		public override void DrawWornExtras()
		{
			if (ShieldState != ShieldState.Active || !ShouldDisplay)
				return;

			float scaleBase = Mathf.Lerp(1.22f, 1.85f, energy);
			const float z_offset = 0.15f;

			if (AdvancedVersion)
			{
				if (Bubble_Background != null)
				{
					Vector3 background_DrawPos = Wearer.Drawer.DrawPos;
					background_DrawPos.z += z_offset;
					float vanillaPulse = (float)(0.300000011920929 + (Math.Sin((Time.realtimeSinceStartup + 397.0 * (Wearer.thingIDNumber % 571)) * 4.0) + 1.0) * 0.5 * 0.699999988079071);
					Material Bubble_Background_Faded = FadedMaterialPool.FadedVersionOf(Bubble_Background, vanillaPulse);
					Matrix4x4 background_Matrix = new Matrix4x4();
					background_Matrix.SetTRS(background_DrawPos, Quaternion.AngleAxis(/* Angle */90f, Vector3.up), new Vector3(scaleBase, 1f, scaleBase));
					Graphics.DrawMesh(MeshPool.plane10, background_Matrix, Bubble_Background_Faded, 0);
				}

				if (Bubble_Foreground != null)
				{
					Vector3 foreground_DrawPos = Wearer.Drawer.DrawPos;
					foreground_DrawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
					foreground_DrawPos.z += z_offset;
					float num1 = scaleBase;
					int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
					if (num2 < 8)
					{
						float num3 = (float)((double)(8 - num2) / 8.0 * 0.0500000007450581);
						foreground_DrawPos += this.impactAngleVect * num3;
						num1 -= num3;
					}
					float angle = Rand.Range(0, 360);
					Matrix4x4 matrix = new Matrix4x4();
					matrix.SetTRS(foreground_DrawPos, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(num1, 1f, num1));
					Graphics.DrawMesh(MeshPool.plane10, matrix, Bubble_Foreground, 0);
				}
			}
			else
			{
				Vector3 foreground_DrawPos = Wearer.Drawer.DrawPos;
				foreground_DrawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				foreground_DrawPos.z += z_offset;
				float num1 = scaleBase;
				int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
				if (num2 < 8)
				{
					float num3 = (float)((double)(8 - num2) / 8.0 * 0.0500000007450581);
					foreground_DrawPos += this.impactAngleVect * num3;
					num1 -= num3;
				}
				float angle = Rand.Range(0, 360);
				Matrix4x4 matrix = new Matrix4x4();
				matrix.SetTRS(foreground_DrawPos, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(num1, 1f, num1));
				Graphics.DrawMesh(MeshPool.plane10, matrix, Bubble_Foreground, 0);
			}
		}

		public bool CompAllowVerbCast(Verb _) => true;
	}

	public class CheatBelt : AbstractShootyBelt
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			AdvancedVersion = true;
		}
	}

	public class WeakBelt : AbstractShootyBelt
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			AdvancedVersion = false;
		}
	}
}
