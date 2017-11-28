using System;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class PrimaryKey
	{
        #region Properties

        public Table Table { get; set; }
        public string Name { get; set; }
        public ColumnCollection Columns { get; set; }

        #endregion

		#region Constructors

		public PrimaryKey()
		{
//			this._Table = null;
			this.Name = "";
//			this._Columns = null;
		}

		#endregion

	}
}
