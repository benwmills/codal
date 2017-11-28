using System;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class Table
	{
        #region Properties

        public Database Database { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TablePropertyCollection Properties { get; private set; }
        public ColumnCollection Columns { get; private set; }
        public PrimaryKey PrimaryKey { get; set; }
        public ForeignKeyCollection OutgoingForeignKeys { get; private set; }
        public ForeignKeyCollection IncomingForeignKeys { get; private set; }

        #endregion

		#region Constructors

		public Table()
		{
//			this._Database = null;
			this.Name = "";
			this.Description = "";
			this.Properties = new TablePropertyCollection();
			this.Columns = new ColumnCollection();
//			this._PrimaryKey = null;
			this.OutgoingForeignKeys = new ForeignKeyCollection();
			this.IncomingForeignKeys = new ForeignKeyCollection();
		}

		#endregion
	}
}
