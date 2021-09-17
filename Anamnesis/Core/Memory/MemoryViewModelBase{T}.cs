// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public abstract class MemoryViewModelBase<T> : MemoryViewModelBase
		where T : struct
	{
		protected MemoryViewModelBase(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		public T Model => (T)this.model;

		public override Type GetModelType()
		{
			return typeof(T);
		}
	}
}
