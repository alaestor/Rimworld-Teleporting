using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace alaestor_teleporting
{
	class MsgHelper
	{
		public static readonly string message_prefix = TeleportingMod.modname + "_Message_";
		private static string strmod(string msg, bool alreadyTranslated = false)
		{
			if (alreadyTranslated)
			{
				return msg;
			}
			else return (message_prefix + msg).Translate();
		}

		public static void Reject(string msg, bool alreadyTranslated = false)
		{
			Messages.Message(new Message(strmod(msg, alreadyTranslated), RimWorld.MessageTypeDefOf.RejectInput));
		}

		public static void Accept(string msg, bool alreadyTranslated = false)
		{
			Messages.Message(new Message(strmod(msg, alreadyTranslated), RimWorld.MessageTypeDefOf.NeutralEvent));
		}

		public static void Neutral(string msg, bool alreadyTranslated = false)
		{
			Messages.Message(new Message(strmod(msg, alreadyTranslated), RimWorld.MessageTypeDefOf.NeutralEvent));
		}

		public static void Silent(string msg, bool alreadyTranslated = false)
		{
			Messages.Message(new Message(strmod(msg, alreadyTranslated), RimWorld.MessageTypeDefOf.SilentInput));
		}
	}
}
