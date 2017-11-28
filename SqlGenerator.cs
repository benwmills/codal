using System;
using MillsSoftware.SqlSchema;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.CoDAL
{
	/// <summary>
	/// The SqlGenerator class is used to generate the insert, update and delete stored procedures to support
	/// the generated business classes.
	/// </summary>
	internal class SqlGenerator
	{
		#region Fields

		/// <summary>
		/// The database that the stored procedures will be created in.
		/// </summary>
		private Database _Database;

		#endregion

		#region Constructors

		/// <summary>
		/// This contructor takes the <paramref name="database"/> object that the stored procedures will be
		/// generated for.
		/// </summary>
		/// <param name="database">The database that the stored procedures will be generated in</param>
		internal SqlGenerator(Database database)
		{
			this._Database = database;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Generates a stored procedure to insert a record into the specified <paramref name="table">table</paramref>.
		/// </summary>
		/// <param name="table">The table to generate the insert stored procedure for.</param>
		internal void GenerateInsert(Table table)
		{
			CodeBuilder cb = new CodeBuilder();
			SqlGenerator.AddComment(cb);
			cb.WriteLine("");

			cb.WriteLine("CREATE PROCEDURE dbo.CoDAL_" + table.Name + "_Insert (");

			bool FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (FirstField)
					FirstField = false;
				else
					cb.WriteLine(", ");

				cb.Write(1, "@" + objColumn.Name + " " + objColumn.LongDataType);

				if (objColumn.Identity)
					cb.Write(" OUTPUT");
			}

			cb.WriteLine("");
			cb.WriteLine(") AS");
			cb.WriteLine("");
			cb.Write("INSERT INTO " + table.Name + " (");

			FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.Identity == false)
				{
					if (FirstField)
					{
						cb.Write(objColumn.Name);
						FirstField = false;
					}
					else
						cb.Write(", " + objColumn.Name);
				}
			}

			cb.WriteLine(")");
			cb.Write(1, "VALUES (");

			FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.Identity == false)
				{
					if (FirstField)
					{
						cb.Write("@" + objColumn.Name);
						FirstField = false;
					}
					else
						cb.Write(", " + "@" + objColumn.Name);
				}
			}

			cb.WriteLine(")");
			cb.WriteLine("");

			// Add error checking.
			cb.WriteLine("IF @@ROWCOUNT <> 1 RAISERROR('Record not inserted', 16, 1)");
			cb.WriteLine("");

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.Identity && objColumn.PrimaryKey)
					cb.WriteLine("SET @" + objColumn.Name + " = @@IDENTITY");
			}

			// Create the procedure.
			this.AddProcedure("CoDAL_" + table.Name + "_Insert", cb.CodeText, true);

			return;
		}

		/// <summary>
		/// Generates a stored procedure to update a record in the specified <paramref name="table">table</paramref>.
		/// </summary>
		/// <param name="table">The table to generate the update stored procedure for.</param>
		internal void GenerateUpdate(Table table)
		{
			CodeBuilder cb = new CodeBuilder();

			SqlGenerator.AddComment(cb);
			cb.WriteLine("");

			cb.WriteLine("CREATE PROCEDURE dbo.CoDAL_" + table.Name + "_Update (");

			bool FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (FirstField)
					FirstField = false;
				else
					cb.WriteLine(", ");

				cb.Write(1, "@" + objColumn.Name + " " + objColumn.LongDataType);
			}

			// Add OriginalUpdated.
			cb.WriteLine(", ");
			cb.WriteLine(1, "@OriginalUpdated DateTime");

			cb.WriteLine(") AS");
			cb.WriteLine("");

			cb.WriteLine("UPDATE " + table.Name + " SET");

			FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.Identity == false && objColumn.PrimaryKey == false)
				{
					if (FirstField)
						FirstField = false;
					else
						cb.WriteLine(",");
					cb.Write(1, objColumn.Name + " = @" + objColumn.Name);
				}
			}

			cb.WriteLine("");
			cb.Write("WHERE ");

			FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.PrimaryKey)
				{
					if (FirstField)
					{
						cb.WriteLine(objColumn.Name + " = @" + objColumn.Name);
						FirstField = false;
					}
					else
						cb.WriteLine(1, "AND " + objColumn.Name + " = @" + objColumn.Name);
				}
			}

			// Add OriginalUpdated.
			cb.WriteLine(1, "AND Updated = @OriginalUpdated");
			cb.WriteLine("");

			// Add error checking.
			cb.WriteLine("IF @@ROWCOUNT <> 1 RAISERROR('Record not updated', 16, 1)");

			// Create the procedure.
			this.AddProcedure("CoDAL_" + table.Name + "_Update", cb.CodeText, true);

			return;
		}

		/// <summary>
		/// Generates a stored procedure to delete a record from the specified <paramref name="table">table</paramref>.
		/// </summary>
		/// <param name="table">The table to generate the delete stored procedure for.</param>
		internal void GenerateDelete(Table table)
		{
			CodeBuilder cb;
			bool FirstField;

			cb = new CodeBuilder();

			SqlGenerator.AddComment(cb);
			cb.WriteLine("");

			cb.WriteLine("CREATE PROCEDURE dbo.CoDAL_" + table.Name + "_Delete (");

			FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.PrimaryKey)
				{
					if (FirstField)
						FirstField = false;
					else
						cb.WriteLine(",");

					cb.Write(1, "@" + objColumn.Name + " " + objColumn.LongDataType);
				}
			}

			// Add OriginalUpdated.
			cb.WriteLine(",");
			cb.WriteLine(1, "@OriginalUpdated DateTime");
			cb.WriteLine(") AS");
			cb.WriteLine("");
			cb.WriteLine("DELETE FROM " + table.Name);
			cb.Write(1, "WHERE ");

			FirstField = true;

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.PrimaryKey)
				{
					if (FirstField)
					{
						cb.WriteLine(objColumn.Name + " = @" + objColumn.Name);
						FirstField = false;
					}
					else
						cb.WriteLine(2, "AND " + objColumn.Name + " = @" + objColumn.Name);
				}
			}

			// Add OriginalUpdated.
			cb.WriteLine(2, "AND Updated = @OriginalUpdated");
			cb.WriteLine("");

			// Add error checking.
			cb.WriteLine("IF @@ROWCOUNT <> 1 RAISERROR('Record not deleted', 16, 1)");

			// Create the procedure.
			this.AddProcedure("CoDAL_" + table.Name + "_Delete", cb.CodeText, true);

			return;
		}

		/// <summary>
		/// Generates a pair of user defined functions for each enumeration column.
		/// The functions are used to convert enumeration column name/value pairs.
		/// </summary>
		/// <param name="table">The table that may contain enumeration columns.</param>
		internal void GenerateEnumerationFunctions(Table table)
		{
			foreach(Column col in table.Columns)
			{
				if (col.DataType == SqlDbType.TinyInt && col.Properties.Contains("codal_enumeration"))
				{
					CodeBuilder cb;
					EnumColumn objEnum;

					// Name to value function.
					cb = new CodeBuilder();
					SqlGenerator.AddComment(cb);
					cb.WriteLine("");

					cb.WriteLine("CREATE FUNCTION dbo.CoDAL_" + table.Name + "_" + col.Name + "_NameToValue (@Param AS VarChar(30)) RETURNS TinyInt AS");
					cb.WriteLine("");
					cb.WriteLine("BEGIN");
					cb.WriteLine(1, "DECLARE @RetVal TinyInt");
					cb.WriteLine("");
					cb.WriteLine(1, "SET @RetVal =");
					cb.WriteLine(2, "CASE");

					objEnum = new EnumColumn(col);

					for (int n = 0; n < objEnum.EnumColumnValues.Count; n++)
					{
						EnumColumnValue objEnumValue = objEnum.EnumColumnValues[n];
						cb.WriteLine(2, "WHEN @Param = '" + objEnumValue.Name + "' THEN " + objEnumValue.Value);
					}

					cb.WriteLine(2, "ELSE NULL");
					cb.WriteLine(1, "END");
					cb.WriteLine("");
					cb.WriteLine(1, "RETURN(@RetVal)");
					cb.WriteLine("END");

					this.AddProcedure("CoDAL_" + table.Name + "_" + col.Name + "_NameToValue", cb.CodeText, false);

					// Value to name function.
					cb = new CodeBuilder();
					SqlGenerator.AddComment(cb);
					cb.WriteLine("");

					cb.WriteLine("CREATE FUNCTION dbo.CoDAL_" + table.Name + "_" + col.Name + "_ValueToName (@Param AS TinyInt) RETURNS VarChar(30) AS");
					cb.WriteLine("");
					cb.WriteLine("BEGIN");
					cb.WriteLine(1, "DECLARE @RetVal VarChar(30)");
					cb.WriteLine("");
					cb.WriteLine(1, "SET @RetVal =");
					cb.WriteLine(2, "CASE");

					objEnum = new EnumColumn(col);

					for (int n = 0; n < objEnum.EnumColumnValues.Count; n++)
					{
						EnumColumnValue objEnumValue = objEnum.EnumColumnValues[n];
						cb.WriteLine(2, "WHEN @Param = " + objEnumValue.Value + " THEN '" + objEnumValue.Name + "'");
					}

					cb.WriteLine(2, "ELSE NULL");
					cb.WriteLine(1, "END");
					cb.WriteLine("");
					cb.WriteLine(1, "RETURN(@RetVal)");
					cb.WriteLine("END");

					this.AddProcedure("CoDAL_" + table.Name + "_" + col.Name + "_ValueToName", cb.CodeText, false);
				}
			}
			return;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds a stored procedure/function to the database.
		/// </summary>
		/// <param name="procName">The name of the stored procedure/function.</param>
		/// <param name="procContents">The contents of the stored procedure/function.</param>
		/// <param name="isStoredProcedure">Is this a stored procedure.  If not, then it is assumed to be a function.</param>
		private void AddProcedure(string procName, string procContents, bool isStoredProcedure)
		{
			SqlConnection con;
			SqlCommand cmd;

			// Set up connection/command objects.
			con = new SqlConnection(this._Database.ConnectionString);
			cmd = new SqlCommand();
			cmd.Connection = con;
			cmd.CommandType = CommandType.Text;
			con.Open();

			// Delete previous procedure.
			if (isStoredProcedure)
				cmd.CommandText = "IF OBJECT_ID('dbo." + procName + "') IS NOT NULL DROP PROCEDURE dbo." + procName;
			else
				cmd.CommandText = "IF OBJECT_ID('dbo." + procName + "') IS NOT NULL DROP FUNCTION dbo." + procName;
			cmd.ExecuteNonQuery();

			// Create new procedure.
			cmd.CommandText = procContents;
			cmd.ExecuteNonQuery();

			// Close connection.
			con.Close();
		}

		/// <summary>
		/// Adds a comment to a <paramref name="cb">CodeBuilder</paramref> containing a warning that this is
		/// an auto generated stored procedure that should not be edited manually.  It also shows the date/time
		/// of generation, the user running the generation process and the version of the code generator being
		/// used.
		/// </summary>
		/// <param name="cb">The CodeBuilder to add the comment to.</param>
		private static void AddComment(CodeBuilder cb)
		{
			cb.WriteLine("/*");
			cb.WriteLine(" * Automatically generated procedure.  Please DO NOT edit this procedure directly.");
//			cb.WriteLine(" * Generated " + DateTime.Today.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " by " + Environment.UserName + " using version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " of the code generator.");
			cb.WriteLine(" */");
		}

		#endregion
	}
}
