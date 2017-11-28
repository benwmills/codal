using System;
using System.Collections;
using MillsSoftware.SqlSchema;
using System.Data;
using System.Collections.Generic;

namespace MillsSoftware.CoDAL
{
	/// <summary>
	/// The EnumColumn class is used to represent an enumeration that needs to be generated.  The enumeration is
	/// built from a TinyInt column with a codal_enumeration extended property on the column.  The extended
	/// property is in the form Value1: Name1, Value2: Name2, etc.
	/// The value and name for each enumeration value are required.
	/// </summary>
	internal class EnumColumn
	{
		#region Fields

		/// <summary>
		/// The name of the enumeration.
		/// </summary>
		private string _Name;

		/// <summary>
		/// The list of enumeration values.
		/// </summary>
		private List<EnumColumnValue> _EnumColumnValues;

		#endregion

		#region Properties

		/// <summary>
		/// The name of the enumeration
		/// </summary>
		internal string Name { get { return this._Name; } }

		/// <summary>
		/// The list of enumeration values.
		/// </summary>
		internal List<EnumColumnValue> EnumColumnValues { get { return this._EnumColumnValues; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes an EnumColumn.  The <paramref name="column"/> that the enumeration should represent is passed
		/// in.  This column should contain an extended property called codal_enumeration that contains details
		/// on the enumeration values.  These details are parsed and used to complete the initialization of the
		/// EnumColumn.
		/// </summary>
		/// <param name="column">The column that this enumeration will represent.</param>
		internal EnumColumn(Column column)
		{
			// Assert that this column is a proper enum field.
			if (column.DataType != SqlDbType.TinyInt || column.Properties["codal_enumeration"] == null)
				throw new ApplicationException("Enumeration columns must be TinyInt types with a codal_enumeration extended property");

	        // Set the name of the enumeration based on the column name.
			this._Name = column.Name + "Enum";

			// Extract values from string.
			this._EnumColumnValues = new List<EnumColumnValue>();

			string[] EnumStrings = column.Properties["codal_enumeration"].Value.Split(new char[] {','});

			if (EnumStrings.Length == 0)
				throw new ApplicationException("At least one enumeration value must be defined for " + column.Table.Name + "." + column.Name);

			for (int n = 0; n < EnumStrings.Length; n++)
			{
				string[] StringParts = EnumStrings[n].Split(":".ToCharArray());

				if (StringParts.Length < 2)
					throw new ApplicationException("At least a value and a name must be specified for each enumeration value");

				// Required value and name.
				byte EnumValue = System.Convert.ToByte(StringParts[0].Trim());
				string EnumName = StringParts[1].Trim();

				// Create the value object and add it to the collection.
				EnumColumnValue objEnumColumnValue = new EnumColumnValue(this, EnumValue, EnumName);
				this._EnumColumnValues.Add(objEnumColumnValue);
			}
		}

		#endregion
	}
}
