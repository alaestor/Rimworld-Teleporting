using System;
using System.Collections.Generic;
using Verse;

namespace alaestor_teleporting
{
	// TODO this should probably be its own file
	public class Dialog_NameInputWindow : Dialog_Rename
	{
		//private readonly CompNameLinkable nameLinkable;
		private readonly Action<string> Output_Callback;
		private readonly Func<string, AcceptanceReport> Validator;

		public Dialog_NameInputWindow(
			string initialString,
			Func<string, AcceptanceReport> Validator,
			Action<string> Output_Callback)
		{
			//this.nameLinkable = nameLinkableComp;
			curName = initialString;
			this.Validator = Validator;
			this.Output_Callback = Output_Callback;
		}

		protected override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport acceptanceReport = base.NameIsValid(name);
			if (!acceptanceReport.Accepted)
			{
				return acceptanceReport;
			}
			else return Validator(name);
			/*
			else if (nameLinkable.NameIsAvailable(name))
			{
				return (AcceptanceReport)true;
			}
			else return (AcceptanceReport)"NameIsInUse".Translate(); // vanilla translated string
			*/
		}

		protected override void SetName(string name) => Output_Callback(name); //this.nameLinkable.Name = name; // = this.curName
	}

	public class CompNameLinkable : ThingComp
	{
		public CompProperties_NameLinkable Props => (CompProperties_NameLinkable)props;
		public bool CanBeNamed => Props.canBeNamed;
		public bool CanBeLinked => Props.canBeLinked;
		public bool ShowGizmos => Props.showGizmos;
		public bool ShowDebugGizmos => Props.showDebugGizmos;

		//
		// NAME stuff
		//

		private string name = null;

		public bool IsNamed => name != null;

		public string Name
		{
			get => name ?? "Teleporting_CompNameLinkable_Unnamed".Translate();
			set
			{
				if (CanBeNamed)
				{
					if (!value.NullOrEmpty())
					{
						if (NameLinkableManager.NameIsAvailable(value))
						{
							Logger.DebugVerbose("CompNameLinkable::Name::set: Changed \"" + (name ?? "(unnamed)").ToString() + "\" to \"" + value + "\"");
							NameLinkableManager.TryToUnregister(name);
							name = value;
							NameLinkableManager.RegisterOrUpdate(name, parent);
						}
						else Logger.Error("CompNameLinkable::Name::set: Tried to name thing\"" + value + "\" but it already exists");
					}
					else Logger.Error("CompNameLinkable::Name::set: Tried to name thing null or empty string");
				}
				else Logger.Error("CompNameLinkable::Name::set: Tried to name thing but thing CanBeNamed is false");
			}
		}

		public static bool IsNewNameValid(string newName)
		{
			return NameLinkableManager.NameIsAvailable(newName);
		}

		public void BeginRename()
		{
			if (CanBeNamed)
			{
				Logger.DebugVerbose("CompNameLinkable::BeginRename: called");
				AcceptanceReport Rename_Validator(string newName)
				{
					if (NameLinkableManager.NameIsAvailable(newName))
					{
						return true;
					}
					else return "Teleporting_CompNameLinkable_NameIsInUse".Translate();
				}

				Find.WindowStack.Add(new Dialog_NameInputWindow(name ?? "", Rename_Validator, Rename_Callback));

				void Rename_Callback(string newName)
				{
					Name = newName;
				}
			}
			else Logger.Error("CompNameLinkable::BeginRename: called but CanBeNamed is false!");
		}

		//
		// LINK stuff
		//

		private string linkedName = null;

		public Thing LinkedThing => HasValidLinkedThing ? NameLinkableManager.GetLinkedThing(linkedName) : null;

		public bool IsLinkedToSomething => linkedName != null;
		public bool IsLinkedToValidName => linkedName != null && NameLinkableManager.NameExists(linkedName);
		public bool HasValidLinkedThing => linkedName != null && NameLinkableManager.IsLinkedThingValid(linkedName);
		public bool HasInvalidLinkedThing => !HasValidLinkedThing;

		public string GetNameOfLinkedLinkedThing => linkedName ?? "Teleporting_CompNameLinkable_Unlinked".Translate();

		public void LinkTo(string linkableName)
		{
			if (CanBeLinked)
			{
				if (NameLinkableManager.NameExists(linkableName))
				{
					linkedName = linkableName;
					Logger.DebugVerbose("CompNameLinkable::LinkTo: \"" + Name + "\" linked to \"" + linkableName + "\"");
				}
				else Logger.Warning("Tried to link to non-existent nameLinkable \"" + linkableName + "\"");
			}
			else Logger.Error("CompNameLinkable::LinkTo: called but CanBeLinked is false!");
		}

		public bool TryLinkTo(string linkableName)
		{
			if (CanBeLinked)
			{
				if (NameLinkableManager.NameExists(linkableName))
				{
					LinkTo(linkableName);
					return true;
				}
				else
				{
					Logger.DebugVerbose("CompNameLinkable::TryLinkTo: Tried to link \""
						+ Name + "\" to \"" + linkableName + "\" but it doesn't exist"
					);
				}
			}
			else Logger.Error("CompNameLinkable::LinkTo: called but CanBeLinked is false!");
			return false;
		}

		public bool Unlink()
		{
			if (CanBeLinked)
			{
				if (linkedName != null)
				{
					Logger.Debug("CompNameLinkable::Unlink: Unlinked \"" + Name + "\" from \"" + linkedName + "\"");
					linkedName = null;
					return true;
				}
				else Logger.Debug("CompNameLinkable::Unlink: Tried to unlink \"" + Name + "\" but it wasn't linked to anything.");
			}
			else Logger.Error("CompNameLinkable::Unlink: called but CanBeLinked is false!");
			return false;
		}

		public void BeginMakeLinkName()
		{
			if (CanBeLinked)
			{
				Logger.DebugVerbose("CompNameLinkable::BeginMakeLink: called");
				AcceptanceReport MakeLink_Validator(string linkableName)
				{
					if (NameLinkableManager.IsLinkedThingValid(linkableName))
					{
						return true;
					}
					else return string.Format("Teleporting_CompNameLinkable_CouldntFindName_FMT".Translate(), linkableName);
				}

				Find.WindowStack.Add(new Dialog_NameInputWindow("", MakeLink_Validator, MakeLink_Callback));

				void MakeLink_Callback(string linkableName)
				{
					TryLinkTo(linkableName);
				}
			}
			else Logger.Error("CompNameLinkable::BeginMakeLink: called but CanBeLinked is false!");
		}

		public void BeginMakeLinkTarget()
		{
			if (CanBeLinked)
			{
				bool TargetValidator(LocalTargetInfo target)
				{
					if (target.HasThing
						&& target.Thing is ThingWithComps thing
						&& thing.GetComp<CompNameLinkable>() is CompNameLinkable nameLinkable)
					{
						if ( nameLinkable.IsNamed)
						{
							return true;
						}
						else MsgHelper.Reject("CompNameLinkable_MakeLinkTarget_Unnamed");
					}
					else MsgHelper.Reject("CompNameLinkable_MakeLinkTarget_NotLinkable");

					return false;
				}

				TeleportTargeter.StartChoosingLocal(
					startingFrom: parent,
					MakeLink_Callback,
					targetParams: new RimWorld.TargetingParameters { canTargetBuildings = true, canTargetItems = true },
					canTargetValidator: TargetValidator
				);

				void MakeLink_Callback(LocalTargetInfo target)//string linkableName)
				{
					if (target.HasThing && target.Thing is Building_TeleportPlatform platform)
					{
						var nameLinkable = platform.GetComp<CompNameLinkable>();
						if (nameLinkable.IsNamed)
						{
							TryLinkTo(nameLinkable.name);
						}
						else Logger.Error("CompNameLinkable::BeginMakeLinkTarget: tried to select unnamed thing, error");
					}
				}
			}
			else Logger.Error("CompNameLinkable::BeginMakeLinkTarget: called but CanBeLinked is false!");
		}

		//
		// OBJECT stuff
		//

		// TODO either use or delete this
		public override void CompTickRare()
		{
			base.CompTickRare();
			// if 
		}

		public override void PostDraw()
		{
			if (!IsLinkedToSomething) // unlinked
			{

			}
			if (HasInvalidLinkedThing) // link broken
			{
				/*
				Thing thing = this.parent;
				float vanillaPulse = (float)(0.300000011920929 + (Math.Sin(((double)Time.realtimeSinceStartup + 397.0 * (double)(thing.thingIDNumber % 571)) * 4.0) + 1.0) * 0.5 * 0.699999988079071);
				Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom("Overlay/Cooldown", ShaderDatabase.MetaOverlay), vanillaPulse);
				Matrix4x4 matrix = new Matrix4x4();
				matrix.SetTRS(thing.DrawPos, Quaternion.AngleAxis(0.0f, Vector3.up), new Vector3(0.6f, 1f, 0.6f));
				Graphics.DrawMesh(MeshPool.plane14, matrix, material, 0);
				*/
			}
		}

		public override string CompInspectStringExtra()
		{
			string s = "";

			if (CanBeNamed)
			{
				if (IsNamed)
				{
					s += string.Format("Teleporting_CompNameLinkable_Inspect_IsNamed_FMT".Translate(), Name); 
				}
				else
				{
					s += "Teleporting_CompNameLinkable_Inspect_IsNotNamed".Translate();
				}
			}

			if (CanBeLinked)
			{
				if (s.Length != 0)
					s += "\n";

				if (HasValidLinkedThing)
				{
					s += string.Format("Teleporting_CompNameLinkable_Inspect_IsLinked_FMT".Translate(), linkedName);
				}
				else s += "Teleporting_CompNameLinkable_Inspect_IsNotLinked".Translate();
			}

			return s;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<string>(ref name, "name", null);
			Scribe_Values.Look<string>(ref linkedName, "linkedToName", null);
			//Scribe_Collections.Look<>

			if (Scribe.mode == LoadSaveMode.LoadingVars) //LoadSaveMode.PostLoadInit ?
			{
				if (IsNamed && parent != null)
				{
					NameLinkableManager.RegisterOrUpdate(name, parent);
				}
				else if (parent == null) Logger.Error("CompNameLinkable::PostExposeData: parent was null?!");
			}
			else if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				//? TODO either use or delete this
			}
		}

		public override void Initialize(CompProperties props)
		{
			// TODO use or delete this
			base.Initialize(props);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			NameLinkableManager.TryToUnregister(Name);
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			NameLinkableManager.TryToUnregister(Name);
		}

		private IEnumerable<Gizmo> CompCommonGizmosExtra()
		{
			if (ShowGizmos)
			{
				if (CanBeNamed)
				{
					yield return GizmoHelper.MakeCommandAction(
						"CompNameLinkable_Rename",
						delegate
						{
							BeginRename();
							Logger.Debug("TeleportPlatform: called Gizmo: rename");
						}
					//icon: ContentFinder<Texture2D>.Get("UI/Commands/..."),
					);
				}

				if (IsLinkedToSomething)
				{
					yield return GizmoHelper.MakeCommandAction(
						"CompNameLinkable_Unlink",
						delegate
						{
							Unlink();
							Logger.Debug("CompNameLinkable: called Gizmo: unlink");
						}
					//icon: ContentFinder<Texture2D>.Get("UI/Commands/..."),
					);

				}
			}

			if (ShowDebugGizmos)
			{
				if (DebugSettings.godMode)
				{
					if (CanBeNamed)
					{
						yield return GizmoHelper.MakeCommandAction(
							"CompNameLinkable_Rename_Debug",
							delegate
							{
								Logger.DebugVerbose("CompNameLinkable: called debug Gizmo: rename");
								BeginRename();
							}
						);
					}

					if (CanBeLinked)
					{
						if (IsLinkedToSomething)
						{
							yield return GizmoHelper.MakeCommandAction(
								"CompNameLinkable_Unlink_Debug",
								delegate
								{
									Logger.DebugVerbose("CompNameLinkable: called debug Gizmo: unlink");
									Unlink();
								}
							);
						}
						else
						{
							yield return GizmoHelper.MakeCommandAction(
								"CompNameLinkable_MakeLink_Debug",
								delegate
								{
									Logger.DebugVerbose("CompNameLinkable: called debug Gizmo: make link");
									BeginMakeLinkName();
								}
							);

							yield return GizmoHelper.MakeCommandAction(
								"CompNameLinkable_MakeLinkTarget_Debug",
								delegate
								{
									Logger.DebugVerbose("CompNameLinkable: called debug Gizmo: make link targeting");
									BeginMakeLinkTarget();
								}
							);
						}
					}
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

		// TODO these 3 gizmos should all be part of one button + a status icon (linked/unlinked)
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			foreach (Gizmo gizmo in CompCommonGizmosExtra())
				yield return gizmo;
		}
	}

	public class CompProperties_NameLinkable : CompProperties
	{
		public bool canBeNamed = true;
		public bool canBeLinked = true;
		public bool showGizmos = true;
		public bool showDebugGizmos = true;

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
				yield return configError;
		}

		public CompProperties_NameLinkable()
		{
			compClass = typeof(CompNameLinkable);
		}

		public CompProperties_NameLinkable(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}
