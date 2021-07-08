using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class GizmoHelper
	{
		public static readonly string gizmo_prefix = TeleportingMod.modname + "_Gizmo_";
		public static readonly string label_suffix = "_Label";
		public static readonly string description_suffix = "_Desc";
		public static readonly string disabled_suffix = "_DisabledReason";

		public static Command_Action MakeCommandAction(
			string name,
			Action action = null,
			SoundDef activateSound = null,
			Texture2D icon = null,
			bool disabled = false,
			string disabledReason = null,
			string description = null,
			KeyBindingDef hotKey = null)
		{
			return new Command_Action
			{
				defaultLabel = (gizmo_prefix + name + label_suffix).Translate(),
				defaultDesc = description ?? (gizmo_prefix + name + description_suffix).Translate(),
				activateSound = activateSound ?? SoundDef.Named("Click"),
				hotKey = hotKey,
				icon = icon,
				disabled = disabled,
				disabledReason = (disabled ? (disabledReason ?? (name + disabled_suffix).Translate()) : null),
				action = action
			};
		}
	}
}
