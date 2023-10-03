// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Services;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class AnimationMemory : MemoryBase
{
	public const int AnimationSlotCount = 13;

	private bool linkSpeeds = true;

	public enum AnimationSlots : int
	{
		FullBody = 0,
		UpperBody = 1,
		Facial = 2,
		Add = 3,
		Lips = 7,
		Parts1 = 8,
		Parts2 = 9,
		Parts3 = 10,
		Parts4 = 11,
		Overlay = 12,
	}

	[Bind(0x0E0)] public AnimationIdArrayMemory? AnimationIds { get; set; }
	[Bind(0x154)] public AnimationSpeedArrayMemory? Speeds { get; set; }
	[Bind(0x1E2)] public byte SpeedTrigger { get; set; }
	[Bind(0x2D0)] public ushort BaseOverride { get; set; }
	[Bind(0x2D2)] public ushort LipsOverride { get; set; }

	public bool BlendLocked { get; set; } = false;

	public bool LinkSpeeds
	{
		get
		{
			return this.linkSpeeds;
		}
		set
		{
			this.linkSpeeds = value;
			this.ApplyAnimationLink(this.Speeds![0].Value);
		}
	}

	protected override void HandlePropertyChanged(PropertyChange change)
	{
		// Update all speeds if linked
		if (this.LinkSpeeds && AnimationService.Instance.SpeedControlEnabled && change.Origin == PropertyChange.Origins.User && change.TopPropertyName == nameof(this.Speeds) && change.BindPath[1].Name == "0")
		{
			this.ApplyAnimationLink((float)change.NewValue!);
		}

		// We need to flip a flag (the engine will flip it back) to apply it
		if (AnimationService.Instance.SpeedControlEnabled && change.Origin == PropertyChange.Origins.User && change.TopPropertyName == nameof(this.Speeds))
		{
			this.SpeedTrigger = 1;
		}

		base.HandlePropertyChanged(change);
	}

	private void ApplyAnimationLink(float value)
	{
		for (int i = 1; i < AnimationSlotCount; i++)
		{
			this.Speeds![i].Value = value;
		}
	}

	public class AnimationIdArrayMemory : InplaceFixedArrayMemory<ValueMemory<ushort>>
	{
		public override int ElementSize => sizeof(ushort);
		public override int Count => AnimationSlotCount;
	}

	public class AnimationSpeedArrayMemory : InplaceFixedArrayMemory<ValueMemory<float>>
	{
		public override int ElementSize => sizeof(float);
		public override int Count => AnimationSlotCount;
	}
}
