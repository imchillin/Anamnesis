// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor;
using Anamnesis.Services;

public class HkaPoseMemory : MemoryBase
{
	[Bind(0x000, BindFlags.Pointer)] public HkaSkeletonMemory? Skeleton { get; set; }
	[Bind(0x010)] public TransformArrayMemory? Transforms { get; set; }

	protected override void HandlePropertyChanged(PropertyChange change)
	{
		// Big hack to keep bone change history names short.
		if (change.Origin == PropertyChange.Origins.User && change.TopPropertyName == nameof(this.Transforms))
		{
			if (PoseService.SelectedBoneName == null)
			{
				change.Name = LocalizationService.GetStringFormatted("History_ChangeBone", "??");
			}
			else
			{
				change.Name = LocalizationService.GetStringFormatted("History_ChangeBone", PoseService.SelectedBoneName);
			}
		}

		base.HandlePropertyChanged(change);
	}

	public class TransformArrayMemory : ArrayMemory<TransformMemory, int>
	{
		public override int CountOffset => 0x000;
		public override int AddressOffset => 0x008;
		public override int ElementSize => 0x030;
	}
}
