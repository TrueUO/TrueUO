#undef HALT_ON_ERRORS
using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using Server;

namespace Xanthos.Utilities
{
	public class ConfigParser
	{
		public static Element GetConfig(string filename, string tag)
		{
			Element element = GetConfig(filename);

			if (element != null)
            {
                element = GetConfig(element, tag);
            }

            return element;
		}

		public static Element GetConfig(string filename)
		{
			XmlTextReader reader = null;
			Element element = null;
			DOMParser parser;

			try 
			{
				Console.WriteLine( "Xanthos.Utilities.ConfigParser attempting to load {0}...", filename );
				reader = new XmlTextReader( filename );
				parser = new DOMParser();
				element = parser.Parse( reader );
				Console.WriteLine( "Xanthos.Utilities.ConfigParser success!" );

			}
			catch ( Exception exc )
			{
				// Fail gracefully only on errors reading the file
				if ( !( exc is System.IO.IOException ))
					throw exc;

				Console.WriteLine( "Xanthos.Utilities.ConfigParser failed." );
			}

			if (reader != null)
            {
                reader.Close();
            }

            return element;
		}

		public static Element GetConfig(Element element, string tag)
		{
			if (element.ChildElements.Count > 0) 
			{
				foreach(Element child in element.ChildElements) 
				{
					if (child.TagName == tag)
                    {
                        return child;
                    }
                }
			}
			return null;
		}
	}

	public class DOMParser
	{
		private Stack m_Elements;
		private Element m_CurrentElement;
		private Element m_RootElement;

		public DOMParser() 
		{
			m_Elements = new Stack();
			m_CurrentElement = null;
			m_RootElement = null;
		}

		public Element Parse(XmlTextReader reader) 
		{
			Element element = null;

			while (!reader.EOF)
			{
				reader.Read();            
				switch ( reader.NodeType )
				{
					case XmlNodeType.Element :					
						element = new Element( reader.LocalName );
						m_CurrentElement = element;
                        
						if ( m_Elements.Count == 0 ) 
						{
							m_RootElement = element;
							m_Elements.Push( element );
						}
						else 
						{                  
							Element parent = (Element)m_Elements.Peek();
							parent.ChildElements.Add( element );

							if ( reader.IsEmptyElement )
                            {
                                break;
                            }

                            m_Elements.Push( element );
                        }
						if ( reader.HasAttributes ) 
						{
							while( reader.MoveToNextAttribute() ) 
							{
								m_CurrentElement.setAttribute( reader.Name, reader.Value );
							}
						}
						break;
					case XmlNodeType.Attribute :
                        if (element != null)
                        {
                            element.setAttribute(reader.Name, reader.Value);
                        }
                        break;
					case XmlNodeType.EndElement :
						m_Elements.Pop();
						break;
					case XmlNodeType.Text :
						m_CurrentElement.Text = reader.Value;
						break;
					case XmlNodeType.CDATA :
						m_CurrentElement.Text = reader.Value;
						break;
					default :
						// ignore
						break;
				}
			}
			return m_RootElement;
		}
	}

	public class Elements : CollectionBase 
	{
		public Elements() 
		{
		}

		public void Add( Element element ) 
		{
			List.Add( element );
		}

		public Element this[int index] => (Element)List[index];

    }

	public class Element 
	{
		private String m_TagName;
		private String m_Text;
		private StringDictionary m_Attributes;
		private Elements m_ChildElements;

		public Element(string tagName) 
		{
			m_TagName = tagName;
			m_Attributes = new StringDictionary();
			m_ChildElements = new Elements();
			m_Text = "";
		}

		public string TagName { get => m_TagName; set => m_TagName = value; }

		public string Text { get => m_Text; set => m_Text = value; }

		public Elements ChildElements => m_ChildElements;

        public StringDictionary Attributes => m_Attributes;

        public string Attribute(string name) 
		{
			return m_Attributes[name];
		}

		public void setAttribute(string name, string value) 
		{
			m_Attributes.Add( name, value );
		}

		#region Xml to data type conversions

		public bool GetBoolValue( out bool val )
		{
			val = false;

			try
			{
				if ( null != m_Text && "" != m_Text )
				{
					val = bool.Parse( m_Text );
					return true;
				}
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return false;
		}

		public bool GetDoubleValue( out double val )
		{
			val = 0;

			try
			{
				if ( null != m_Text && "" != m_Text )
				{
					val = double.Parse( m_Text );
					return true;
				}
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return false;
		}

		public bool GetIntValue( out int val )
		{
			val = 0;

			try
			{
				if ( null != m_Text && "" != m_Text )
				{
					val = Int32.Parse( m_Text );
					return true;
				}
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return false;
		}

		public bool GetAccessLevelValue( out AccessLevel val )
		{
			val = AccessLevel.Player;
			try
			{
				val = (AccessLevel)AccessLevel.Parse( typeof(Server.AccessLevel), m_Text, true );
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		public bool GetMapValue( out Map val )
		{
			val = null;
			try
			{
				val = Map.Parse( m_Text );
			
				if ( null == val )
					throw new ArgumentException( "Map expected" );
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		public bool GetTypeValue( out Type val )
		{
			val = null;
			try
			{
				val = Type.GetType( m_Text );
			
				if ( null == val )
					throw new ArgumentException( "Type expected" );
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		public bool GetPoint3DValue( out Point3D val )
		{
			val = new Point3D();
			int elementsExpected = 3;

			try
			{
				if ( null == ChildElements )
					return false;

				if ( elementsExpected != ChildElements.Count )
					throw new System.IndexOutOfRangeException( elementsExpected + " elements were expected" );

				int temp;
			
				if ( ChildElements[ 0 ].GetIntValue( out temp ))
					val.X = temp;
				else
					throw new System.ArrayTypeMismatchException( "Int expected" );

				if ( ChildElements[ 1 ].GetIntValue( out temp ))
					val.Y = temp;
				else
					throw new System.ArrayTypeMismatchException( "Int expected" );

				if ( ChildElements[ 2 ].GetIntValue( out temp ))
					val.Z = temp;
				else
					throw new System.ArrayTypeMismatchException( "Int expected" );
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		
		public bool GetArray( out bool[] val )
		{
			return GetArray( 0, out val );
		}

		public bool GetArray( int elementsExpected, out bool[] val )
		{
			val = null;

			if ( null == ChildElements )
				return false;

			try
			{
				if ( elementsExpected > 0 && elementsExpected != ChildElements.Count )
					throw new System.IndexOutOfRangeException( elementsExpected + " elements were expected" );

				bool[] array = new bool[ ChildElements.Count ];
				bool temp;
			
				for ( int i = 0; i < ChildElements.Count; i++ )
				{
					if ( ChildElements[ i ].GetBoolValue( out temp ))
						array[ i ] = temp;
					else
						throw new System.ArrayTypeMismatchException( "Bool expected" );
				}
				val = array;
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		public bool GetArray( out int[] val )
		{
			return GetArray( 0, out val );
		}

		public bool GetArray( int elementsExpected, out int[] val )
		{
			val = null;
			
			if ( null == ChildElements )
				return false;

			try
			{
				if ( elementsExpected > 0 && elementsExpected != ChildElements.Count )
					throw new System.IndexOutOfRangeException( elementsExpected + " elements were expected" );

				int[] array = new int[ ChildElements.Count ];
				int temp;
			
				for ( int i = 0; i < ChildElements.Count; i++ )
				{
					if ( ChildElements[ i ].GetIntValue( out temp ))
						array[ i ] = temp;
					else
						throw new System.ArrayTypeMismatchException( "Int expected" );
				}
				val = array;
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		public bool GetArray( out Type[] val )
		{
			return GetArray( 0, out val );
		}

		public bool GetArray( int elementsExpected, out Type[] val )
		{
			val = null;
			
			if ( null == ChildElements )
				return false;

			try
			{
				if ( elementsExpected > 0 && elementsExpected != ChildElements.Count )
					throw new System.IndexOutOfRangeException( elementsExpected + " elements were expected" );

				Type[] array = new Type[ ChildElements.Count ];
			
				for ( int i = 0; i < ChildElements.Count; i++ )
				{
					array[ i ] = Type.GetType( ChildElements[ i ].Text );
				}
				val = array;
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		public bool GetArray( out string[] val )
		{
			return GetArray( 0, out val );
		}

		public bool GetArray( int elementsExpected, out string[] val )
		{
				val = null;
			
			if ( null == ChildElements )
				return false;

			try
			{
				if ( elementsExpected > 0 && elementsExpected != ChildElements.Count )
					throw new System.IndexOutOfRangeException( elementsExpected + " elements were expected" );

				string[] array = new string[ ChildElements.Count ];
			
				for ( int i = 0; i < ChildElements.Count; i++ )
				{
					if ( null != ChildElements[ i ].Text )
						array[ i ] = ChildElements[ i ].Text;
					else
						throw new System.ArrayTypeMismatchException( "String expected" );
				}
				val = array;
			}
			catch ( Exception exc ) { HandleError( exc ); }

			return true;
		}

		#endregion

		private void HandleError( Exception exc )
		{
			Console.WriteLine( "\nXanthos.Utilities.ConfigParser error:\n{0}\nElement: <{1}>{2}</{1}>\n", exc.Message, TagName, Text );
#if HALT_ON_ERRORS
			throw exc;
#endif
		}
	}
}
