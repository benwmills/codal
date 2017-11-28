using System;

namespace MillsSoftware.CoDAL
{
	/// <summary>
	/// EnumColumnValue represents a value in an EnumColumn.  Each value specifies a value, name and long name.
	/// The long name can be obtained from the regular name using a function that will be generated for each
	/// enumeration.
	/// </summary>
	internal class EnumColumnValue
	{
		#region Fields

		/// <summary>
		/// The EnumColumn that this EnumColumnValue belongs to.
		/// </summary>
		private EnumColumn _EnumColumn;

		/// <summary>
		/// The underlying integer value of this enumeration value.
		/// </summary>
		private byte _Value;

		/// <summary>
		/// The name of this enumeration value.
		/// </summary>
	    private string _Name;

		#endregion

		#region Properties

		/// <summary>
		/// The EnumColumn that this EnumColumnValue belongs to.
		/// </summary>
		internal EnumColumn EnumColumn { get { return this._EnumColumn; } }

		/// <summary>
		/// The underlying integer value of this enumeration value.
		/// </summary>
		internal byte Value { get { return this._Value; } }

		/// <summary>
		/// The name of this enumeration value.
		/// </summary>
		internal string Name { get { return this._Name; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes an EnumColumnValue.
		/// </summary>
		/// <param name="enumColumn">The EnumColumn that this EnumColumnValue belongs to.</param>
		/// <param name="value">The underlying integer value.</param>
		/// <param name="name">The name.</param>
		internal EnumColumnValue(EnumColumn enumColumn, byte value, string name)
		{
			this._EnumColumn = enumColumn;
			this._Value = value;
			this._Name = name;
		}

		#endregion
	}
}
