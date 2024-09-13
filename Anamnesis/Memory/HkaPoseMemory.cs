// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor;
using Anamnesis.Services;

/// <summary>
/// Represents the memory structure for a Havok pose in FFXIV.
/// </summary>
public class HkaPoseMemory : MemoryBase
{
	/// <summary>Gets or sets the skeleton memory of the Havok pose.</summary>
	[Bind(0x000, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public HkaSkeletonMemory? Skeleton { get; set; }

	/// <summary>Gets or sets the transform array memory of the Havok pose.</summary>
	[Bind(0x018, BindFlags.DontCacheOffsets)] public TransformArrayMemory? Transforms { get; set; }

	/// <summary>
	/// Handles pose memory property changes.
	/// </summary>
	/// <param name="change">The property change information.</param>
	/// <remarks>The method updates the bones' change history.</remarks>
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
}

/// <summary>Represents an array of transform memories.</summary>
public class TransformArrayMemory : ArrayMemory<TransformMemory, int>
{
	public override int AddressOffset => 0x000;
	public override int LengthOffset => 0x008;
	public override int ElementSize => 0x030;
}
