// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Documents;
	using PropertyChanged;

	[StructLayout(LayoutKind.Explicit)]
	public struct Bones
	{
		////[FieldOffset(0x00)]
		////public IntPtr HkAnimationFile;

		[FieldOffset(0x10)]
		public int Count;

		[FieldOffset(0x18)]
		public IntPtr TransformArray;
	}

	public class BonesViewModel : MemoryViewModelBase<Bones>
	{
		public BonesViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public int Count { get; set; }
		[ModelField] public IntPtr TransformArray { get; set; }

		public List<TransformPtrViewModel> Transforms { get; set; } = new List<TransformPtrViewModel>();

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			bool changed = base.HandleModelToViewUpdate(viewModelProperty, modelField);

			if (viewModelProperty.Name == nameof(BonesViewModel.TransformArray) && this.TransformArray != IntPtr.Zero)
			{
				// If we think there are more than 512 bones its likely that
				// this is actually an invalid pointer that we got from memory.
				if (this.Count > 512)
					return changed;

				lock (this.Transforms)
				{
					if (this.Count <= 0)
					{
						this.ClearTransforms();
					}
					else
					{
						if (this.Transforms.Count != this.Count)
						{
							// new transforms
							this.ClearTransforms();

							IntPtr ptr = this.TransformArray;
							for (int i = 0; i < this.Count; i++)
							{
								this.Transforms.Add(new TransformPtrViewModel(ptr, this));
								ptr += 0x30;
							}
						}
						else
						{
							// Update pointers
							IntPtr ptr = this.TransformArray;
							for (int i = 0; i < this.Transforms.Count; i++)
							{
								this.Transforms[i].Pointer = ptr;
								ptr += 0x30;
							}
						}
					}
				}
			}

			return changed;
		}

		private void ClearTransforms()
		{
			foreach (TransformPtrViewModel transform in this.Transforms)
			{
				transform.Dispose();
			}

			this.Transforms.Clear();
		}
	}
}
