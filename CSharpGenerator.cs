using System;
using MillsSoftware.SqlSchema;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.CoDAL
{
	/// <summary>
	/// The CSharpGenerator class is used to generate C# data access code for a SQL Server database.
	/// </summary>
	internal class CSharpGenerator
	{
		#region Fields

		/// <summary>
		/// The folder where the generated .cs files should be saved.
		/// </summary>
		private string _FolderPath;

		/// <summary>
		/// The folder where the one time skeleton files should be saved.
		/// </summary>
		private string _SkeletonPath;

		/// <summary>
		/// The namespace for the classes.
		/// </summary>
		private string _NamespaceName;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes an instance of the CSharpGenerator class.
		/// </summary>
		/// <param name="folderPath">The folder where the generated .cs files should be saved.</param>
		/// <param name="skeletonPath">The folder where the one time skeleton files should be saved.</param>
		/// <param name="namespaceName">The namespace used for the generated classes.</param>
		internal CSharpGenerator(string folderPath, string skeletonPath, string namespaceName)
		{
			this._FolderPath = folderPath;
			this._SkeletonPath = skeletonPath;
			this._NamespaceName = namespaceName;
		}
		
		#endregion

		#region Methods

		/// <summary>
		/// Generates a .cs file containing data access code for a table.  See the private methods that
		/// build individual regions for more details.
		/// </summary>
		/// <param name="table">The SQL Server table that data access code is being generated for.</param>
		internal void GenerateBaseClass(Table table)
		{
			CodeBuilder cb = new CodeBuilder();

			// Build the start of the class.
            cb.AddCSharpComment();
			cb.WriteLine("");
			cb.WriteLine(0, "using System;");
			cb.WriteLine(0, "using System.Data;");
            cb.WriteLine(0, "using System.Data.SqlClient;");
            cb.WriteLine(0, "using FluentValidation;");
            cb.WriteLine(0, "using System.ComponentModel;");
            cb.WriteLine("");
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");
//			if (table.Description != "")
//			{
//				cb.WriteLine(1, "/// <summary>");
//				cb.WriteLine(1, "/// " + table.Description);
//				cb.WriteLine(1, "/// </summary>");
//			}
			cb.WriteLine(1, "public abstract class " + table.Name + "Base : IBusinessObject");
			cb.WriteLine(1, "{");
			cb.WriteLine("");

			// Build regions.
			CSharpGenerator.BuildEnumerationsRegion(cb, table);
			CSharpGenerator.BuildFieldsRegion(cb, table);
			CSharpGenerator.BuildPropertiesRegion(cb, table);
			CSharpGenerator.BuildConstructorsRegion(cb, table);
			CSharpGenerator.BuildMethodsRegion(cb, table);
			CSharpGenerator.BuildRetrievalMethods(cb, table);

			// Build the end of the class.
            cb.WriteLine(1, "}");
            cb.WriteLine("");

            // Build the base validator class.
            cb.WriteLine(1, "public abstract class " + table.Name + "ValidatorBase : AbstractValidator<" + this._NamespaceName + "." + table.Name + ">");
            cb.WriteLine(1, "{");

            // Build regions.
            this.BuildValidatorRules(cb, table);

            // Build the end of the class.
            cb.WriteLine(1, "}");

            // End the namespace.
            cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(_FolderPath + @"\" + table.Name + "Base.cs");
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		/// <summary>
		/// Generates a .cs file containing a generic list based collection.
		/// </summary>
		/// <param name="table">The SQL Server table that the collection is being generated for.</param>
		internal void GenerateCollectionClass(Table table)
		{
			string FileName = _SkeletonPath + @"\" + table.Name + "Collection.cs";

			// Don't overwrite a current file.
			if (File.Exists(FileName)) return;

			// Build the stub class.
			CodeBuilder cb = new CodeBuilder();

			cb.WriteLine(0, "using System;");
			cb.WriteLine(0, "using System.Collections.Generic;");
			cb.WriteLine("");
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");
			cb.WriteLine(1, "public class " + table.Name + "Collection : List<" + table.Name + ">");
			cb.WriteLine(1, "{");
			cb.WriteLine(2, "public " + table.Name + "Collection() : base() {}");
			cb.WriteLine(1, "}");
			cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(FileName);
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		/// <summary>
		/// Creates a .cs file containing containing basic skeleton code for the sub class.  The programmer can
		/// add extra code to the skeleton code.  Note that this method will not override an existing file.  
		/// </summary>
		/// <param name="table">The table to generate the stub class for.</param>
		internal void GenerateStubClass(Table table)
		{
			string FileName = _SkeletonPath + @"\" + table.Name + ".cs";

			// Don't overwrite a current file.
			if (File.Exists(FileName)) return;

			// Build the stub class.
			CodeBuilder cb = new CodeBuilder();

			cb.WriteLine(0, "using System;");
			cb.WriteLine(0, "using System.Data.SqlClient;");
			cb.WriteLine("");
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");
			cb.WriteLine(1, "public class " + table.Name + " : " + table.Name + "Base");
			cb.WriteLine(1, "{");
			cb.WriteLine(2, "public " + table.Name + "() : base() {}");
			cb.WriteLine("");
			cb.WriteLine(2, "public " + table.Name + "(SqlDataReader reader) : base(reader) {}");
			cb.WriteLine(1, "}");
			cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(FileName);
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		/// <summary>
		/// Generates a .cs file containing the ValidationException that may be thrown by the Validate methods.
		/// </summary>
		internal void GenerateValidationException()
		{
			CodeBuilder cb = new CodeBuilder();

            cb.AddCSharpComment();
			cb.WriteLine("");

			// Header.
			cb.WriteLine(0, "using System;");
			cb.WriteLine("");
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");

			// Create the exception class.
			cb.WriteLine(0, "public class ValidationException : System.ApplicationException");
			cb.WriteLine(0, "{");
			cb.WriteLine(1, "public ValidationException(string message) : base(message) {}");
			cb.WriteLine(0, "}");

			// Footer.
			cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(_FolderPath + @"\" + "ValidationException.cs");
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		/// <summary>
		/// Generates a .cs file containing the RecordNotFoundException that may be thrown by the GetItem methods.
		/// </summary> 
		internal void GenerateRecordNotFoundException()
		{
			CodeBuilder cb = new CodeBuilder();

            cb.AddCSharpComment();
			cb.WriteLine("");

			// Header.
			cb.WriteLine(0, "using System;");
			cb.WriteLine("");
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");

			// Create the exception class.
			cb.WriteLine(1, "public class RecordNotFoundException : System.ApplicationException");
			cb.WriteLine(1, "{");
			cb.WriteLine(2, "public RecordNotFoundException(string message) : base(message) {}");
			cb.WriteLine(1, "}");

			// Footer.
			cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(_FolderPath + @"\" + "RecordNotFoundException.cs");
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		/// <summary>
		/// Generated a .cs file containing the IBusinessObject interface which all the generated business objects implement.
		/// </summary>
		internal void GenerateIBusinessObject()
		{
			CodeBuilder cb = new CodeBuilder();

            cb.AddCSharpComment();
			cb.WriteLine("");

			// Header.
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");

			// Create the exception class.
			cb.WriteLine(1, "public interface IBusinessObject");
			cb.WriteLine(1, "{");
			cb.WriteLine(2, "void Insert();");
			cb.WriteLine(2, "void Update();");
			cb.WriteLine(2, "void Delete();");
			cb.WriteLine(2, "void Validate();");
			cb.WriteLine(1, "}");

			// Footer.
			cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(_FolderPath + @"\" + "IBusinessObject.cs");
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		/// <summary>
		/// Creates a .cs file containing code to call stored procedures beginning with Query_ and
		/// return a SqlDataReader.
		/// </summary>
		internal void GenerateQueryWrapperClass(Database database)
		{
			CodeBuilder cb = new CodeBuilder();

            cb.AddCSharpComment();

			// Header.
			cb.WriteLine("");
			cb.WriteLine(0, "using System;");
			cb.WriteLine(0, "using System.Data;");
			cb.WriteLine(0, "using System.Data.SqlClient;");
			cb.WriteLine("");
			cb.WriteLine(0, "namespace " + this._NamespaceName);
			cb.WriteLine(0, "{");

			// Create the exception class.
			cb.WriteLine(1, "public class Queries");
			cb.WriteLine(1, "{");

			foreach (StoredProcedure sp in database.StoredProcedures)
			{
				if (sp.Name.StartsWith("Query_"))
				{
					string QueryName = sp.Name.Substring(6);

					cb.WriteLine("");
					cb.Write(2, "public static SqlDataReader " + QueryName + " (");

					foreach (Parameter pm in sp.Parameters)
					{
						if (pm.Name != "@RETURN_VALUE")
							cb.Write(pm.DataType.GetCSharpType(false) + " " + pm.Name.Substring(1) + ", ");
					}

					if (sp.Parameters.Count > 1)
					{
						cb.RemoveLastCharacters(2);
					}

					cb.WriteLine(")");
					cb.WriteLine(2, "{");
					cb.WriteLine(3, "SqlConnection con;");
					cb.WriteLine(3, "SqlCommand cmd;");
					cb.WriteLine(3, "SqlDataReader reader;");
					cb.WriteLine("");
					cb.WriteLine(3, "con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[\"codal\"].ConnectionString);");
					cb.WriteLine(3, "cmd = new SqlCommand(\"" + sp.Name + "\", con);");
					cb.WriteLine(3, "cmd.CommandType = CommandType.StoredProcedure;");

					foreach (Parameter pm in sp.Parameters)
					{
						if (pm.Name != "@RETURN_VALUE")
							cb.WriteLine(3, "cmd.Parameters.AddWithValue(\"" + pm.Name + "\", " + pm.Name.Substring(1) + ");");
					}

					cb.WriteLine(3, "");
					cb.WriteLine(3, "con.Open();");
					cb.WriteLine(3, "reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);");
					cb.WriteLine("");
					cb.WriteLine(3, "return reader;");
					cb.WriteLine(2, "}");
				}
			}

			// Footer.
			cb.WriteLine(1, "}");
			cb.WriteLine(0, "}");

			// Write the code to a file.
			StreamWriter sw = new StreamWriter(_FolderPath + @"\" + "Queries.cs");
			sw.Write(cb.CodeText);
			sw.Flush();
			sw.Close();

			return;
		}

		#endregion

		#region Private methods to build regions of the base class

		/// <summary>
		/// Builds a region containing enumerations.  The enumerations consist of a Columns enumeration
		/// which contains the oridinal positions of columns in the table, as well as an enuemration for
		/// each TinyInt column that has a codegen_enumration extended property.
		/// </summary>
		/// <param name="cb">The CodeBuilder that this region should be added to.</param>
		/// <param name="table">The table that code is being built for.</param>
		private static void BuildEnumerationsRegion(CodeBuilder cb, Table table)
		{
			// Start region.
			cb.WriteLine(1, "#region Enumerations");
			cb.WriteLine("");

			// Columns enumeration.
			cb.WriteLine(2, "public enum Columns : byte");
			cb.WriteLine(2, "{");

			for (int n = 0; n < table.Columns.Count; n++)
			{
				Column objColumn = table.Columns[n];
				cb.Write(3, objColumn.Name + " = " + n.ToString());

				// Add a column for all but the last value.
				if (n == table.Columns.Count - 1)
					cb.WriteLine("");
				else
					cb.WriteLine(",");
			}

			cb.WriteLine(2, "}");

			// TinyInt enumeration columns.
			foreach (Column objColumn in table.Columns)
			{

	            if (objColumn.DataType == SqlDbType.TinyInt && objColumn.Properties.Contains("codal_enumeration"))
				{
	                EnumColumn objEnum = new EnumColumn(objColumn);

					cb.WriteLine("");
					cb.WriteLine(2, "public enum " + objEnum.Name + " : byte");
					cb.WriteLine(2, "{");

					for (int n = 0; n < objEnum.EnumColumnValues.Count; n++)
					{
						EnumColumnValue objEnumValue = objEnum.EnumColumnValues[n];
						cb.Write(3, objEnumValue.Name + " = " + objEnumValue.Value.ToString());

						// Add a column for all but the last value.
						if (n == objEnum.EnumColumnValues.Count - 1)
							cb.WriteLine("");
						else
							cb.WriteLine(",");
					}

					cb.WriteLine(2, "}");
	            }
			}

			// End region.
			cb.WriteLine("");
			cb.WriteLine(1, "#endregion");
			cb.WriteLine("");
		}
	
		/// <summary>
		/// Builds a region containing private fields.  A field is added for each column.  An extra field
		/// is added for each foreign key to represent a reference to another business class.
		/// </summary>
		/// <param name="cb">The CodeBuilder that this region should be added to.</param>
		/// <param name="table">The table that code is being built for.</param>
		private static void BuildFieldsRegion(CodeBuilder cb, Table table)
		{
			// Start region.
	        cb.WriteLine(1, "#region Fields");
			cb.WriteLine("");
			cb.WriteLine(2, "// Columns.");

			// Add table columns.
			foreach (Column objColumn in table.Columns)
			{
				cb.Write(2, "protected ");
				cb.Write(objColumn.GetCSharpType());
				cb.WriteLine(" _" + objColumn.Name + ";");
			}
	
			cb.WriteLine("");
			cb.WriteLine(2, "// Reference objects based on foreign keys.");

			foreach (ForeignKey objFK in table.OutgoingForeignKeys)
			{
//				if (objFK.ColumnPairs.Count > 1)
//					throw new NotSupportedException("Can't handle foreign keys with more than one column");

				if (objFK.ColumnPairs.Count == 1)
                    cb.WriteLine(2, "protected " + objFK.PKTable.Name + " _" + objFK.ColumnPairs[0].FKColumn.FKObjectName() + ";");
				else
					cb.WriteLine(2, "protected " + objFK.PKTable.Name + " _" + objFK.PKTable.Name + ";");
			}

			// End region.
			cb.WriteLine("");
			cb.WriteLine(1, "#endregion");
			cb.WriteLine("");
		}

		/// <summary>
		/// Build a property wrapper around each private field.  Some properties are read only or contain
		/// validation logic as necessary.
		/// </summary>
		/// <param name="cb">The CodeBuilder that this region should be added to.</param>
		/// <param name="table">The table that code is being built for.</param>
		private static void BuildPropertiesRegion(CodeBuilder cb, Table table)
		{
			// Start region.
			cb.WriteLine(1, "#region Properties");

			// Property for each column.
			foreach (Column objColumn in table.Columns)
			{
				cb.WriteLine("");
//				if (objColumn.Description != "")
//				{
//					cb.WriteLine(2, "/// <summary>");
//					cb.WriteLine(2, "/// " + objColumn.Description);
//					cb.WriteLine(2, "/// </summary>");
//				}
			    string DisplayName = !string.IsNullOrWhiteSpace(objColumn.Description) ? objColumn.Description : objColumn.SplitNameIntoWords();
                if (DisplayName != objColumn.Name) cb.WriteLine(2, "[DisplayName(\"" + DisplayName + "\")]");
				cb.WriteLine(2, "public virtual " + objColumn.GetCSharpType() + " " + objColumn.Name);
				cb.WriteLine(2, "{");

				// Get.
				cb.WriteLine(3, "get {return this._" + objColumn.Name + ";}");

				// Set.
				if (objColumn.Identity == false)
				{
					cb.WriteLine(3, "set");
					cb.WriteLine(3, "{");
                    
                    //switch (objColumn.DataType)
                    //{
                    //    case SqlDbType.Char:
                    //    case SqlDbType.VarChar:
                    //    case SqlDbType.NChar:
                    //    case SqlDbType.NVarChar:
                    //        if (objColumn.Length > 0)
                    //        {
                    //            cb.WriteLine(4, "if (value.Length > " + objColumn.Length.ToString() + ")");
                    //            cb.WriteLine(4, "{");
                    //            cb.WriteLine(5, "throw new ArgumentOutOfRangeException(\"" + objColumn.Name + "\", value, \"" + objColumn.Name + " is too long.\");");
                    //            cb.WriteLine(4, "}");
                    //        }
                    //        break;
                    //}

					cb.WriteLine(4, "this._" + objColumn.Name + " = value;");

					foreach (ForeignKey objForeignKey in objColumn.OutgoingForeignKeys)
					{
//						if (objForeignKey.ColumnPairs.Count > 1)
//							throw new NotSupportedException("Multi column foreign keys are not supported.");
    
						if (objForeignKey.ColumnPairs.Count == 1)
                            cb.WriteLine(4, "this._" + objForeignKey.ColumnPairs[0].FKColumn.FKObjectName() + " = null;");
						else
							cb.WriteLine(4, "this._" + objForeignKey.PKTable.Name + " = null;");
					}

					cb.WriteLine(3, "}");
				}
				cb.WriteLine(2, "}");
			}

			// Object references based on foreign keys.
			foreach (ForeignKey objForeignKey in table.OutgoingForeignKeys)
			{
//				if (objForeignKey.ColumnPairs.Count > 1)
//					throw new NotSupportedException("Multi column foreign keys are not currently supported.");

				string ObjectName;
				
				if (objForeignKey.ColumnPairs.Count == 1)
                    ObjectName = objForeignKey.ColumnPairs[0].FKColumn.FKObjectName();
				else
					ObjectName = objForeignKey.PKTable.Name;

//				string TypeName = FKObjectName(objForeignKey.ColumnPairs[0].PKColumn.Name);

				cb.WriteLine("");
				cb.WriteLine(2, "public virtual " + objForeignKey.PKTable.Name + " " + ObjectName);
				cb.WriteLine(2, "{");

				// Get.
				cb.WriteLine(3, "get");
				cb.WriteLine(3, "{");
				cb.Write(4, "if (this._" + ObjectName + " == null");
				foreach(ColumnPair cp in objForeignKey.ColumnPairs)
				{
                    cb.Write(" && this._" + cp.FKColumn.Name + " != " + cp.FKColumn.GetBlankValueForType());
				}
				cb.WriteLine(")");
				cb.Write(5, "this._" + ObjectName + " = " + objForeignKey.PKTable.Name + ".GetItem(");
				foreach(ColumnPair cp in objForeignKey.ColumnPairs)
				{
				    cb.Write("this._" + cp.FKColumn.Name);
                    if (cp.FKColumn.IsCSharpNullableType()) cb.Write(".Value");
                    cb.Write(", ");
				}
				cb.RemoveLastCharacters(2);
				cb.WriteLine(");");
				cb.WriteLine(4, "return this._" + ObjectName + ";");
				cb.WriteLine(3, "}");

				// Set.
				cb.WriteLine(3, "set");
				cb.WriteLine(3, "{");
				cb.WriteLine(4, "this._" + ObjectName + " = value;");
				// Update the column(s) that make up this foreign key.
				cb.WriteLine(4, "if (this._" + ObjectName + " == null)");
				cb.WriteLine(4, "{");
				foreach(ColumnPair cp in objForeignKey.ColumnPairs)
				{
                    //cb.WriteLine(5, "this._" + cp.FKColumn.Name + " = Blank" + cp.FKColumn.Name + ";");
                    cb.WriteLine(5, "this._" + cp.FKColumn.Name + " = " + cp.FKColumn.GetBlankValueForType() + ";");
                }
				cb.WriteLine(4, "}");
				cb.WriteLine(4, "else");
				cb.WriteLine(4, "{");
				foreach(ColumnPair cp in objForeignKey.ColumnPairs)
				{
					cb.WriteLine(5, "this._" + cp.FKColumn.Name + " = this._" + ObjectName + "." + cp.PKColumn.Name + ";");
				}
				cb.WriteLine(4, "}");
				cb.WriteLine(3, "}");

				cb.WriteLine(2, "}");
			}

			// End region.
			cb.WriteLine("");
	        cb.WriteLine(1, "#endregion");
			cb.WriteLine("");
		}

		/// <summary>
		/// Builds two constructors.  One is used to initialize a new instance of the business class.  The other
		/// is used by the retrieval methods to build an instance based on a current row in the table.
		/// </summary>
		/// <param name="cb">The CodeBuilder that this region should be added to.</param>
		/// <param name="table">The table that code is being built for.</param>
		private static void BuildConstructorsRegion(CodeBuilder cb, Table table)
		{
			// Start region.
			cb.WriteLine(1, "#region Constructors");
			cb.WriteLine("");

	        // Build a no parameter constructor.
			cb.WriteLine(2, "protected " + table.Name + "Base()");
			cb.WriteLine(2, "{");

            foreach (Column objColumn in table.Columns)
            {
                cb.Write(3, "this._" + objColumn.Name + " = ");

                switch (objColumn.DataType)
                {
                    case SqlDbType.TinyInt:
                        if (objColumn.Properties.Contains("codal_enumeration"))
                        {
                            EnumColumn objEnumColumn = new EnumColumn(objColumn);
                            cb.Write(objColumn.Name + "Enum." + objEnumColumn.EnumColumnValues[0].Name + ";");
                        }
                        else if (objColumn.AllowNulls)
                        {
                            cb.Write("null;");
                        }
                        else
                        {
                            //cb.Write("Blank" + objColumn.Name + ";");
                            cb.Write(objColumn.GetBlankValueForType());
                            cb.Write(";");
                        }
                        break;

                    case SqlDbType.Bit:
                        cb.Write("false;");
                        break;

                    case SqlDbType.Int:
                    case SqlDbType.SmallInt:
                    case SqlDbType.BigInt:
                    case SqlDbType.DateTime:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.Decimal:
                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Char:
                    case SqlDbType.VarChar:
                    case SqlDbType.Text:
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.NText:
                        if (objColumn.AllowNulls)
                        {
                            cb.Write("null;");
                        }
                        else
                        {
                            //cb.Write("Blank" + objColumn.Name + ";");
                            cb.Write(objColumn.GetBlankValueForType());
                            cb.Write(";");
                        }
                        break;

                    default:
                        throw new NotSupportedException("Unsupported data type in constructor");
                }

                cb.WriteLine("");
            }

	        cb.WriteLine(2, "}");
			cb.WriteLine("");

			// Build a  constructor that takes a reader object.
			cb.WriteLine(2, "protected " + table.Name + "Base" + "(SqlDataReader reader)");
	        cb.WriteLine(2, "{");

			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.AllowNulls == false)
				{
					cb.Write(3, "this._" + objColumn.Name + " = ");
					if (objColumn.Properties.Contains("codal_enumeration"))
						cb.Write("(" + objColumn.Name + "Enum) ");
                    cb.WriteLine("reader." + objColumn.DataType.GetReaderFunction() + "((byte) Columns." + objColumn.Name + ");");
				}
				else
				{
					cb.WriteLine(3, "if (reader.IsDBNull((byte) Columns." + objColumn.Name + "))");
                    //cb.WriteLine(4, "this._" + objColumn.Name + " = Blank" + objColumn.Name + ";");
                    cb.WriteLine(4, "this._" + objColumn.Name + " = null;");
                    cb.WriteLine(3, "else");
                    cb.WriteLine(4, "this._" + objColumn.Name + " = reader." + objColumn.DataType.GetReaderFunction() + "((byte) Columns." + objColumn.Name + ");");
				}
			}

		    cb.WriteLine(2, "}");

			// End region.
	        cb.WriteLine("");
	        cb.WriteLine(1, "#endregion");
	        cb.WriteLine("");
		}

		/// <summary>
		/// Builds insert, update, delete and validate methods.
		/// </summary>
		/// <param name="cb">The CodeBuilder that this region should be added to.</param>
		/// <param name="table">The table that code is being built for.</param>
		private static void BuildMethodsRegion(CodeBuilder cb, Table table)
		{
			// Start region.
			cb.WriteLine(1, "#region Methods");
			cb.WriteLine("");

			// -------------------------------------------------------------------------------
			//	Insert.
			// -------------------------------------------------------------------------------

			cb.WriteLine(2, "public virtual void Insert()");
			cb.WriteLine(2, "{");

			// Setup connection and command.
			cb.WriteLine(3, "DateTime dtCurrentTime;");
			cb.WriteLine(3, "SqlConnection con;");
			cb.WriteLine(3, "SqlCommand cmd;");
			cb.WriteLine(3, "SqlParameter param;");
			cb.WriteLine("");
			cb.WriteLine(3, "this.Validate();");
			cb.WriteLine("");
			cb.WriteLine(3, "con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[\"codal\"].ConnectionString);");
			cb.WriteLine("");
			cb.WriteLine(3, "try");
			cb.WriteLine(3, "{");
			cb.WriteLine(4, "dtCurrentTime = DateTime.Now;");
			cb.WriteLine("");
			cb.WriteLine(4, "cmd = new SqlCommand(\"CoDAL_" + table.Name + "_Insert\", con);");	
			cb.WriteLine(4, "cmd.CommandType = CommandType.StoredProcedure;");
			cb.WriteLine("");

			// Add parameters for each column.
			foreach (Column objColumn in table.Columns)
			{
				cb.WriteLine(4, "param = new SqlParameter(\"@" + objColumn.Name + "\", SqlDbType." + objColumn.DataType.ToString() + ");");

				if (objColumn.Name == "Updated")
					cb.WriteLine(4, "param.Value = dtCurrentTime;");
				else if (objColumn.AllowNulls == false)
					cb.WriteLine(4, "param.Value = this._" + objColumn.Name + ";");
				else
				{
                    if (objColumn.GetCSharpType() == "string")
                    {
    					cb.WriteLine(4, "if (string.IsNullOrWhiteSpace(this._" + objColumn.Name + "))");
                    }
                    else
                    {
    					cb.WriteLine(4, "if (this._" + objColumn.Name + " == null)");
                    }
					// HACK:  SqlDecimal.Null does not work (seems like a bug).  We have to use System.DBNull.Value instead.
					if (objColumn.DataType == SqlDbType.Decimal)
						cb.WriteLine(5, "param.Value = System.DBNull.Value;");
					else
                        cb.WriteLine(5, "param.Value = System.Data.SqlTypes." + objColumn.DataType.GetDBNullValue() + ";");
					cb.WriteLine(4, "else");
					cb.WriteLine(5, "param.Value = this._" + objColumn.Name + ";");
				}

				if (objColumn.Identity)
					cb.WriteLine(4, "param.Direction = ParameterDirection.Output;");

				cb.WriteLine(4, "cmd.Parameters.Add(param);");
			}

			// Execute command.
			cb.WriteLine("");
			cb.WriteLine(4, "con.Open();");
			cb.WriteLine(4, "cmd.ExecuteNonQuery();");

			// Get the identity value.
			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.Identity)
				{
					cb.WriteLine("");
                    cb.WriteLine(4, "this._" + objColumn.Name + " = (" + objColumn.GetCSharpType() + ") cmd.Parameters[\"@" + objColumn.Name + "\"].Value;");
				}
			}

			// Update the last updated property.
			cb.WriteLine("");
	        cb.WriteLine(4, "this._Updated = dtCurrentTime;");

			cb.WriteLine(3, "}");

			// Finally block.
			cb.WriteLine(3, "finally");
			cb.WriteLine(3, "{");
			cb.WriteLine(4, "if (con != null) con.Close();");
	        cb.WriteLine(3, "}");

	        cb.WriteLine(2, "}");

			// -------------------------------------------------------------------------------
			// Update.
			// -------------------------------------------------------------------------------

			cb.WriteLine("");
	        cb.WriteLine(2, "public virtual void Update()");
	        cb.WriteLine(2, "{");

	        // Setup connection and command.
			cb.WriteLine(3, "DateTime dtCurrentTime;");
			cb.WriteLine(3, "SqlConnection con;");
			cb.WriteLine(3, "SqlCommand cmd;");
			cb.WriteLine(3, "SqlParameter param;");
			cb.WriteLine("");
			cb.WriteLine(3, "this.Validate();");
			cb.WriteLine("");
			cb.WriteLine(3, "con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[\"codal\"].ConnectionString);");
			cb.WriteLine("");
			cb.WriteLine(3, "try");
			cb.WriteLine(3, "{");
			cb.WriteLine(4, "dtCurrentTime = DateTime.Now;");
			cb.WriteLine("");
			cb.WriteLine(4, "cmd = new SqlCommand(\"CoDAL_" + table.Name + "_Update\", con);");
			cb.WriteLine(4, "cmd.CommandType = CommandType.StoredProcedure;");
			cb.WriteLine("");

			// Add parameters for each column.
			foreach (Column objColumn in table.Columns)
			{
				cb.WriteLine(4, "param = new SqlParameter(\"@" + objColumn.Name + "\", SqlDbType." + objColumn.DataType.ToString() + ");");

				if (objColumn.Name == "Updated")
					cb.WriteLine(4, "param.Value = dtCurrentTime;");
				else if (objColumn.AllowNulls == false)
					cb.WriteLine(4, "param.Value = this._" + objColumn.Name + ";");
				else
				{
                    if (objColumn.GetCSharpType() == "string")
				    {
				        cb.WriteLine(4, "if (string.IsNullOrWhiteSpace(this._" + objColumn.Name + "))");
				    }
				    else
				    {
				        cb.WriteLine(4, "if (this._" + objColumn.Name + " == null)");
				    }
				    // HACK:  SqlDecimal.Null does not work (seems like a bug).  We have to use System.DBNull.Value instead.
					if (objColumn.DataType == SqlDbType.Decimal)
						cb.WriteLine(5, "param.Value = System.DBNull.Value;");
					else
                        cb.WriteLine(5, "param.Value = System.Data.SqlTypes." + objColumn.DataType.GetDBNullValue() + ";");
					cb.WriteLine(4, "else");
					cb.WriteLine(5, "param.Value = this._" + objColumn.Name + ";");
				}

				cb.WriteLine(4, "cmd.Parameters.Add(param);");
			}

	        // Add original updated date/time so that an error is thrown if the record has benn updated since retrieval.
		    cb.WriteLine("");
			cb.WriteLine(4, "param = new SqlParameter(\"@OriginalUpdated\", SqlDbType.DateTime);");
	        cb.WriteLine(4, "param.Value = this._Updated;");
		    cb.WriteLine(4, "cmd.Parameters.Add(param);");

	        // Execute command.
		    cb.WriteLine("");
			cb.WriteLine(4, "con.Open();");
			cb.WriteLine(4, "cmd.ExecuteNonQuery();");

	        // Update the last updated property.
			cb.WriteLine("");
	        cb.WriteLine(4, "this._Updated = dtCurrentTime;");
			cb.WriteLine(3, "}");
			
			// Finally block.
	        cb.WriteLine(3, "finally");
			cb.WriteLine(3, "{");
			cb.WriteLine(4, "if (con != null) con.Close();");
			cb.WriteLine(3, "}");

			cb.WriteLine(2, "}");

			// -------------------------------------------------------------------------------
			// Delete.
			// -------------------------------------------------------------------------------

			cb.WriteLine("");
	        cb.WriteLine(2, "public virtual void Delete()");
	        cb.WriteLine(2, "{");

			// Setup connection and command.
			cb.WriteLine(3, "DateTime dtCurrentTime;");
			cb.WriteLine(3, "SqlConnection con;");
	        cb.WriteLine(3, "SqlCommand cmd;");
		    cb.WriteLine(3, "SqlParameter param;");
			cb.WriteLine("");
			cb.WriteLine(3, "con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[\"codal\"].ConnectionString);");
			cb.WriteLine("");
			cb.WriteLine(3, "try");
			cb.WriteLine(3, "{");
			cb.WriteLine(4, "dtCurrentTime = DateTime.Now;");
			cb.WriteLine("");
			cb.WriteLine(4, "cmd = new SqlCommand(\"CoDAL_" + table.Name + "_Delete\", con);");
	        cb.WriteLine(4, "cmd.CommandType = CommandType.StoredProcedure;");
		    cb.WriteLine("");

	        // Add parameters for each primary key column.
			foreach (Column objColumn in table.Columns)
			{
				if (objColumn.PrimaryKey == true)
				{
					cb.WriteLine(4, "param = new SqlParameter(\"@" + objColumn.Name + "\", SqlDbType." + objColumn.DataType.ToString() + ");");
					cb.WriteLine(4, "param.Value = this._" + objColumn.Name + ";");
					cb.WriteLine(4, "cmd.Parameters.Add(param);");
				}
			}

	        // Add original updated date/time so that an error is thrown if the record has benn updated since retrieval.
			cb.WriteLine("");
			cb.WriteLine(4, "param = new SqlParameter(\"@OriginalUpdated\", SqlDbType.DateTime);");
			cb.WriteLine(4, "param.Value = this._Updated;");
			cb.WriteLine(4, "cmd.Parameters.Add(param);");

			// Execute command.
			cb.WriteLine("");
			cb.WriteLine(4, "con.Open();");
			cb.WriteLine(4, "cmd.ExecuteNonQuery();");

			cb.WriteLine(3, "}");
			
			// Finally block.
			cb.WriteLine(3, "finally");
			cb.WriteLine(3, "{");
			cb.WriteLine(4, "if (con != null) con.Close();");
	        cb.WriteLine(3, "}");

	        cb.WriteLine(2, "}");

			// -------------------------------------------------------------------------------
			// Validate.
			// -------------------------------------------------------------------------------
			cb.WriteLine("");
	        cb.WriteLine(2, "public virtual void Validate()");
	        cb.WriteLine(2, "{");

            //foreach (Column objColumn in table.Columns)
            //{
            //    if (objColumn.AllowNulls == false && objColumn.DataType != SqlDbType.Bit && !objColumn.Properties.Contains("codal_enumeration") && objColumn.Name != "Updated" && objColumn.Identity == false)
            //    {
            //        cb.WriteLine(3, "if (this._" + objColumn.Name + " == " + CSharpGenerator.GetBlankValueForType(objColumn) + ")");
            //        cb.WriteLine(4, "throw new ValidationException(\"" + objColumn.Name + " cannot be blank.\");");
            //    }
            //}

			cb.WriteLine(2, "}");

			// End region.
			cb.WriteLine("");
	        cb.WriteLine(1, "#endregion");
	        cb.WriteLine("");
		}

		/// <summary>
		/// Builds methods to create instances of this business class base on records in the database.  Note
		/// that these methods are built based on retrieval stored procedures that already exist in the database.
		/// For example, if the table is called Account, then this method, looks for stored procedures starting
		/// with Account_GetItem or Account_GetList
		/// </summary>
		/// <param name="cb">The CodeBuilder that this region should be added to.</param>
		/// <param name="table">The table that code is being built for.</param>
		private static void BuildRetrievalMethods(CodeBuilder cb, Table table)
		{
			// Start region.
	        cb.WriteLine(1, "#region Retrieval Methods");

			// Create a static method used to create an instance of the appropriate type.  This method
			// can be "overidden" in the main class to allow creation of subclasses depending on
			// some business logic.
			cb.WriteLine("");
			cb.WriteLine(2, "public static " + table.Name + " GetInstance (SqlDataReader reader)");
			cb.WriteLine(2, "{");
			cb.WriteLine(3, "return new " + table.Name + " (reader);");
			cb.WriteLine(2, "}");

			// Look for GetItem and GetList retrieval stored procedures and create a retrieval function for each.
			foreach (StoredProcedure sp in table.Database.StoredProcedures)
			{
				if (sp.Name.StartsWith(table.Name + "_GetItem"))
				{
					table.IsRetrievalProcedureValid(sp);

					string FunctionName = sp.Name.Substring(table.Name.Length + 1);

					cb.WriteLine("");
					cb.Write(2, "public static " + table.Name + " " + FunctionName + "(");

					foreach (Parameter pm in sp.Parameters)
					{
						if (pm.Name != "@RETURN_VALUE")
							cb.Write(pm.DataType.GetCSharpType(false) + " " + pm.Name.Substring(1) + ", ");
					}

					cb.RemoveLastCharacters(2);

					cb.Write(", bool throwErrorIfNoResult = true");
					cb.WriteLine(")");
                    cb.WriteLine(2, "{");
					cb.WriteLine(3, "SqlConnection con;");
					cb.WriteLine(3, "SqlCommand cmd;");
					cb.WriteLine(3, "SqlDataReader reader;");
					cb.WriteLine(3, table.Name + " obj" + table.Name + " = null;");
					cb.WriteLine("");
					cb.WriteLine(3, "con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[\"codal\"].ConnectionString);");
					cb.WriteLine(3, "cmd = new SqlCommand(\"" + sp.Name + "\", con);");
					cb.WriteLine(3, "cmd.CommandType = CommandType.StoredProcedure;");

					foreach (Parameter pm in sp.Parameters)
					{
						if (pm.Name != "@RETURN_VALUE")
							cb.WriteLine(3, "cmd.Parameters.AddWithValue(\"" + pm.Name + "\", " + pm.Name.Substring(1) + ");");
					}

					cb.WriteLine(3, "");
					cb.WriteLine(3, "con.Open();");
					cb.WriteLine(3, "reader = cmd.ExecuteReader(CommandBehavior.SingleRow);");
					cb.WriteLine("");
					cb.WriteLine(3, "if (reader.Read())");
//					cb.WriteLine(4, "obj" + table.Name + " = new " + table.Name + " (reader);");
					cb.WriteLine(4, "obj" + table.Name + " = " + table.Name + ".GetInstance(reader);");
					cb.WriteLine(3, "else if (throwErrorIfNoResult)");
					cb.WriteLine(3, "{");
					cb.WriteLine(4, "reader.Close();");
					cb.WriteLine(4, "con.Close();");
					cb.WriteLine(4, "throw new RecordNotFoundException(\"" + table.Name + " not found.\");");
					cb.WriteLine(3, "}");
					cb.WriteLine("");
					cb.WriteLine(3, "reader.Close();");
					cb.WriteLine(3, "con.Close();");
					cb.WriteLine("");
					cb.WriteLine(3, "return obj" + table.Name + ";");
					cb.WriteLine(2, "}");
				}
				else if (sp.Name.StartsWith(table.Name + "_GetList"))
				{
					table.IsRetrievalProcedureValid(sp);

					string FunctionName = sp.Name.Substring(table.Name.Length + 1);
	
					cb.WriteLine("");
					cb.Write(2, "public static " + table.Name + "Collection " + FunctionName + "(");
	
					foreach (Parameter pm in sp.Parameters)
					{
						if (pm.Name != "@RETURN_VALUE")
                            cb.Write(pm.DataType.GetCSharpType(false) + " " + pm.Name.Substring(1) + ", ");
					}
	
					// Note that the @RETURN_VALUE always exists.
					if (sp.Parameters.Count > 1)
						cb.RemoveLastCharacters(2);
	
					cb.WriteLine(")");
					cb.WriteLine(2, "{");
					cb.WriteLine(3, "SqlConnection con;");
					cb.WriteLine(3, "SqlCommand cmd;");
					cb.WriteLine(3, "SqlDataReader reader;");
					cb.WriteLine(3, table.Name + "Collection " + table.Name + "s = new " + table.Name + "Collection();");
					cb.WriteLine("");
					cb.WriteLine(3, "con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[\"codal\"].ConnectionString);");
					cb.WriteLine(3, "cmd = new SqlCommand(\"" + sp.Name + "\", con);");
					cb.WriteLine(3, "cmd.CommandType = CommandType.StoredProcedure;");
	
					foreach (Parameter pm in sp.Parameters)
					{
						if (pm.Name != "@RETURN_VALUE")
							cb.WriteLine(3, "cmd.Parameters.AddWithValue(\"" + pm.Name + "\", " + pm.Name.Substring(1) + ");");
					}
	
					cb.WriteLine("");
					cb.WriteLine(3, "con.Open();");
					cb.WriteLine(3, "reader = cmd.ExecuteReader();");
					cb.WriteLine("");
					cb.WriteLine(3, "while (reader.Read())");
					cb.WriteLine(3, "{");
//					cb.WriteLine(4, table.Name + "s.Add(new " + table.Name + "(reader));");
					cb.WriteLine(4, table.Name + "s.Add(" + table.Name + ".GetInstance(reader));");
					cb.WriteLine(3, "}");
					cb.WriteLine("");
					cb.WriteLine(3, "reader.Close();");
					cb.WriteLine(3, "con.Close();");
					cb.WriteLine("");
					cb.WriteLine(3, "return " + table.Name + "s;");
					cb.WriteLine(2, "}");
				}
			}

			// End region.
	        cb.WriteLine("");
	        cb.WriteLine(1, "#endregion");
	        cb.WriteLine("");
		}

        /// <summary>
        /// Build a base FluentValidation class for the table based off simple field properties.
        /// For example a VarChar(20) NOT NULL field will have NotEmpty() and Length(0, 20) FluentValidation rules set.
        /// </summary>
        /// <param name="cb">The CodeBuilder that this region should be added to.</param>
        /// <param name="table">The table that code is being built for.</param>
        private void BuildValidatorRules(CodeBuilder cb, Table table)
        {
            cb.WriteLine(2, "public " + table.Name + "ValidatorBase()");
            cb.WriteLine(2, "{");

            foreach (Column column in table.Columns)
            {
                if (column.GetCSharpType() == "string")
                {
                    // Rules for non-nullable string and max string length.
                    if (!column.AllowNulls || column.Length > 0)
                    {
                        cb.Write(3, "RuleFor(m => m." + column.Name + ")");
                        if (!column.AllowNulls) cb.Write(".NotEmpty()");
                        if (column.Length > 0) cb.Write(".Length(0, " + column.Length + ")");
                        cb.WriteLine(";");
                    }
                }
                else if (column.GetCSharpType() == "DateTime")
                {
                    // Rules for non-nullable DateTime.
                    if (!column.AllowNulls && column.Name != "Updated")
                    {
                        cb.WriteLine(3, "RuleFor(m => m." + column.Name + ").NotEmpty();");
                    }
                }
            }

            cb.WriteLine(2, "}");
        }

		#endregion
	}
}
