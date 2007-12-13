using System;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;
//****************************************************
//
//		Source Control Author : Justin Wendlandt
//			jwendl@hotmail.com
//
//		Control Protected by the Creative Commons License
//		http://creativecommons.org/licenses/by-nc-sa/2.0/
//
//****************************************************
namespace YAF.Classes.UI
{
	public class HighLighter
	{
		/* Ederon : 6/16/2007 - conventions */

		// To Replace Enter with <br />
		private bool _replaceEnter;
		public bool ReplaceEnter
		{
			get { return _replaceEnter; }
			set { _replaceEnter = value; }
		}

		// Default Constructor
		public HighLighter()
		{
			_replaceEnter = false;
		}

		public string ColorText( string tmpCode, string pathToDefFile, string language )
		{
			language = language.ToLower();
			if ( language == "c#" || language == "csharp" )
				language = "cs";

			language = language.Replace( "\"", "" );
			// language = language.Replace("&#8220;", "");

			string tmpOutput = "";
			string comments = "";
			bool valid = true;

			ArrayList alKeyWords = new ArrayList();
			ArrayList alKeyTypes = new ArrayList();

			// cut it off at the pass...
			if ( !File.Exists( pathToDefFile + language.ToString() + ".def" ) )
			{
				return tmpCode;
			}

			// Read def file.
			try
			{
				StreamReader sr = new StreamReader( pathToDefFile + language.ToString() + ".def" );
				string tmpLine = "";
				string curFlag = "";
				while ( sr.Peek() != -1 )
				{
					tmpLine = sr.ReadLine();
					if ( tmpLine != "" )
					{
						if ( tmpLine.Substring( 0, 1 ) == "-" )
						{
							// Ignore these lines and set the Current Flag
							if ( tmpLine.ToLower().IndexOf( "keywords" ) > 0 )
							{
								curFlag = "keywords";
							}

							if ( tmpLine.ToLower().IndexOf( "keytypes" ) > 0 )
							{
								curFlag = "keytypes";
							}

							if ( tmpLine.ToLower().IndexOf( "comments" ) > 0 )
							{
								curFlag = "comments";
							}
						}
						else
						{
							if ( curFlag == "keywords" )
							{
								alKeyWords.Add( tmpLine );
							}

							if ( curFlag == "keytypes" )
							{
								alKeyTypes.Add( tmpLine );
							}

							if ( curFlag == "comments" )
							{
								comments = tmpLine;
							}
						}
					}
				}
				sr.Close();
			}
			catch ( Exception ex )
			{
				string foobar = ex.ToString();
				tmpOutput = "<span class=\"errors\">There was an error opening file " + pathToDefFile + language.ToString() + ".def...</span>";
				valid = false;
				throw new ApplicationException( string.Format( "There was an error opening file {0}{1}.def", pathToDefFile, language ), ex );
			}

			if ( valid == true )
			{
				// Replace Comments
				int lineNum = 0;
				ArrayList thisComment = new ArrayList();
				MatchCollection mColl = Regex.Matches( tmpCode, comments, RegexOptions.Multiline | RegexOptions.IgnoreCase );
				foreach ( Match m in mColl )
				{
					thisComment.Add( m.ToString() );
					tmpCode = tmpCode.Replace( m.ToString(), "[ReplaceComment" + lineNum++ + "]" );
				}

				// Replace Strings
				lineNum = 0;
				ArrayList thisString = new ArrayList();
				string thisMatch = "\"((\\\\\")|[^\"(\\\\\")]|)+\"";
				mColl = Regex.Matches( tmpCode, thisMatch, RegexOptions.Singleline | RegexOptions.IgnoreCase );
				foreach ( Match m in mColl )
				{
					thisString.Add( m.ToString() );
					tmpCode = tmpCode.Replace( m.ToString(), "[ReplaceString" + lineNum++ + "]" );
				}

				// Replace Chars
				lineNum = 0;
				ArrayList thisChar = new ArrayList();
				mColl = Regex.Matches( tmpCode, "\'.*?\'", RegexOptions.Singleline | RegexOptions.IgnoreCase );
				foreach ( Match m in mColl )
				{
					thisChar.Add( m.ToString() );
					tmpCode = tmpCode.Replace( m.ToString(), "[ReplaceChar" + lineNum++ + "]" );
				}

				// Replace KeyWords
				string [] keyWords = new String [alKeyWords.Count];
				alKeyWords.CopyTo( keyWords );
				string tmpKeyWords = "(?<replacethis>" + String.Join( "|", keyWords ) + ")";
				tmpCode = Regex.Replace( tmpCode, "\\b" + tmpKeyWords + "\\b(?<!//.*)", "<span class=\"keyword\">${replacethis}</span>" );

				// Replace KeyTypes
				string [] keyTypes = new String [alKeyTypes.Count];
				alKeyTypes.CopyTo( keyTypes );
				string tmpKeyTypes = "(?<replacethis>" + String.Join( "|", keyTypes ) + ")";
				tmpCode = Regex.Replace( tmpCode, "\\b" + tmpKeyTypes + "\\b(?<!//.*)", "<span class=\"keytype\">${replacethis}</span>" );

				lineNum = 0;
				foreach ( string m in thisChar )
				{
					tmpCode = tmpCode.Replace( "[ReplaceChar" + lineNum++ + "]", "<span class=\"string\">" + m.ToString() + "</span>" );
				}

				lineNum = 0;
				foreach ( string m in thisString )
				{
					tmpCode = tmpCode.Replace( "[ReplaceString" + lineNum++ + "]", "<span class=\"string\">" + m.ToString() + "</span>" );
				}

				lineNum = 0;
				foreach ( string m in thisComment )
				{
					tmpCode = tmpCode.Replace( "[ReplaceComment" + lineNum++ + "]", "<span class=\"comment\">" + m.ToString() + "</span>" );
				}

				// Replace Numerics
				tmpCode = Regex.Replace( tmpCode, "(\\d{1,12}\\.\\d{1,12}|\\d{1,12})", "<span class=\"integer\">$1</span>" );

				if ( _replaceEnter == true )
				{
					tmpCode = Regex.Replace( tmpCode, "\r", "" );
					tmpCode = Regex.Replace( tmpCode, "\n", "<br />" + Environment.NewLine );
				}

				tmpCode = Regex.Replace( tmpCode, "  ", "&nbsp;&nbsp;" );
				tmpCode = Regex.Replace( tmpCode, "\t", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" );

				// Create Output
				tmpOutput = "<div class=\"yafcodehighlighting\">" + Environment.NewLine;
				tmpOutput += tmpCode;
				tmpOutput += "</div>" + Environment.NewLine;
			}
			return tmpOutput;
		}
	}
}

