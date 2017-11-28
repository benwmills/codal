using System;
using System.Collections.ObjectModel;

namespace MillsSoftware.SqlSchema
{
	public class TablePropertyCollection : KeyedCollection<string, TableProperty>
	{
		protected override string GetKeyForItem(TableProperty tableProperty)
		{
            return tableProperty.Name;
		}
	}
}
