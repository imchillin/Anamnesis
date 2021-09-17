// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using Anamnesis.Services;

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
			switch (GameDataService.Region)
			{
				case GameDataService.ClientRegion.Global: return typeof(TGlobal);
				case GameDataService.ClientRegion.Korean: return typeof(TKo);
				case GameDataService.ClientRegion.Chinese: return typeof(TCn);
			}

			throw new Exception($"Invalid client region: {GameDataService.Region}");
		}
	}
}
