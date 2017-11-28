using System;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class StoredProcedure
	{
		#region Member Variables

		private ParameterCollection _Parameters;
		private bool _Complete;

		#endregion

		#region Properties

		public Database Database { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ProcedureText { get; set; }

		public ParameterCollection Parameters
		{
			get
			{
				if (!this._Complete) this.GetInfo();
				return this._Parameters;
			}
		}

		#endregion

		#region Constructors

		public StoredProcedure()
		{
//			this._Database = null;
			this.Name = "";
			this.Description = "";
			this.ProcedureText = "";
			this._Parameters = new ParameterCollection();
//			this._Complete = false;
		}

		#endregion

		#region Methods

		private void GetInfo()
		{
			// Create connection.
			SqlConnection con = new SqlConnection(this.Database.ConnectionString);
			con.Open();

			// Run command to get stored procedure parameters.
			SqlCommand cmd = new SqlCommand("sp_sproc_columns", con);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@procedure_name", this.Name);
			SqlDataReader dr = cmd.ExecuteReader();

			int oColumnName = dr.GetOrdinal("COLUMN_NAME");
			int oTypeName = dr.GetOrdinal("TYPE_NAME");

			while (dr.Read())
			{
				Parameter parameter = new Parameter();
				parameter.StoredProcedure = this;
				parameter.Name = dr.GetString(oColumnName);
				parameter.DataType = Column.ConvertDataType(dr.GetString(oTypeName));
				this._Parameters.Add(parameter);
			}
			dr.Close();

			// Cleanup.
			con.Close();
			this._Complete = true;
		}

		#endregion
	}
}
