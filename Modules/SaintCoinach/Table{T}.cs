// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class Table<T> : IData<T>
		where T : IDataObject
	{
		private Dictionary<int, T> data = new Dictionary<int, T>();

		public IEnumerable<T> All
		{
			get
			{
				return this.data.Values;
			}
		}

		public int Count
		{
			get
			{
				return this.data.Count;
			}
		}

		public T Get(int key)
		{
			if (this.data.ContainsKey(key))
				return this.data[key];

			return default;
		}

		public T Get(byte key)
		{
			return this.Get((int)key);
		}

		internal void Import<TRow, TWrapper>(IXivSheet<TRow> sheet)
			where TRow : XivRow
			where TWrapper : ObjectWrapper, T
		{
			try
			{
				Type type = typeof(TWrapper);
				foreach (TRow row in sheet)
				{
					if (row.Key == 0)
						continue;

					TWrapper wrapper = (TWrapper)Activator.CreateInstance(type, row);
					this.data.Add(wrapper.Key, wrapper);
				}
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}
