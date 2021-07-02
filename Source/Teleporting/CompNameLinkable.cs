using System;
using System.Collections.Generic;
using Verse;

namespace alaestor_teleporting
{
	/*
	[StaticConstructorOnStartup]
	class NameLinkableManager
	{
		static public Dictionary<string, Thing> nameLinkableThings;

		public static bool NameExists(string linkableName) => nameLinkableThings.ContainsKey(linkableName);
		public static bool NameIsAvailable(string linkableName) => !nameLinkableThings.ContainsKey(linkableName);
	}
	*/

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
			this.curName = initialString;
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

		protected override void SetName(string name) => this.Output_Callback(name); //this.nameLinkable.Name = name; // = this.curName
	}

	public class CompNameLinkable : ThingComp
	{
		private static readonly Dictionary<string, Thing> nameLinkableThings = new Dictionary<string, Thing>();

		public static bool NameExists(string linkableName) => nameLinkableThings.ContainsKey(linkableName);
		public static bool NameIsAvailable(string linkableName) => !nameLinkableThings.ContainsKey(linkableName);


		public CompProperties_NameLinkable Props => (CompProperties_NameLinkable)this.props;
		public bool CanBeNamed => Props.canBeNamed; // TODO 
		public bool CanBeLinked => Props.canBeLinked; // TODO

		//
		// NAME stuff
		//

		private string name = null;

		public string Name
		{
			get => name;
			set
			{
				if (NameIsAvailable(value))
				{
					if (name != null && nameLinkableThings.ContainsKey(name))
						nameLinkableThings.Remove(name);

					name = value;

					if (!name.NullOrEmpty())
						nameLinkableThings.Add(name, parent);
				}
				else Logger.Error("CompNameLinkable::Name::set: Tried to name thing\"" + value + "\" but it already exists");
			}
		}

		//
		// LINK stuff
		//

		private string linkedToName = null;
		private Thing linkedToThing = null;

		public bool IsLinkedToSomething => linkedToName != null && NameExists(linkedToName);

		public string LinkedName => linkedToName;

		public Thing LinkedThing
		{
			get
			{
				if (IsLinkedToSomething)
				{
					if (linkedToThing != null && linkedToThing.Spawned)
					{
						return linkedToThing;
					}
					else
					{
						var thing = nameLinkableThings[linkedToName];
						if (thing != null && thing.Spawned)
						{
							linkedToThing = thing;
							return linkedToThing;
						}
					}
				}
				return null;
			}
		}

		public void LinkTo(string linkableName)
		{
			if (NameExists(linkableName))
			{
				linkedToName = linkableName;
				linkedToThing = nameLinkableThings[linkedToName];
			}
			else Logger.Warning("Tried to link to non-existent nameLinkable \"" + linkableName + "\"");
		}

		public bool TryLinkTo(string linkableName)
		{
			if (NameExists(linkableName))
			{
				LinkTo(linkableName);

				Logger.Debug("CompNameLinkable::TryLinkTo: \""
					+ (name.NullOrEmpty() ? "(noname)" : name)
					+ "\" linked to \"" + linkableName + "\"");

				return true;
			}
			else
			{
				Logger.Debug("CompNameLinkable::TryLinkTo: Tried to link \""
					+ (name.NullOrEmpty() ? "(noname)" : name)
					+ "\" to \"" + linkableName
					+ "\" but it doesn't exist");

				return false;
			}
		}

		public void Unlink()
		{
			linkedToName = null;
			linkedToThing = null;
		}

		//
		// OBJECT stuff
		//

		public override void CompTickRare()
		{
			base.CompTickRare();
			// if 
		}

		public override void PostDraw()
		{
			if (false) // is link invalid?
			{// overlay broken link
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
			if (!Name.NullOrEmpty())
			{
				if (s.Length != 0)
					s += "\n";

				s += "Name: \"" + Name + "\"";
			}

			if (s.Length != 0)
				s += "\n";

			if (IsLinkedToSomething)
			{
				s += "Linked to \"" + linkedToName + "\"";
			}
			else s += "Unlinked.";

			return s;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<string>(ref this.name, "name", null);
			Scribe_Values.Look<string>(ref this.linkedToName, "linkedToName", null);
			//Scribe_Collections.Look<>

			if (Scribe.mode == LoadSaveMode.LoadingVars) //LoadSaveMode.PostLoadInit ?
			{
				if (!name.NullOrEmpty() && parent != null)
				{
					if (NameIsAvailable(name))
					{
						nameLinkableThings.Add(name, parent);
						Logger.Debug("CompNameLinkable::PostExposeData: registered \"" + name + "\" to " + parent.Label);
					}
					else
					{
						nameLinkableThings[name] = parent;
						Logger.Debug("CompNameLinkable::PostExposeData: updated \"" + name + "\" with " + parent.Label);
					}
					//Logger.Error("CompNameLinkable::PostExposeData: \"" + name + "\" was not available (already exists)");
				}
				else if (parent == null) Logger.Error("CompNameLinkable::PostExposeData: parent was null?!");
			}
			else if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (!linkedToName.NullOrEmpty())
				{
					//if (NameExists(linkedToName))
					LinkTo(linkedToName);
				}
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
		}

		// TODO these 3 gizmos should all be part of one button + a status icon (linked/unlinked)
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				yield return gizmo;

			AcceptanceReport Rename_Validator(string newName)
			{
				if (CompNameLinkable.NameIsAvailable(newName))
				{
					return (AcceptanceReport)true;
				}
				else return (AcceptanceReport)"NameIsInUse".Translate();
			}

			void Rename(string newName)
			{
				Logger.Debug(
					"CompNameLinkable::Rename: "
					+ (Name != null ? Name : "(unnamed)")
					+ " to " + newName
				);

				Name = newName;
			}

			yield return new Command_Action
			{
				//icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone"),
				defaultLabel = "CompNameLinkable_Gizmo_Rename_Label".Translate(),
				defaultDesc = "CompNameLinkable_Gizmo_Rename_Desc".Translate(),
				activateSound = SoundDef.Named("Click"),
				action = delegate
				{
					Logger.DebugVerbose("CompNameLinkable:: called Gizmo: rename");
					Find.WindowStack.Add(new Dialog_NameInputWindow((Name ?? "(unnamed)"), Rename_Validator, Rename));
				}
			};

			AcceptanceReport MakeLink_Validator(string newName)
			{
				if (CompNameLinkable.NameExists(newName))
				{
					return (AcceptanceReport)true;
				}
				else return (AcceptanceReport)("Couldn't find " + newName); // TODO format and translate
			}

			void MakeLink(string linkableName)
			{
				if (TryLinkTo(linkableName))
				{
					Logger.Debug(
						"CompNameLinkable::MakeLink: "
						+ (Name ?? "(unnamed)")
						+ " linked to " + linkableName
					);
				}
				else
				{
					Logger.Debug(
						"CompNameLinkable::MakeLink: "
						+ (Name ?? "(unnamed)")
						+ " failed to link to " + linkableName
					);
				}
			}

			yield return new Command_Action
			{
				//icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone"),
				defaultLabel = "CompNameLinkable_Gizmo_MakeLink_Label".Translate(),
				defaultDesc = "CompNameLinkable_Gizmo_MakeLink_Desc".Translate(),
				activateSound = SoundDef.Named("Click"),
				action = delegate
				{
					Logger.DebugVerbose("CompNameLinkable:: called Gizmo: make link");
					Find.WindowStack.Add(new Dialog_NameInputWindow("", MakeLink_Validator, MakeLink));
				}
			};

			yield return new Command_Action
			{
				//icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone"),
				defaultLabel = "CompNameLinkable_Gizmo_Unlink_Label".Translate(),
				defaultDesc = "CompNameLinkable_Gizmo_Unlink_Desc".Translate(),
				activateSound = SoundDef.Named("Click"),
				action = delegate
				{
					Logger.DebugVerbose("CompNameLinkable:: called Gizmo: unlink");
					Unlink();
				}
			};

			//if (DebugSettings.godMode)
		}
	}

	public class CompProperties_NameLinkable : CompProperties
	{
		public bool canBeNamed = true;
		public bool canBeLinked = true;

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
				yield return configError;
			//if (targetQuantityConfigurable == true && )
		}

		public CompProperties_NameLinkable()
		{
			this.compClass = typeof(CompNameLinkable);
		}

		public CompProperties_NameLinkable(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}
}
