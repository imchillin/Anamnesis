// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;

	public class ArrayMemory<T> : MemoryBase, IEnumerable<T>
		where T : MemoryBase
	{
		private readonly List<T> items = new();

		[Bind(0x000)] public int Count { get; protected set; }
		[Bind(0x008, BindFlags.Pointer)] public IntPtr ArrayAddress { get; protected set; }

		public T this[int index]
		{
			get => this.items[index];
		}

		public IEnumerator<T> GetEnumerator() => this.items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();

		protected void UpdateArray()
		{
			lock (this.items)
			{
				foreach (T item in this.items)
				{
					item.Dispose();
				}

				this.items.Clear();

				if (this.ArrayAddress == IntPtr.Zero || this.Count <= 0)
					return;

				IntPtr address = this.ArrayAddress;
				for (int i = 0; i < this.Count; i++)
				{
					T instance = Activator.CreateInstance<T>();
					instance.SetAddress(address);
					this.items.Add(instance);

					address += instance.Size;
				}
			}
		}

		protected override void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			base.OnSelfPropertyChanged(sender, e);

			if (e.PropertyName != nameof(this.Count) && e.PropertyName != nameof(this.ArrayAddress))
				return;

			if (!this.IsReading)
				throw new Exception("Array properties should only change while reading memory");

			this.UpdateArray();
		}

		////[Bind(0x010)] public int Count { get; set; }
		////[Bind(0x018, BindFlags.Pointer)] public IntPtr TransformArray { get; set; }

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
	}
}
