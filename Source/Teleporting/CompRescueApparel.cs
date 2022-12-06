using alaestor_teleporting;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace alaestor_teleporting_cheatbelt
{
	class CompRescueApparel : ThingComp
	{
		public CompProperties_RescueApparel Props => (CompProperties_RescueApparel)props;

		public CompNameLinkable nameLinkableComp;

		public bool HasNameLinkableComp =>
			nameLinkableComp != null;

		public CompTeleportApparel teleportApparelComp;
		public bool HasTeleportApparelComp => teleportApparelComp != null;
		public bool CanRescueTeleport =>
			HasTeleportApparelComp
			&& teleportApparelComp.UseNameLinkable
			&& HasNameLinkableComp
			&& nameLinkableComp.CanBeLinked
			&& nameLinkableComp.HasValidLinkedThing;

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
					return null;
				}
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			nameLinkableComp = parent.GetComp<CompNameLinkable>();
			teleportApparelComp = parent.GetComp<CompTeleportApparel>();
		}

		// things are unequiped when a pawn dies. Need to know _before_ pawn dies.
		public override void CompTick()
		{
			base.CompTick();

			Pawn pawn = Wearer;

			if (pawn != null)
			{
				bool isDead = pawn.health.Dead;
				bool isDowned = pawn.health.Downed;
				if (isDead || isDowned)
				{

					if (CanRescueTeleport)
					{
						teleportApparelComp.StartTeleport_LinkedThing();
					}

					if (isDead)
					{
						ResurrectionUtility.ResurrectWithSideEffects(pawn);
					}
					else // if (isDowned)
					{
						var healer = new CompUseEffect_FixWorstHealthCondition();
						healer.DoEffect(pawn);
						Messages.Message(string.Format("{0}'s emergency rescue system was activated", Wearer), pawn, MessageTypeDefOf.SituationResolved);
					}
				}
			}
		}
	}

	public class CompProperties_RescueApparel : CompProperties
	{
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
				yield return configError;
		}

		public CompProperties_RescueApparel()
		{
			compClass = typeof(CompRescueApparel);
		}

		public CompProperties_RescueApparel(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}
