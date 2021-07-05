using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	public class CompCooldown : ThingComp
	{
		public CompProperties_Cooldown Props => (CompProperties_Cooldown)this.props;
		public bool ShowDebugGizmos => Props.showDebugGizmos; // TODO

		public int remaining;

		public bool IsOnCooldown => this.remaining > 0;
		public static implicit operator bool(CompCooldown c) => c.IsOnCooldown;

		public void Reset()
		{
			this.remaining = 0;
			Logger.DebugVerbose("CompCooldown cooldown reset");
		}

		public void Set(int ticks)
		{
			if (ticks > 0) this.remaining = ticks;
			else this.remaining = 0;
			Logger.DebugVerbose("CompCooldown cooldown set to " + ticks.ToString() + " ticks (" + ((int)(ticks / 60)).ToString() + " seconds)");
		}

		public void SetSeconds(int seconds)
		{
			this.Set(seconds * 60);
		}

		public void Add(int ticks)
		{
			if (ticks > 0) this.remaining += ticks;
			else Logger.Error("CompCooldown Tried to add negative cooldown time");
			Logger.DebugVerbose("CompCooldown cooldown increased by adding " + ticks.ToString() + " ticks (" + ((int)(ticks / 60)).ToString() + " seconds)");
		}

		public void AddSeconds(int seconds)
		{
			this.Add(seconds * 60);
		}

		public void Subtract(int ticks)
		{
			if (this.remaining > 0)
			{
				if (this.remaining - ticks >= 0) this.remaining -= ticks;
				else this.remaining = 0;
			}
		}

		public void SubtractSeconds(int seconds)
		{
			if (this.remaining > 0)
			{
				if (this.remaining - (seconds * 60) >= 0) this.remaining -= seconds * 60;
				else this.remaining = 0;
			}
		}

		public override void CompTick()
		{ // ticks every 1/60th second (1t / 60tps)
			base.CompTick();
			this.Subtract(1);
		}

		public override void CompTickRare()
		{ // ticks every 4.16 seconds (250t / 60tps)
			base.CompTickRare();
			this.Subtract(250);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.remaining, "Remaining", 0);
		}

		public override void PostDraw()
		{
			if (this.IsOnCooldown)
			{// overlay cooldown icon on thing
				Thing thing = this.parent;
				float vanillaPulse = (float)(0.300000011920929 + (Math.Sin(((double)Time.realtimeSinceStartup + 397.0 * (double)(thing.thingIDNumber % 571)) * 4.0) + 1.0) * 0.5 * 0.699999988079071);
				Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom("Overlay/Cooldown", ShaderDatabase.MetaOverlay), vanillaPulse);
				Matrix4x4 matrix = new Matrix4x4();
				matrix.SetTRS(thing.DrawPos, Quaternion.AngleAxis(0.0f, Vector3.up), new Vector3(0.6f, 1f, 0.6f));
				Graphics.DrawMesh(MeshPool.plane14, matrix, material, 0);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (this.IsOnCooldown)
			{
				return "IsOnCooldown_Label".Translate() + ": " + ((int)(remaining / 60)).ToString() + " seconds remaining";
			}
			else return "";
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			if (ShowDebugGizmos && DebugSettings.godMode)
			{
				yield return GizmoHelper.MakeCommandAction(
					"CompCooldown_SetCool_Debug",
					delegate
					{
						Logger.Debug("CompCooldown: called Godmode Gizmo: cooldown");
						this.Reset();
					}
				);
			}
		}
	}

	public class CompProperties_Cooldown : CompProperties
	{
		public bool showDebugGizmos = true;

		public CompProperties_Cooldown()
		{
			this.compClass = typeof(CompCooldown);
		}

		public CompProperties_Cooldown(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}// namespace alaestor_teleporting
