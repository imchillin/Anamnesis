// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ModelViewModel : MemoryViewModelBase<Model>
	{
		public ModelViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public WeaponExtendedViewModel? Weapons { get; set; }
		[ModelField] public TransformViewModel? Transform { get; set; }
		[ModelField] public SkeletonWrapperViewModel? Skeleton { get; set; }
		[ModelField] public BustViewModel? Bust { get; set; }
		[ModelField] public float Height { get; set; }
		[ModelField] public float Wetness { get; set; }
		[ModelField] public float Drenched { get; set; }
		[ModelField] public short DataPath { get; set; }
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

		public bool IsPlayer => Enum.IsDefined(typeof(Model.DataPaths), this.DataPath);

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			if (viewModelProperty.Name == nameof(ModelViewModel.ExtendedAppearance))
			{
				// No extended appearance for anything that isn't a player!
				if (!this.IsPlayer)
				{
					if (this.ExtendedAppearance != null)
					{
						this.ExtendedAppearance.Dispose();
						this.ExtendedAppearance = null;
					}

					return false;
				}
			}

			return base.HandleModelToViewUpdate(viewModelProperty, modelField);
		}
	}
}
