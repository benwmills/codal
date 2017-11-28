using System;
using System.Collections.ObjectModel;

namespace MillsSoftware.SqlSchema
{
	public class TableCollection : KeyedCollection<string, Table>
	{
		protected override string GetKeyForItem(Table table)
		{
            return table.Name;
		}
	}
}
