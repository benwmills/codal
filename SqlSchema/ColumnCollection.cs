using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace MillsSoftware.SqlSchema
{
	public class ColumnCollection : KeyedCollection<string, Column>
	{
        protected override string GetKeyForItem(Column column)
		{
            return column.Name;
		}
	}
}