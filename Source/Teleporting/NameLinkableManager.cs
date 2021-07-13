using System.Collections.Generic;
using Verse;

namespace alaestor_teleporting
{
	[StaticConstructorOnStartup]
	class NameLinkableManager
	{
		private static readonly Dictionary<string, Thing> nameLinkableThings = new Dictionary<string, Thing>();

		public static bool NameExists(string linkableName) => nameLinkableThings.ContainsKey(linkableName);
		public static bool NameIsAvailable(string linkableName) => !nameLinkableThings.ContainsKey(linkableName);

		public static bool IsLinkedThingValid(string linkableName)
		{
			return (!linkableName.NullOrEmpty())
				&& NameExists(linkableName)
				&& nameLinkableThings[linkableName].Spawned;
		}

		public static Thing GetLinkedThing(string linkableName)
		{
			if (!linkableName.NullOrEmpty())
			{
				if (NameExists(linkableName))
				{
					return nameLinkableThings[linkableName];
				}
				else
				{
					Logger.Error("NameLinkableManager::GetLinkedThing: LinkableName " + linkableName + " doesn't exist");
				}
			}
			else Logger.Error("NameLinkableManager::GetLinkedThing: Got null or empty parameter");
			return null;
		}

		public static bool RegisterOrUpdate(string linkableName, Thing thing)
		{
			if (!linkableName.NullOrEmpty() && thing != null)
			{
				if (NameIsAvailable(linkableName))
				{
					nameLinkableThings.Add(linkableName, thing);
					Logger.DebugVerbose("NameLinkableManager::TryToRegister: Registered \"" + linkableName + "\" with " + thing.Label);
				}
				else
				{
					nameLinkableThings[linkableName] = thing;
					Logger.DebugVerbose("NameLinkableManager::TryToRegister: Updated \"" + linkableName + "\" with " + thing.Label);
				}
				return true;
			}
			else
			{
				Logger.Error(
					"NameLinkableManager::TryToRegister: Got null or empty parameter",
					"linkableName: " + linkableName.ToString(),
					"thing: " + thing.ToString()
				);
			}
			return false;
		}

		public static bool TryToUnregister(string linkableName)
		{
			if (!linkableName.NullOrEmpty())
			{
				if (NameExists(linkableName))
				{
					nameLinkableThings.Remove(linkableName);
					Logger.DebugVerbose("NameLinkableManager::TryToUnregister: Unregistered \"" + linkableName + "\"");
					return true;
				}
				else
				{
					Logger.DebugVerbose(
						"NameLinkableManager::TryToUnregister: Tried to unregister \""
						+ linkableName + "\" but it wasn't registered"
					);
				}
			}
			else Logger.DebugVerbose("NameLinkableManager::TryToUnregister: Got null or empty parameter");
			return false;
		}
	}
}
