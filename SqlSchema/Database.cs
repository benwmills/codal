using System;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class Database
	{
		#region Member Variables

		private TableCollection _Tables;
		private StoreProcedureCollection _StoredProcedures;
		private bool _Complete;

		#endregion

		#region Properties

		public Server Server { get; private set; }
		public string Name { get; private set; }

		public TableCollection Tables
		{
			get
			{
				if (!this._Complete) this.GetDatabaseInfo();
				return this._Tables;
			}
		}

		public StoreProcedureCollection StoredProcedures
		{
			get
			{
				if (!this._Complete) this.GetDatabaseInfo();
				return this._StoredProcedures;
			}
		}

		public string ConnectionString
		{
			get { return this.Server.ConnectionString + "; Database=" + this.Name; }
		}

		#endregion

		#region Constructors

		public Database(Server server, string serverName)
		{
			this.Server = server;
			this.Name = serverName;
//			this._Complete = false;
		}

		#endregion

		#region Methods

		private void GetDatabaseInfo()
		{
			// Create connection.
			SqlConnection con = new SqlConnection(this.ConnectionString);
			con.Open();

			// Create table objects using INFORMATION_SCHEMA.TABLES.
			CreateTableObjects(con);

			// Add table descriptions and extended properties using fn_listextendedproperty.
			AddTableDescriptionsAndExtendedProperties(con);

			// Add columns for each table using sp_columns.
			AddColumns(con);

			// Add column descriptions and extended properties using fn_listextendedproperty.
			AddColumnDescriptionsAndExtendedProperties(con);

			// Add primary key for each table using sp_pkeys.
			AddPrimaryKeys(con);

			// Add foreign keys using sp_fkeys.
			AddForeignKeys(con);

			// Get stored procedures using INFORMATION_SCHEMA.ROUTINES.
			AddStoredProcedures(con);

			// Get the stored procedure descriptions using fn_listextendedproperty.
			AddStoredProcedureDescriptions(con);

			// Close connection and set _Complete to True.
			con.Close();
			this._Complete = true;
		}

		private void CreateTableObjects(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "SELECT TABLE_NAME " + "FROM INFORMATION_SCHEMA.TABLES " + "WHERE TABLE_SCHEMA = 'dbo' " + "AND TABLE_TYPE = 'BASE TABLE' " + "AND TABLE_NAME <> 'dtproperties' " + "ORDER BY TABLE_NAME;";
			SqlDataReader dr = cmd.ExecuteReader();

			this._Tables = new TableCollection();
			while (dr.Read())
			{
				Table table = new Table();
				table.Database = this;
				table.Name = dr.GetString(dr.GetOrdinal("TABLE_NAME"));
				this._Tables.Add(table);
			}

			dr.Close();
		}

		private void AddTableDescriptionsAndExtendedProperties(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = "SELECT objname, name, value " + "FROM ::fn_listextendedproperty (default, 'user', 'dbo', 'table', default, default, default) " + "ORDER BY objname, name;";
			SqlDataReader dr = cmd.ExecuteReader();

			int oObjName = dr.GetOrdinal("objname");
			int oName = dr.GetOrdinal("name");
			int oValue = dr.GetOrdinal("value");

			while (dr.Read())
			{
				Table table = this._Tables[dr.GetString(oObjName)];
				if (dr.GetString(oName) == "MS_Description")
				{
					table.Description = dr.GetString(oValue);
				}
				else
				{
					table.Properties.Add(new TableProperty(table, dr.GetString(oName), dr.GetString(oValue)));
				}
			}
			dr.Close();
		}

		private void AddColumns(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "sp_columns_90";
			cmd.Parameters.Add("@table_name", SqlDbType.NVarChar);
			cmd.Parameters.Add("@table_owner", SqlDbType.NVarChar);

			foreach (Table table in this._Tables)
			{
				cmd.Parameters["@table_name"].Value = table.Name;
				cmd.Parameters["@table_owner"].Value = "dbo";
				SqlDataReader dr = cmd.ExecuteReader();

				int oColumnName = dr.GetOrdinal("COLUMN_NAME");
				int oNullable = dr.GetOrdinal("NULLABLE");
				int oTypeName = dr.GetOrdinal("TYPE_NAME");
				int oLength = dr.GetOrdinal("LENGTH");
				int oPrecision = dr.GetOrdinal("PRECISION");
				int oScale = dr.GetOrdinal("SCALE");

				while (dr.Read())
				{
					Column column = new Column();
					column.Table = table;
					column.Name = dr.GetString(oColumnName);
					column.AllowNulls = Convert.ToBoolean(dr.GetInt16(oNullable));

					// Extract data type and identity info from the TYPE_NAME field.
					string DataTypeString = dr.GetString(oTypeName);
					if (DataTypeString.EndsWith(" identity"))
					{
						column.Identity = true;
						DataTypeString = DataTypeString.Replace(" identity", "");
					}
					else
					{
						column.Identity = false;
					}
					column.DataType = Column.ConvertDataType(DataTypeString);

					// Get values for the length, precision and scale.
					switch (column.DataType)
					{
						case SqlDbType.Char:
						case SqlDbType.VarChar:
							column.Length = Convert.ToInt16(dr.GetInt32(oLength));
							break;

						case SqlDbType.NChar:
						case SqlDbType.NVarChar:
							column.Length = Convert.ToInt16(dr.GetInt32(oLength) / 2);
							break;

						case SqlDbType.Decimal:
							column.Precision = Convert.ToInt16(dr.GetInt32(oPrecision));
							column.Scale = dr.GetInt16(oScale);
							break;
					}

					// Add the column to the collection.
					table.Columns.Add(column);
				}
				dr.Close();
			}
		}

		private void AddColumnDescriptionsAndExtendedProperties(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.Text;

			foreach (Table table in this._Tables)
			{
				cmd.CommandText = "SELECT objname, name, value " + "FROM ::fn_listextendedproperty (default, 'user', 'dbo', 'table', '" + table.Name + "', 'column', default) " + "ORDER BY objname, name;";
				SqlDataReader dr = cmd.ExecuteReader();

				int oObjName = dr.GetOrdinal("objname");
				int oName = dr.GetOrdinal("name");
				int oValue = dr.GetOrdinal("value");

				while (dr.Read())
				{
					Column column = table.Columns[dr.GetString(oObjName)];
					if (dr.GetString(oName) == "MS_Description")
					{
						column.Description = dr.GetString(oValue);
					}
					else
					{
						column.Properties.Add(new ColumnProperty(column, dr.GetString(oName), dr.GetString(oValue)));
					}
				}

				dr.Close();
			}
		}

		private void AddPrimaryKeys(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "sp_pkeys";
			cmd.Parameters.Add("@table_name", SqlDbType.VarChar);
			cmd.Parameters.Add("@table_owner", SqlDbType.VarChar);

			foreach (Table table in this._Tables)
			{
				cmd.Parameters["@table_name"].Value = table.Name;
				cmd.Parameters["@table_owner"].Value = "dbo";
				SqlDataReader dr = cmd.ExecuteReader();

				int oPkName = dr.GetOrdinal("PK_NAME");
				int oColumnName = dr.GetOrdinal("COLUMN_NAME");

				if (dr.HasRows)
				{
					PrimaryKey primaryKey = new PrimaryKey();
					primaryKey.Table = table;
					primaryKey.Columns = new ColumnCollection();
					while (dr.Read())
					{
						primaryKey.Name = dr.GetString(oPkName);
						Column column = table.Columns[dr.GetString(oColumnName)];
						column.PrimaryKey = true;
						primaryKey.Columns.Add(column);
					}
					table.PrimaryKey = primaryKey;
				}
				dr.Close();
			}
		}

		private void AddForeignKeys(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "sp_fkeys";
			cmd.Parameters.Add("@fktable_name", SqlDbType.VarChar);
			cmd.Parameters.Add("@fktable_owner", SqlDbType.VarChar);

			foreach (Table objTable in this._Tables)
			{
				cmd.Parameters["@fktable_name"].Value = objTable.Name;
				cmd.Parameters["@fktable_owner"].Value = "dbo";
				SqlDataReader dr = cmd.ExecuteReader();

				int oFkName = dr.GetOrdinal("FK_NAME");
				int oPkTableName = dr.GetOrdinal("PKTABLE_NAME");
				int oFkColumnName = dr.GetOrdinal("FKCOLUMN_NAME");
				int oPkColumnName = dr.GetOrdinal("PKCOLUMN_NAME");
//				int oKeySeq = dr.GetOrdinal("KEY_SEQ");

				if (dr.HasRows)
				{
					string ForeignKeyName = "";
					ForeignKey objForeignKey;
					Table objPKTable = null;
					ColumnPairCollection colColumnPairs = null;

					while (dr.Read())
					{
						if (dr.GetString(oFkName) != ForeignKeyName)
						{
							if (!string.IsNullOrEmpty(ForeignKeyName))
							{
								objForeignKey = new ForeignKey();
								objForeignKey.Name = ForeignKeyName;
								objForeignKey.FKTable = objTable;
								objForeignKey.PKTable = objPKTable;
								objForeignKey.ColumnPairs = colColumnPairs;
								objTable.OutgoingForeignKeys.Add(objForeignKey);
								objPKTable.IncomingForeignKeys.Add(objForeignKey);
								// Add foreign key to column outgoing foreign keys collection.
								foreach (ColumnPair objColumnPair in colColumnPairs)
								{
									objColumnPair.FKColumn.OutgoingForeignKeys.Add(objForeignKey);
								}
							}
							ForeignKeyName = dr.GetString(oFkName);
							// TODO:1  Fix situation when PKTable cannot be found due to security.
							objPKTable = this._Tables[dr.GetString(oPkTableName)];
							colColumnPairs = new ColumnPairCollection();
						}
						ColumnPair objColumnPair2 = new ColumnPair(objTable.Columns[dr.GetString(oFkColumnName)], objPKTable.Columns[dr.GetString(oPkColumnName)]);
						colColumnPairs.Add(objColumnPair2);
					}
					// Add final foreign key.
					objForeignKey = new ForeignKey();
					objForeignKey.Name = ForeignKeyName;
					objForeignKey.FKTable = objTable;
					objForeignKey.PKTable = objPKTable;
					objForeignKey.ColumnPairs = colColumnPairs;
					objTable.OutgoingForeignKeys.Add(objForeignKey);
					objPKTable.IncomingForeignKeys.Add(objForeignKey);
					// Add foreign key to column outgoing foreign keys collection.
					foreach (ColumnPair objColumnPair in colColumnPairs)
					{
						objColumnPair.FKColumn.OutgoingForeignKeys.Add(objForeignKey);
					}
				}
				dr.Close();
			}
		}

		private void AddStoredProcedures(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT ROUTINE_NAME, ROUTINE_DEFINITION " + "FROM INFORMATION_SCHEMA.ROUTINES " + "WHERE ROUTINE_SCHEMA = 'dbo' " + "AND ROUTINE_TYPE = 'PROCEDURE' " + "AND ROUTINE_NAME NOT LIKE 'dt_%' " + "ORDER BY ROUTINE_NAME;";
			SqlDataReader dr = cmd.ExecuteReader();

			int oRoutineName = dr.GetOrdinal("ROUTINE_NAME");
			int oRoutineDefinition = dr.GetOrdinal("ROUTINE_DEFINITION");

			this._StoredProcedures = new StoreProcedureCollection();
			while (dr.Read())
			{
				StoredProcedure objStoredProcedure = new StoredProcedure();
				objStoredProcedure.Database = this;
				objStoredProcedure.Name = dr.GetString(oRoutineName);
				objStoredProcedure.ProcedureText = dr.GetString(oRoutineDefinition);
				this._StoredProcedures.Add(objStoredProcedure);
			}
			dr.Close();
		}

		private void AddStoredProcedureDescriptions(SqlConnection con)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT objname, name, value " + "FROM ::fn_listextendedproperty (default, 'user', 'dbo', 'procedure', default, default, default) " + "ORDER BY objname, name;";
			SqlDataReader dr = cmd.ExecuteReader();

			int oObjName = dr.GetOrdinal("objname");
			int oName = dr.GetOrdinal("name");
			int oValue = dr.GetOrdinal("value");

			while (dr.Read())
			{
				StoredProcedure objStoredProcedure = this._StoredProcedures[dr.GetString(oObjName)];
				if (dr.GetString(oName) == "MS_Description")
				{
					objStoredProcedure.Description = dr.GetString(oValue);
				}
			}
			dr.Close();
		}

		#endregion
	}
}
