using System;
using MillsSoftware.SqlSchema;
using System.IO;

namespace MillsSoftware.CoDAL
{
	/// <summary>
	/// Main entry class for the application.  This class collects and validates the required command line
	/// parameters and kicks off the code generation process.
	/// </summary>
	internal class EntryPoint
	{
		/// <summary>
		/// The main entry point for the application.  Collect and validate the command line parameters.
		/// </summary>
		/// <returns>
		/// The return value.  Possible values are:
		/// <list type="bullet">
		///		<item>0 = There were no errors.</item>
		///		<item>1 = There were errors.</item>
		/// </list>
		/// </returns>
		[STAThread]
		private static int Main(string[] args)
		{
			int ReturnValue = 0;

			// When in debug mode, do not catch exceptions, so that the debugger breaks at the correct line.
			// When in release mode, the exceptions are caught and written to the console.
			#if (DEBUG)
				EntryPoint.Kickoff(args);
			#else
				try
				{
					EntryPoint.Kickoff(args);
					ReturnValue = 0;
				}
				catch (Exception ex)
				{
					Console.WriteLine("");
					Console.WriteLine("ERROR MESSAGE");
					Console.WriteLine("--------------------------------------------------");
					Console.WriteLine(ex.Message);
					Console.WriteLine("--------------------------------------------------");
					Console.WriteLine("");
					ReturnValue = 1;
				}
			#endif

			return ReturnValue;
		}

		private static void Kickoff(string[] args)
		{
			// Loop through the arguments.
			string LastArg = "";
			string ServerName = "";
			string DatabaseName = "";
			string Username = "";
			string Password = "";
			string Namespace = "";
			string GenerationPath = "";
			string SkeletonPath = "";

			foreach (string arg in args)
			{
				if (string.IsNullOrEmpty(LastArg))
				{
					switch (arg.ToUpper())
					{
						case "/S":
						case "/D":
						case "/U":
						case "/P":
						case "/N":
						case "/GP":
						case "/SP":
							LastArg = arg;
							break;

						default:
							throw new ArgumentException(arg + " is an invalid argument.", arg);
					}
				}
				else
				{
					switch (LastArg)
					{
						case "/S":
							ServerName = arg;
							break;

						case "/D":
							DatabaseName = arg;
							break;

						case "/U":
							Username = arg;
							break;

						case "/P":
							Password = arg;
							break;

						case "/N":
							Namespace = arg;
							break;

						case "/GP":
							GenerationPath = arg;
							break;

						case "/SP":
							SkeletonPath = arg;
							break;

						default:
							throw new ApplicationException(LastArg + " is an unrecognized argument.");
					}

					LastArg = "";
				}
			}

			// Check that all required arguments were provided and they are valid.
			if (string.IsNullOrEmpty(ServerName))
			{
				throw new ArgumentException("The name of the SQL Server must be specified using the /S switch");
			}
			if (string.IsNullOrEmpty(DatabaseName))
			{
				throw new ArgumentException("The name of the database must be specified using /D switch");
			}
			if (string.IsNullOrEmpty(GenerationPath))
			{
				throw new ArgumentException("The generation path must be specified using /GP switch");
			}

			// Make sure that the specified folders exist.
			if (!Directory.Exists(GenerationPath))
			{
				throw new ArgumentException("The generation path " + GenerationPath + " does not exist.");
			}
			if (!string.IsNullOrEmpty(SkeletonPath) && !Directory.Exists(SkeletonPath))
			{
				throw new ArgumentException("The skeleton path " + SkeletonPath + " does not exist.");
			}

			// Use the database name if no namespace is specified.
			if (string.IsNullOrEmpty(Namespace))
			{
				Namespace = DatabaseName;
			}

			// Create server and database objects.
			Server objServer;
		    if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
		    {
		        objServer = new Server(ServerName);
		    }
		    else
		    {
		        objServer = new Server(ServerName, Username, Password);
		    }
		    Database objDatabase = objServer.Databases[DatabaseName];

			// Create the generators.
			CSharpGenerator objClassGenerator = new CSharpGenerator(GenerationPath, string.IsNullOrEmpty(SkeletonPath) ? GenerationPath : SkeletonPath, Namespace);
			SqlGenerator objSqlGenerator = new SqlGenerator(objDatabase);

			// Generate code for each of the tables in the database.
			foreach (Table objTable in objDatabase.Tables)
			{
				Console.WriteLine("Generating code for table " + objTable.Name + "...");

				// Check to see if this table should be ignored.
				if (objTable.Properties.Contains("codal_ignore"))
				{
					continue;
				}

				// Base class and collection class.
				objClassGenerator.GenerateBaseClass(objTable);
				objClassGenerator.GenerateCollectionClass(objTable);
				objClassGenerator.GenerateStubClass(objTable);

				// SQL insert, update and deletes to support the base class.
				objSqlGenerator.GenerateInsert(objTable);
				objSqlGenerator.GenerateUpdate(objTable);
				objSqlGenerator.GenerateDelete(objTable);

				// Enumeration column functions.
				objSqlGenerator.GenerateEnumerationFunctions(objTable);
			}

			// Generate the exception classes.
			Console.WriteLine("Generating exception classes...");
			objClassGenerator.GenerateValidationException();
			objClassGenerator.GenerateRecordNotFoundException();

			// Generate the IBusinessObject interface which all the business objects implement.
			Console.WriteLine("Generating IBusinessObject class...");
			objClassGenerator.GenerateIBusinessObject();

			// Generate the query wrapper class.
			Console.WriteLine("Generating query wrapper class...");
			objClassGenerator.GenerateQueryWrapperClass(objDatabase);
		}
	}
}
