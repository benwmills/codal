using System;
using System.Collections.ObjectModel;

namespace MillsSoftware.SqlSchema
{
	public class ColumnPropertyCollection : KeyedCollection<string, ColumnProperty>
	{
        protected override string GetKeyForItem(ColumnProperty columnProperty)
		{
            return columnProperty.Name;
		}
	}
}
