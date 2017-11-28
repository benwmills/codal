using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MillsSoftware.SqlSchema;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.CoDAL
{
    public static class CSharpExtensionMethods
    {
        /// <overloads>
        /// Returns the CSharp type that should be used to represent a SQL Server data type.  Overloaded versions 
        /// allow enumerations for columns and stored procedure parameters.
        /// </overloads>
        /// <summary>
        /// Returns the CSharp type that should be used to represent a SQL Server data type.
        /// </summary>
        /// <param name="sqlDbType">The SQL Server data type.</param>
        /// <param name="nullable">Should the CSharp type allow nulls?</param>
        /// <returns>The C# type to use.</returns>
        internal static string GetCSharpType(this SqlDbType sqlDbType, bool nullable)
        {
            switch (sqlDbType)
            {
                case SqlDbType.Int:
                    return nullable ? "int?" : "int";

                case SqlDbType.SmallInt:
                    return nullable ? "short?" : "short";

                case SqlDbType.TinyInt:
                    return nullable ? "byte?" : "byte";

                case SqlDbType.BigInt:
                    return nullable ? "long?" : "long";

                case SqlDbType.Char:
                case SqlDbType.VarChar:
                case SqlDbType.Text:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                    return "string";

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    return nullable ? "DateTime?" : "DateTime";

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                case SqlDbType.Decimal:
                    return nullable ? "decimal?" : "decimal";

                case SqlDbType.Bit:
                    return nullable ? "bool?" : "bool";

                default:
                    throw new NotSupportedException("Unsupported SqlDbType");
            }
        }

        /// <summary>
        /// This function returns the correct SqlDataReader function to use for a given SqlDbType.
        /// </summary>
        /// <param name="sqlDbType">The SQL Server data type being read.</param>
        /// <returns>The name of the correct reader retrieval function.</returns>
        internal static string GetReaderFunction(this SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.Int:
                    return "GetInt32";

                case SqlDbType.SmallInt:
                    return "GetInt16";

                case SqlDbType.TinyInt:
                    return "GetByte";

                case SqlDbType.BigInt:
                    return "GetInt64";

                case SqlDbType.Char:
                case SqlDbType.VarChar:
                case SqlDbType.Text:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                    return "GetString";

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    return "GetDateTime";

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return "GetDecimal";

                case SqlDbType.Decimal:
                    return "GetDecimal";

                case SqlDbType.Bit:
                    return "GetBoolean";

                default:
                    throw new NotSupportedException("Unsupported SqlDbType");
            }
        }

        /// <summary>
        /// This function returns value that a SqlParameter should be set to if the column should be set to null.
        /// </summary>
        /// <param name="sqlDbType">The SQL Server data type that should be set to null.</param>
        /// <returns>The value that represents null.</returns>
        internal static string GetDBNullValue(this SqlDbType sqlDbType)
        {

            switch (sqlDbType)
            {
                case SqlDbType.Int:
                    return "SqlInt32.Null";

                case SqlDbType.SmallInt:
                    return "SqlInt16.Null";

                case SqlDbType.TinyInt:
                    return "SqlByte.Null";

                case SqlDbType.BigInt:
                    return "SqlInt64.Null";

                case SqlDbType.Char:
                case SqlDbType.VarChar:
                case SqlDbType.Text:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                    return "SqlString.Null";

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    return "SqlDateTime.Null";

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return "SqlMoney.Null";

                case SqlDbType.Decimal:
                    return "SqlDecimal.Null";

                case SqlDbType.Bit:
                    throw new ApplicationException("Bit data type cannot be null.");

                default:
                    throw new NotSupportedException("Unsupported SqlDbType");
            }
        }

        /// <summary>
        /// Returns the CSharp type that should be used to represent a SQL Server column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The C# type to use.</returns>
        internal static string GetCSharpType(this Column column)
        {
            if (column.Properties.Contains("codal_enumeration"))
            {
                return column.Name + "Enum";
            }
            else
            {
                return column.DataType.GetCSharpType(column.AllowNulls);
            }
        }

        /// <summary>
        /// Determines if the database column will be represented by a C# nullable type.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal static bool IsCSharpNullableType(this Column column)
        {
            if (!column.AllowNulls) return false;

            switch (column.DataType)
            {
                case SqlDbType.Int:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                case SqlDbType.BigInt:
                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                case SqlDbType.Decimal:
                case SqlDbType.Bit:
                    return true;

                case SqlDbType.Char:
                case SqlDbType.VarChar:
                case SqlDbType.Text:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                    return false;

                default:
                    throw new NotSupportedException("Unsupported SqlDbType");
            }
        }

        /// <summary>
        /// This function returns the value that the code generator will determine as blank.
        /// Blank values are saved to the database as null.
        /// </summary>
        /// <param name="column">The column to determin the "blank" value for</param>
        /// <returns>
        /// A string containing a "blank" value.  An example is short.MinValue for a column with a DataType of
        /// SmallInt.
        /// </returns>
        internal static string GetBlankValueForType(this Column column)
        {
            if (column.Properties.Contains("codal_enumeration"))
            {
                throw new ApplicationException("Enumerations cannot have blank values");
            }
            else if (column.AllowNulls)
            {
                return "null";
            }
            else
            {
                switch (column.DataType)
                {
                    case SqlDbType.Int:
                        return "0";

                    case SqlDbType.SmallInt:
                        return "0";

                    case SqlDbType.TinyInt:
                        return "0";

                    case SqlDbType.BigInt:
                        return "0";

                    case SqlDbType.Char:
                    case SqlDbType.VarChar:
                    case SqlDbType.Text:
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.NText:
                        return "null";

                    case SqlDbType.DateTime:
                    case SqlDbType.SmallDateTime:
                        return "DateTime.MinValue";

                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Decimal:
                        return "0m";

                    case SqlDbType.Bit:
                        throw new ApplicationException("Bit data type cannot be blank.");

                    default:
                        throw new NotSupportedException("Unsupported SqlDbType");
                }
            }
        }

        /// <summary>
        /// This function returns the object reference name for the properties that represent foreign keys.
        /// </summary>
        /// <param name="column">The column in the child table.</param>
        /// <returns>A string to be used as the name for the object reference representing a foreign key.</returns>
        internal static string FKObjectName(this Column column)
        {
            if (column.Name.EndsWith("ID"))
                return column.Name.Substring(0, column.Name.Length - 2);
            else if (column.Name.EndsWith("Code"))
                return column.Name.Substring(0, column.Name.Length - 4);
            else
                return column.Name + "Obj";
            //				throw new NotSupportedException("All foreign key columns must end in ID or Code");
        }

        internal static string SplitNameIntoWords(this Column column)
        {
            //return System.Text.RegularExpressions.Regex.Replace(column.Name, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim().Replace("I D", "ID");
            //return System.Text.RegularExpressions.Regex.Replace(column.Name, "([A-Z][a-z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();

            // Pattern matches upper case letter which must be preceded by a lower case letter (notice the lookbehind operator).
            return System.Text.RegularExpressions.Regex.Replace(column.Name, "(?<=[a-z])([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        /// <summary>
        /// This procedure checks to see if a retrieval stored procedure can be executed and that the fields
        /// returned by match the table definition.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure that acts as a object retrieval query.</param>
        /// <param name="table">The table that data is being retrieved from.</param>
        internal static void IsRetrievalProcedureValid(this Table table, StoredProcedure storedProcedure)
        {
            SqlDataReader dr;

            SqlConnection con = new SqlConnection(table.Database.ConnectionString);
            SqlCommand cmd = new SqlCommand(storedProcedure.Name, con);
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (Parameter pm in storedProcedure.Parameters)
            {
                if (pm.Name != "@RETURN_VALUE")
                {
                    cmd.Parameters.Add(pm.Name, pm.DataType);
                }
            }

            con.Open();

            try
            {
                dr = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 208 && ex.Message.Contains("#temp"))
                {
                    // Do nothing.  CommandBehavior.SchemaOnly doesn't work when temporary tables are involved.  This fix expects the temporary table to start with #temp.
                    con.Close();
                    return;
                }
                else
                {
                    throw new ApplicationException("The stored procedure " + storedProcedure.Name + " could not be executed for the following reason:\r\n\r\n" + ex.Message);
                }
            }

            for (int n = 0; n <= table.Columns.Count - 1; n++)
            {
                if (table.Columns[n].Name != dr.GetName(n))
                {
                    throw new ApplicationException("The retrieval method " + storedProcedure.Name + " is not consistent with the table " + table.Name + ".  The 1st mismatch is at column " + table.Columns[n].Name);
                }
            }

            dr.Close();
            con.Close();

            return;
        }

        /// <summary>
        /// Add a comment to a CodeBuilder which instructs any developer to not edit this source code file.
        /// It also embeds the date and time the code was generated, the user and the version of the generator.
        /// </summary>
        /// <param name="cb">The CodeBuilder to add the comment to.</param>
        internal static void AddCSharpComment(this CodeBuilder cb)
        {
            cb.WriteLine("/*");
            cb.WriteLine(" * Automatically generated class.  Please DO NOT edit this class directly.");
            cb.WriteLine(" */");
        }
    }
}
