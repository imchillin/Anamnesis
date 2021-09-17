// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public abstract class StructViewModelBase<T> : StructViewModelBase
		where T : struct
	{
		protected StructViewModelBase()
		{
		}

		protected StructViewModelBase(IStructViewModel? parent)
			: base(parent)
		{
		}

		protected StructViewModelBase(IStructViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		public T Model
		{
			get
			{
				if (this.model == null)
					throw new Exception("Struct View Modal has no modal");

				return (T)this.model;
			}
		}

		public override Type GetModelType()
		{
			return typeof(T);
		}

		public override void Import(object model)
		{
			if (model is T tModel)
			{
				base.Import(tModel);
			}
			else
			{
				throw new Exception($"Invalid model type. Expected: {typeof(T)}, got: {model?.GetType()}");
			}
		}

		public override void SetModel(object? model)
		{
			if (model is T tModel)
			{
				base.SetModel(tModel);
			}
			else
			{
				throw new Exception($"Invalid model type. Expected: {typeof(T)}, got: {model?.GetType()}");
			}
		}
	}
}
