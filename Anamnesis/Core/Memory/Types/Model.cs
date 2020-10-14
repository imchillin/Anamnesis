// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Model
	{
		[FieldOffset(0x050)] public Transform Transform;
		[FieldOffset(0x0A0)] public IntPtr Skeleton;
		[FieldOffset(0x148)] public IntPtr Bust;
		[FieldOffset(0x240)] public IntPtr ExtendedAppearance;
		[FieldOffset(0x26C)] public float Height;
		[FieldOffset(0x2B0)] public float Wetness;
		[FieldOffset(0x2BC)] public float Drenched;
		[FieldOffset(0x938)] public DataPaths DataPath;
		[FieldOffset(0x93C)] public byte DataHead;
	}

	public class ModelViewModel : MemoryViewModelBase<Model>
	{
		public ModelViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public ModelViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public TransformViewModel? Transform { get; set; }
		[ModelField] public SkeletonWrapperViewModel? Skeleton { get; set; }
		[ModelField] public float Height { get; set; }
		[ModelField] public float Wetness { get; set; }
		[ModelField] public float Drenched { get; set; }
		[ModelField] public DataPaths DataPath { get; set; }
		[ModelField] public byte DataHead { get; set; }
		[ModelField(0x28, 0x20)] public ExtendedAppearanceViewModel? ExtendedAppearance { get; set; }

		public bool LockWetness
		{
			get => this.IsValueFrozen(nameof(ModelViewModel.Wetness));
			set => this.FreezeValue(nameof(ModelViewModel.Wetness), value);
		}

		public bool ForceDrenched
		{
			get => this.IsValueFrozen(nameof(ModelViewModel.Drenched));
			set => this.FreezeValue(nameof(ModelViewModel.Drenched), value, value ? 5 : 0);
		}

		public DataPaths DataPathAndHead
		{
			get
			{
				return this.DataPath;
			}

			set
			{
				this.DataPath = value;

				if (this.Parent is ActorViewModel vm && vm.Customize != null)
				{
					this.DataHead = value.GetHead(vm.Customize.Tribe);
				}
			}
		}
	}
}
