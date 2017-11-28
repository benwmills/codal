using System;

namespace MillsSoftware.SqlSchema
{
	public class TableProperty
	{
        #region Properties

        public Table Table { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }

        #endregion

		#region Constructors

		internal TableProperty(Table table, string name, string value)
		{
			this.Table = table;
			this.Name = name;
			this.Value = value;
		}

		#endregion
	}
}
