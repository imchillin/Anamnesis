// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using SaintCoinach.Xiv;

	internal class ObjectWrapper<T> : ObjectWrapper
		where T : XivRow
	{
		protected readonly T Value;

		public ObjectWrapper(T value)
			: base(value)
		{
			this.Value = value;
		}
	}
}
