// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	/// <summary>
	/// Indicates that a property should be bound to a backing view model.
	/// Only functional in classes that inherit from <see cref="StructViewModelBase{T}"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ModelFieldAttribute : Attribute
	{
		public readonly int[]? Offsets;
		public readonly string? FieldName;

		public ModelFieldAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModelFieldAttribute"/> class that binds to a field with a custom name.
		/// </summary>
		public ModelFieldAttribute(string fieldName)
		{
			this.FieldName = fieldName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModelFieldAttribute"/> class that binds to an <see cref="IntPtr"/> address with a custom offset pointer path.
		/// </summary>
		public ModelFieldAttribute(params int[] offsets)
		{
			this.Offsets = offsets;
			this.FieldName = null;
		}
	}
}
