// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.ComponentModel;

	public interface IStructViewModel : INotifyPropertyChanged
	{
		IStructViewModel? Parent { get; }
		bool Enabled { get; set; }
		Type GetModelType();
		void SetModel(object? model);
		object? GetModel();

		TParent? GetParent<TParent>()
			where TParent : IStructViewModel;

		void RaisePropertyChanged(string propertyName);
	}
}
