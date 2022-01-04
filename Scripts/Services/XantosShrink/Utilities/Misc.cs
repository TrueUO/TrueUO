using System.Reflection;
using Server;
using Server.Commands;

namespace Xanthos.Utilities
{
	public class Misc
	{
        public static int kRedHue = 0x20;

		//
		// Puts spaces before type name inner-caps
		//
		public static string GetFriendlyClassName( string typeName )
		{
			for ( int index = 1; index < typeName.Length; index++ )
			{
				if (char.IsUpper(typeName, index))
				{
					typeName.Insert( index++, " " );
				}
			}

			return typeName;
		}

		public static void SendCommandDetails( Mobile player, string command )
		{
			SendCommandDescription( player, command );
			SendCommandUsage( player, command );
		}

		public static void SendCommandUsage(Mobile player, string command)
		{
			string message;
			CommandEntry entry = CommandSystem.Entries[command];

			if (entry != null)
			{
				MethodInfo mi = entry.Handler.Method;

				object[] attrs = mi.GetCustomAttributes(typeof(UsageAttribute), false);

				UsageAttribute usage = attrs.Length > 0 ? attrs[0] as UsageAttribute : null;

				message = "Format: " + (null == usage ? " - no usage" : usage.Usage);
			}
			else
            {
                message = command + " - unknown command";
            }

            player.SendMessage(kRedHue, message);
		}

		public static void SendCommandDescription( Mobile player, string command )
		{
			string message;
			CommandEntry entry = CommandSystem.Entries[ command ];

			if ( null != entry )
			{
				MethodInfo mi = entry.Handler.Method;

				object[] attrs = mi.GetCustomAttributes(typeof(DescriptionAttribute), false);

				DescriptionAttribute desc = attrs.Length > 0 ? attrs[0] as DescriptionAttribute : null;

				message = command + ": " + (null == desc ? " - no description" : desc.Description);
			}
			else
            {
                message = command + " - unknown command";
            }

            player.SendMessage(kRedHue, message);
		}
	}
}
