// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;

	public class BonesMemory : MemoryBase
	{
		////[Bind(0x000, BindFlags.Pointer)] public IntPtr HkAnimationFile;
		[Bind(0x010)] public int Count { get; set; }
		////[Bind(0x018, BindFlags.Pointer)] public IntPtr TransformArray { get; set; }

		public List<TransformMemory> Transforms { get; set; } = new List<TransformMemory>();

		/*protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
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
		}*/

		private void ClearTransforms()
		{
			foreach (TransformMemory transform in this.Transforms)
			{
				transform.Dispose();
			}

			this.Transforms.Clear();
		}
	}
}
