namespace Xanthos.Utilities
{
	public class Misc
	{
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
    }
}
