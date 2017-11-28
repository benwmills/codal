using System;
using System.Collections.ObjectModel;

namespace MillsSoftware.SqlSchema
{
	public class DatabaseCollection : KeyedCollection<string, Database>
	{
        protected override string GetKeyForItem(Database database)
		{
            return database.Name;
		}
	}
}
