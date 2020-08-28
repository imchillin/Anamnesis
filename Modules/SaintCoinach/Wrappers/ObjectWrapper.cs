// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class ObjectWrapper : IDataObject
	{
		private XivRow row;

		public ObjectWrapper(XivRow row)
		{
			this.row = row;
		}

		public int Key
		{
			get
			{
				return this.row.Key;
			}
		}
	}
}
