using System;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class Parameter
	{
        #region Properties

        public StoredProcedure StoredProcedure { get; set; }
        public string Name { get; set; }
        public SqlDbType DataType { get; set; }

        #endregion

		#region Constructors

		internal Parameter() { }

		#endregion
	}
}
