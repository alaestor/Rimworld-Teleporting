using System;
using UnityEngine;
using Verse;

namespace alaestor_teleporting
{
	class GizmoHelper
	{
		public static Command_Action MakeCommandAction(
			string name,
			Action action,
			SoundDef activateSound = null,
			Texture2D icon = null,
			bool disabled = false,
			KeyBindingDef hotKey = null)
		{
			return new Command_Action
			{
				defaultLabel = (name + "_Label").Translate(),
				defaultDesc = (name + "_Desc").Translate(),
				activateSound = activateSound ?? SoundDef.Named("Click"),
				hotKey = hotKey,
				icon = icon,
				disabled = disabled,
				disabledReason = (disabled ? (name + "_DisabledReason").Translate() : null),
				action = action
			};
		}
	}
}
