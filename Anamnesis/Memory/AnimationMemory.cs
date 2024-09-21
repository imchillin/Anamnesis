// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Services;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class AnimationMemory : MemoryBase
{
	public const int AnimationSlotCount = 14;

	private bool linkSpeeds = true;

	public AnimationMemory()
	{
		this.PropertyChanged += this.HandlePropertyChanged;
	}

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
	[Bind(0x2E0)] public ushort BaseOverride { get; set; }
	[Bind(0x2E2)] public ushort LipsOverride { get; set; }

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
			this.ApplyAnimationLink(this.Speeds![0]);
		}
	}

	private void HandlePropertyChanged(object? sender, MemObjPropertyChangedEventArgs e)
	{
		// Update all speeds if linked
		if (this.LinkSpeeds && AnimationService.Instance.SpeedControlEnabled &&
			e.Context.Origin == PropertyChange.Origins.User &&
			e.Context.TopPropertyName == nameof(this.Speeds) &&
			e.Context.BindPath[1].Name == "0" &&
			e.Context.OriginBind.LastValue != null)
		{
			this.ApplyAnimationLink((float)e.Context.OriginBind.LastValue);
		}

		// We need to flip a flag (the engine will flip it back) to apply it
		if (AnimationService.Instance.SpeedControlEnabled &&
			e.Context.Origin == PropertyChange.Origins.User &&
			e.Context.TopPropertyName == nameof(this.Speeds))
		{
			this.SpeedTrigger = 1;
		}
	}

	private void ApplyAnimationLink(float value)
	{
		for (int i = 1; i < AnimationSlotCount; i++)
		{
			this.Speeds![i] = value;
		}
	}

	public class AnimationIdArrayMemory : InplaceFixedArrayMemory<ushort>
	{
		public override int ElementSize => sizeof(ushort);
		public override int Length => AnimationSlotCount;
	}

	public class AnimationSpeedArrayMemory : InplaceFixedArrayMemory<float>
	{
		public override int ElementSize => sizeof(float);
		public override int Length => AnimationSlotCount;
	}
}
