using System;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class ForeignKey
	{
        #region Properties

        public string Name { get; set; }
        public Table FKTable { get; set; }
        public Table PKTable { get; set; }
        public ColumnPairCollection ColumnPairs { get; set; }

        #endregion

		#region Constructors

		public ForeignKey()
		{
			this.Name = "";
//			this._FKTable = null;
//			this._PKTable = null;
			this.ColumnPairs = new ColumnPairCollection();
		}

		#endregion

	}
}
