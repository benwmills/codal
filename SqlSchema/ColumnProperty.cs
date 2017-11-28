using System;

namespace MillsSoftware.SqlSchema
{
	public class ColumnProperty
	{
        #region Properties

        public Column Column { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }

        #endregion

		#region Constructors

		internal ColumnProperty(Column column, string name, string value)
		{
			this.Column = column;
			this.Name = name;
			this.Value = value;
		}

		#endregion
	}
}
