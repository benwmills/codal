using System;
using System.Data.SqlClient;

namespace MillsSoftware.SqlSchema
{
	public class Server
	{
        #region Properties

        public string Name { get; private set; }
        public DatabaseCollection Databases { get; private set; }
        public string ConnectionString { get; private set; }

        #endregion

		#region Constructors

		public Server(string serverName)
		{
			this.Name = serverName;
			this.ConnectionString = "Server=" + serverName + "; Integrated Security=SSPI";
			GetServerInfo();
		}

		public Server(string serverName, string login, string password)
		{
			this.Name = serverName;
			this.ConnectionString = "Server=" + serverName + "; User ID=" + login + "; Password=" + password + "; Application Name=SqlSchema";
			GetServerInfo();
		}

		private void GetServerInfo()
		{
			// Create connection and command.
			SqlConnection con = new SqlConnection(this.ConnectionString);
			SqlCommand cmd = new SqlCommand("sp_databases", con);
			cmd.CommandType = System.Data.CommandType.StoredProcedure;

			// Run command to get databases on the server.
			con.Open();
			SqlDataReader dr = cmd.ExecuteReader();
			this.Databases = new DatabaseCollection();
			while (dr.Read())
			{
				Database database = new Database(this, dr.GetSqlString(0).Value);
				this.Databases.Add(database);
			}
			dr.Close();
			con.Close();
		}

		#endregion

	}
}
