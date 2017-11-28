using System;

namespace MillsSoftware.SqlSchema
{
	public class ColumnPair
	{
        #region Properties

        public Column FKColumn { get; private set; }
        public Column PKColumn { get; private set; }

        #endregion

		#region Constructors

		public ColumnPair(Column foreignKeyColumn, Column primaryKeyColumn)
		{
			this.FKColumn = foreignKeyColumn;
			this.PKColumn = primaryKeyColumn;
		}

		#endregion
	}
}