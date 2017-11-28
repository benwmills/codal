using System;
using System.Text;

namespace MillsSoftware.CoDAL
{
	/// <summary>
	/// The CodeBuilder class is a utility class to help generate the code.  It has useful methods to specify
	/// the indentation level of a line of code.  It uses an internal StringBuilder for high performance.
	/// </summary>
	internal class CodeBuilder
	{
		/// <summary>
		/// Internal StringBuilder used for high performance construction of the code.
		/// </summary>
		private StringBuilder _sb;

		/// <summary>
		/// Initializes a new instance of the CodeBuilder class.
		/// </summary>
		internal CodeBuilder()
		{
			this._sb = new StringBuilder("");
		}

		/// <overloads>
		/// The Write mehod adds a line of code to the internal StringBuilder without adding a carriage return.
		/// </overloads>
		/// <summary>
		/// Writes a line of code specified by <paramref name="text"/> without adding a carriage return.  This allows
		/// code to be added to the end of this line.
		/// </summary>
		/// <param name="text">The line of code to write</param>
		internal void Write(string text)
		{
			this._sb.Append(text);
		}

		/// <summary>
		/// Writes the specified number of <paramref name="tabs"/> followed by the specified <paramref name="text"/>.
		/// A carriage return is not added to the end of the line.  This allows extra code to be added to the end
		/// of the line.
		/// </summary>
		/// <param name="tabs">The number of tabs to prefix the line with</param>
		/// <param name="text">The line of code to write</param>
		internal void Write(int tabs, string text)
		{
			for (int n=1; n<=tabs; n++)
			{
				this._sb.Append("\t");
			}

			this._sb.Append(text);
		}		
		
		/// <overloads>
		/// The WriteLine adds a line of code to the internal StringBuilder followed by a carriage return.
		/// </overloads>
		/// <summary>
		/// Writes a line of code specified by <paramref name="text"/> followed by a carriage return.
		/// </summary>
		/// <param name="text">The line of code to write</param>
		internal void WriteLine(string text)
		{
			this._sb.Append(text);
			this._sb.Append("\r\n");
		}

		/// <summary>
		/// Writes the specified number of <paramref name="tabs"/> followed by the specified <paramref name="text"/>
		/// and finally a carriage return.
		/// </summary>
		/// <param name="tabs">The number of tabs to prefix the line with</param>
		/// <param name="text">The line of code to write</param>
		internal void WriteLine(int tabs, string text)
		{
			for (int n=1; n<=tabs; n++)
			{
				this._sb.Append("\t");
			}

			this._sb.Append(text);
			this._sb.Append("\r\n");
		}

		/// <summary>
		/// This method removes a specified number of characters from the end of the generated code.
		/// </summary>
		/// <param name="characterCount">The number of characters to remove from the end of the code.</param>
		/// <remarks>
		/// A good example of where this method is used is a situation where lines of code are generated in
		/// a loop.  Each line might need to end with a comma, except the last line.  It's easy to include the comma
		/// in the last line and then remove it using this method after the loop.
		/// </remarks>
		internal void RemoveLastCharacters(int characterCount)
		{
			this._sb.Remove(this._sb.Length - characterCount, characterCount);
		}

		/// <summary>
		/// CodeText returns the text contained in the internal StringBuilder.
		/// </summary>
		internal string CodeText
		{
			get{return this._sb.ToString();}
		}
	}
}
