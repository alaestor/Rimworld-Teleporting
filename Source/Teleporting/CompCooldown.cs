using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	public class CompCooldown : ThingComp
	{
		public CompProperties_Cooldown Props => (CompProperties_Cooldown)props;
		public bool ShowGizmos => Props.showGizmos;
		public bool ShowDebugGizmos => Props.showDebugGizmos;

		private int remaining;

		public int TicksRemaining => remaining;
		public int SecondsRemaining => remaining / 60;

		public bool IsOnCooldown => TicksRemaining > 0;
		public static implicit operator bool(CompCooldown c) => c.IsOnCooldown;

		public void Reset()
		{
			remaining = 0;
			Logger.DebugVerbose("CompCooldown cooldown reset");
		}

		public void Set(int ticks)
		{
			if (ticks > 0) remaining = ticks;
			else remaining = 0;
			Logger.DebugVerbose("CompCooldown cooldown set to " + ticks.ToString() + " ticks (" + (ticks / 60).ToString() + " seconds)");
		}

		public void SetSeconds(int seconds)
		{
			Set(seconds * 60);
		}

		public void Add(int ticks)
		{
			if (ticks > 0) remaining += ticks;
			else Logger.Error("CompCooldown Tried to add negative cooldown time");
			Logger.DebugVerbose("CompCooldown cooldown increased by adding " + ticks.ToString() + " ticks (" + (ticks / 60).ToString() + " seconds)");
		}

		public void AddSeconds(int seconds)
		{
			Add(seconds * 60);
		}

		public void Subtract(int ticks)
		{
			if (remaining > 0)
			{
				if (remaining - ticks >= 0) remaining -= ticks;
				else remaining = 0;
			}
		}

		public void SubtractSeconds(int seconds)
		{
			if (remaining > 0)
			{
				if (remaining - (seconds * 60) >= 0) remaining -= seconds * 60;
				else remaining = 0;
			}
		}

		public override void CompTick()
		{ // ticks every 1/60th second (1t / 60tps)
			base.CompTick();
			Subtract(1);
		}

		public override void CompTickRare()
		{ // ticks every 4.16 seconds (250t / 60tps)
			base.CompTickRare();
			Subtract(250);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref remaining, "remaining", 0);
		}

		public override void PostDraw()
		{
			if (IsOnCooldown)
			{// overlay cooldown icon on thing
				Thing thing = parent;
				float vanillaPulse = (float)(0.300000011920929 + (Math.Sin((Time.realtimeSinceStartup + 397.0 * (thing.thingIDNumber % 571)) * 4.0) + 1.0) * 0.5 * 0.699999988079071);
				Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom("Overlay/Cooldown", ShaderDatabase.MetaOverlay), vanillaPulse);
				Matrix4x4 matrix = new Matrix4x4();
				matrix.SetTRS(thing.DrawPos, Quaternion.AngleAxis(0.0f, Vector3.up), new Vector3(0.6f, 1f, 0.6f));
				Graphics.DrawMesh(MeshPool.plane14, matrix, material, 0);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (IsOnCooldown)
			{
				return string.Format("Teleporting_CooldownComp_Inspect_OnCooldown_FMT".Translate(), SecondsRemaining.ToString());
			}
			else return "";
		}

		private IEnumerable<Gizmo> CompCommonGizmosExtra()
		{
			if (ShowGizmos)
			{
				if (ShowDebugGizmos && DebugSettings.godMode)
				{
					yield return GizmoHelper.MakeCommandAction(
						"CompCooldown_SetCool_Debug",
						delegate
						{
							Logger.Debug("CompCooldown: called Godmode Gizmo: cooldown");
							Reset();
						}
					);
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
				yield return gizmo;

			foreach (Gizmo gizmo in CompCommonGizmosExtra())
				yield return gizmo;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			foreach (Gizmo gizmo in CompCommonGizmosExtra())
				yield return gizmo;
		}
	}

	public class CompProperties_Cooldown : CompProperties
	{
		public bool showGizmos = true;
		public bool showDebugGizmos = true;

		public CompProperties_Cooldown()
		{
			compClass = typeof(CompCooldown);
		}

		public CompProperties_Cooldown(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}// namespace alaestor_teleporting
