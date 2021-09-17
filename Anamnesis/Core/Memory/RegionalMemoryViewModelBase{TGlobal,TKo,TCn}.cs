// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public abstract class RegionalMemoryViewModelBase<TGlobal, TKo, TCn> : MemoryViewModelBase
		where TGlobal : struct
		where TKo : struct
		where TCn : struct
	{
		protected RegionalMemoryViewModelBase(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		public override Type GetModelType()
		{
			return typeof(TGlobal);
		}
	}
}
