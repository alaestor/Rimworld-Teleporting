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
		public CompProperties_TeleportApparel Props => (CompProperties_TeleportApparel)props;

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

		// Cooldown
		private CompCooldown CooldownComp => parent.GetComp<CompCooldown>() ?? null;
		private bool HasCooldownComp => CooldownComp != null;
		public bool UseCooldown => Props.useCooldown && TeleportingMod.settings.enableCooldown && TeleportingMod.settings.enableCooldown_ApparelComp;

		// NameLinkable
		private CompNameLinkable NameLinkableComp => parent.GetComp<CompNameLinkable>() ?? null;
		private bool HasNameLinkableComp => NameLinkableComp != null;
		public bool UseNameLinkable => Props.useNameLinkable;

		// Consumable fuel
		private bool IsConsumable => Props.limitedUses > 0 && TeleportingMod.settings.enableFuel && TeleportingMod.settings.enableApparelFuel;
		private int fuelRemaining;
		public int FuelRemaining => fuelRemaining;
		public int InitialFuelQuantity
		{
			get
			{
				int mult = Props.limitedUses > 0 ? Props.limitedUses : 1;
				if (CanDoTeleport_LongRange)
				{
					return TeleportingMod.settings.longRange_FuelCost * mult;
				}
				else if (CanDoTeleport_ShortRange)
				{
					return TeleportingMod.settings.shortRange_FuelCost * mult;
				}
				else if (UseNameLinkable)
				{
					return 1 * mult;
				}
				else
				{
					return mult;
				}
			}
		}

		public void ConsumeFuel(int n)
		{
			if (IsConsumable)
			{
				if (fuelRemaining - n >= 0)
				{
					Logger.DebugVerbose("Consuming fuel", "initial: " + InitialFuelQuantity.ToString(), "current: " + fuelRemaining.ToString(), "n: " + n.ToString());
					fuelRemaining -= n;
					if (fuelRemaining == 0)
						SelfDestruct();
				}
				else Logger.Error("CompTeleportApparel::ConsumeFuel: overconsumption");
			}
			else Logger.Error("CompTeleportApparel::ConsumeFuel: tried to consume fuel but is not consumable");
		}

		// Teleport settings
		public bool CanDoTeleport_ShortRange => Props.shortRange;
		public bool CanDoTeleport_LongRange => Props.longRange;
		public bool CanTeleportOthers => Props.canTeleportOthers;

		private void AfterSuccessfulTeleport(bool cheat = false, int setCooldown = 0, int consumeFuel = 0)
		{
			if (!cheat)
			{
				if (UseCooldown)
				{
					if (HasCooldownComp)
					{
						CooldownComp.SetSeconds(setCooldown);
					}
					else Logger.Error("CompTeleportApparel::AfterSuccessfulTeleport: UseCooldown is true but CooldownComp is null");
				}

				if (IsConsumable)
				{
					ConsumeFuel(consumeFuel);
				}
			}
		}

		private void AfterSuccessfulTeleport_Link(bool cheat = false)
		{
			AfterSuccessfulTeleport(
				cheat: cheat,
				setCooldown: TeleportingMod.settings.nameLinkable_CooldownDuration,
				consumeFuel: 0
			);
		}

		private void AfterSuccessfulTeleport_Normal(TeleportData teleportData)
		{
			if (teleportData.longRangeFlag)
			{
				AfterSuccessfulTeleport(
					cheat: teleportData.cheat,
					setCooldown: TeleportingMod.settings.longRange_CooldownDuration,
					consumeFuel: TeleportBehavior.FuelCostToTravel(true, teleportData.distance)
				);
			}
			else
			{
				AfterSuccessfulTeleport(
					cheat: teleportData.cheat,
					setCooldown: TeleportingMod.settings.shortRange_CooldownDuration,
					consumeFuel: TeleportBehavior.FuelCostToTravel(false, teleportData.distance)
				);
			}
		}

		public void StartTeleport_ShortRange(bool cheat = false)
		{
			if (CanDoTeleport_ShortRange)
			{
				if (Wearer != null && Wearer.Map != null)
				{
					if (CanTeleportOthers)
					{
						TeleportBehavior.StartTeleportTargetting(false, Wearer, AfterSuccessfulTeleport_Normal, cheat: cheat);
					}
					else
					{
						TeleportBehavior.StartTeleportPawn(false, Wearer, AfterSuccessfulTeleport_Normal, cheat: cheat);
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

					int fuel = (IsConsumable) ? fuelRemaining : TeleportingMod.settings.longRange_FuelCost;

					if (CanTeleportOthers)
					{
						TeleportBehavior.StartTeleportTargetting(true, Wearer, AfterSuccessfulTeleport_Normal, fuel, cheat);
					}
					else
					{
						TeleportBehavior.StartTeleportPawn(true, Wearer, AfterSuccessfulTeleport_Normal, fuel, cheat);
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
				if (HasNameLinkableComp)
				{
					CompNameLinkable nameLinkable = NameLinkableComp;
					if (nameLinkable.IsLinkedToSomething)
					{
						if (nameLinkable.HasValidLinkedThing)
						{
							Thing destination = nameLinkable.LinkedThing;
							if (destination.Map != null && destination.InteractionCell.IsValid)
							{
								if (CanTeleportOthers)
								{
									TeleportTargeter.StartChoosingLocal(
										Wearer,
										DoTeleport,
										TeleportBehavior.targetTeleportSubjects,
										mouseAttachment: MyTextures.MouseAttachment_SelectPawn);
								}
								else
								{
									DoTeleport(Wearer);
								}

								void DoTeleport(LocalTargetInfo target)
								{
									if (target.IsValid && target.HasThing && target.Thing is Pawn pawn)
									{
										if (TeleportBehavior.ExecuteTeleport(pawn, destination.Map, destination.InteractionCell))
										{
											Logger.Debug(
												"CompTeleportApparel::StartTeleport_LinkedThing::DoTeleport: Teleported "
													+ pawn.Label
													+ " from \"" + nameLinkable.Name
													+ "\" to \"" + nameLinkable.GetNameOfLinkedLinkedThing + "\"",
												"Destination Map: " + destination.Map.ToString(),
												"Destination Cell: " + destination.InteractionCell.ToString()
											);
											AfterSuccessfulTeleport_Link(cheat: cheat);
										}
										else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing::DoTeleport: ExecuteTeleport failed.");
									}
									else
									{
										Logger.Error(
											"CompTeleportApparel::StartTeleport_LinkedThing::DoTeleport: invalid target",
											"valid: " + target.IsValid.ToString(),
											"thing: " + (target.HasThing ? (target.Thing.Label) : "None")
										);
									}
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
				else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: UseNameLinkable is true but NameLinkable is null");
			}
			else Logger.Error("CompTeleportApparel::StartTeleport_LinkedThing: UseNameLinkable is false");
		}
		public void SelfDestruct()
		{
			Logger.DebugVerbose(parent.Label + " self destructed");
			parent.SplitOff(1).Destroy();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref fuelRemaining, "fuelRemaining", IsConsumable ? InitialFuelQuantity : 0);
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			if (UseCooldown && !HasCooldownComp)
			{
				Logger.Error(
					"CompTeleportApparel set to use CompCooldown but has no CompCooldown",
					"Parent: " + parent.Label
				);
			}

			if (UseNameLinkable && !HasNameLinkableComp)
			{
				Logger.Error(
					"CompTeleportApparel set to use CompNameLinkable but has no CompNameLinkable",
					"Parent: " + parent.Label
				);
			}

			if (IsConsumable)
				fuelRemaining = InitialFuelQuantity;
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
				yield return gizmo;

			if (Find.Selector.SingleSelectedThing == Wearer)
			{
				// common comps
				bool isOnCooldown = false;
				string cooldownRemainingString = null;
				if (UseCooldown && HasCooldownComp && CooldownComp.IsOnCooldown)
				{
					isOnCooldown = true;
					cooldownRemainingString = string.Format(
						"Teleporting_CompTeleportApparel_CooldownRemaining_FMT".Translate(),
						CooldownComp.SecondsRemaining
					);
				}

				string fuelRemainingDesc = null;
				if (IsConsumable)
				{
					fuelRemainingDesc = string.Format(
						"Teleporting_CompTeleportApparel_FuelRemaining_FMT".Translate(),
						fuelRemaining
					);
				}

				// gizmos

				if (CanDoTeleport_ShortRange)
				{
					yield return GizmoHelper.MakeCommandAction(
						"TeleportApparel_ShortRange",
						delegate
						{
							Logger.Debug("CompTeleportApparel: called Gizmo: Short Range Teleport");
							StartTeleport_ShortRange();
						},
						icon: MyTextures.Gizmo_Teleport_ShortRange,
						disabled: isOnCooldown,
						disabledReason: cooldownRemainingString,
						description: fuelRemainingDesc
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
						},
						icon: MyTextures.Gizmo_Teleport_LongRange,
						disabled: isOnCooldown,
						disabledReason: cooldownRemainingString,
						description: fuelRemainingDesc
					);
				}

				if (UseNameLinkable)
				{
					if (HasNameLinkableComp)
					{
						var nameLinkable = NameLinkableComp;
						if (nameLinkable.IsLinkedToSomething)
						{
							if (nameLinkable.HasValidLinkedThing)
							{
								yield return GizmoHelper.MakeCommandAction(
									"TeleportApparel_TeleportToLink",
									delegate
									{
										Logger.Debug("CompTeleportApparel: called Gizmo: Teleport to Link");
										StartTeleport_LinkedThing();
									},
									icon: MyTextures.Gizmo_Teleport_Link,
									disabled: isOnCooldown,
									disabledReason: cooldownRemainingString
								);
							}
							else
							{
								yield return GizmoHelper.MakeCommandAction(
									"TeleportApparel_TeleportToLink",
									icon: MyTextures.Gizmo_Link_Broken,
									disabled: true,
									disabledReason: "Teleporting_CompNameLinkable_NotLinked".Translate()
								);
							}


							yield return GizmoHelper.MakeCommandAction(
								"TeleportApparel_Unlink",
								delegate
								{
									Logger.Debug("CompTeleportApparel: called Gizmo: Unlink");
									ConsumeFuel(1);
									nameLinkable.Unlink();
								},
								icon: MyTextures.Gizmo_Unlink,
								description: fuelRemainingDesc
							);
						}
						else
						{
							yield return GizmoHelper.MakeCommandAction(
								"TeleportApparel_MakeLink_Name",
								delegate
								{
									Logger.Debug("CompTeleportApparel: called Gizmo: Make Link");
									nameLinkable.BeginMakeLinkName();
								},
								icon: MyTextures.Gizmo_Link_Make_Name
							);

							yield return GizmoHelper.MakeCommandAction(
								"TeleportApparel_MakeLink_Target",
								delegate
								{
									Logger.Debug("CompTeleportApparel: called Gizmo: Make Link");
									nameLinkable.BeginMakeLinkTarget();
								},
								icon: MyTextures.Gizmo_Link_Make_Target
							);
						}
					}
					else
					{
						Logger.Error("CompTeleportApparel: UseNameLinkable is true but NameLinkable is null",
							"Parent: " + parent.Label
						);
					}
				}

				if (DebugSettings.godMode)
				{
					if (IsConsumable)
					{
						yield return GizmoHelper.MakeCommandAction(
							"TeleportApparel_FillFuel_Debug",
							delegate
							{
								Logger.Debug("CompTeleportApparel: called godmode Gizmo: refill fuel");
								fuelRemaining = InitialFuelQuantity;
							}
						);
					}
				}
			}
		}
	}

	public class CompProperties_TeleportApparel : CompProperties
	{
		public bool shortRange = false;
		public bool longRange = false;
		public bool useNameLinkable = false;
		public bool useCooldown = false;
		public bool canTeleportOthers = false;
		public int limitedUses = -1;

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
			compClass = typeof(CompTeleportApparel);
		}

		public CompProperties_TeleportApparel(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}// namespace alaestor_teleporting
