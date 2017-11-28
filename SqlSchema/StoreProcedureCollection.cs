using System;
using System.Collections.ObjectModel;

namespace MillsSoftware.SqlSchema
{
	public class StoreProcedureCollection : KeyedCollection<string, StoredProcedure>
	{
        protected override string GetKeyForItem(StoredProcedure storedProcedure)
		{
            return storedProcedure.Name;
		}
	}
}
