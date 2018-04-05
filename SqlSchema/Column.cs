using System;
using System.Data;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class Column
	{
        #region Properties

        public Table Table { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SqlDbType DataType { get; set; }
        public bool AllowNulls { get; set; }
        public bool Identity { get; set; }
        public short Length { get; set; }
        public short Precision { get; set; }
        public short Scale { get; set; }
        public bool PrimaryKey { get; set; }
        public ColumnPropertyCollection Properties { get; private set; }
        public ForeignKeyCollection OutgoingForeignKeys { get; private set; }

        public string LongDataType
        {
            get
            {
                switch (this.DataType)
                {
                    case SqlDbType.Char:
                    case SqlDbType.VarChar:
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                        if (this.Length == 0)
                        {
                            return this.DataType.ToString() + "(MAX)";
                        }
                        else
                        {
                            return this.DataType.ToString() + "(" + this.Length.ToString() + ")";
                        }
                    case SqlDbType.Decimal:
                        return this.DataType.ToString() + "(" + this.Precision.ToString() + ", " + this.Scale.ToString() + ")";
                    default:
                        return this.DataType.ToString();
                }
            }
        }

        #endregion

		#region Constructors

		public Column()
		{
//			this._Table = null;
			this.Name = "";
			this.Description = "";
			//this._AllowNulls = false;
			//this._Identity = false;
			this.Length = 0;
			this.Precision = 0;
			this.Scale = 0;
			this.Properties = new ColumnPropertyCollection();
			this.OutgoingForeignKeys = new ForeignKeyCollection();
		}

		#endregion

		#region Methods

		static internal SqlDbType ConvertDataType(string dataType)
		{
			switch (dataType)
			{
				case "bigint":
					return SqlDbType.BigInt;
				case "binary":
					return SqlDbType.Binary;
				case "bit":
					return SqlDbType.Bit;
				case "char":
					return SqlDbType.Char;
				case "datetime":
				case "datetime2":
					return SqlDbType.DateTime;
				case "decimal":
					return SqlDbType.Decimal;
				case "float":
					return SqlDbType.Float;
				case "image":
					return SqlDbType.Image;
				case "int":
					return SqlDbType.Int;
				case "money":
					return SqlDbType.Money;
				case "nchar":
					return SqlDbType.NChar;
				case "ntext":
					return SqlDbType.NText;
				case "nvarchar":
					return SqlDbType.NVarChar;
				case "real":
					return SqlDbType.Real;
				case "smalldatetime":
					return SqlDbType.SmallDateTime;
				case "smallint":
					return SqlDbType.SmallInt;
				case "smallmoney":
					return SqlDbType.SmallMoney;
				case "text":
					return SqlDbType.Text;
				case "timestamp":
					return SqlDbType.Timestamp;
				case "tinyint":
					return SqlDbType.TinyInt;
				case "uniqueidentifier":
					return SqlDbType.UniqueIdentifier;
				case "varbinary":
					return SqlDbType.VarBinary;
				case "varchar":
					return SqlDbType.VarChar;
				case "variant":
					return SqlDbType.Variant;
				default:
					throw new ApplicationException("Unrecognized SQL Server data type");
			}
		}

		#endregion
	}
}
