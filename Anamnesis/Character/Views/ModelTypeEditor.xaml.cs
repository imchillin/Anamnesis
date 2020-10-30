// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for ModelTypeEditor.xaml.
	/// </summary>
	public partial class ModelTypeEditor : UserControl
	{
		public ModelTypeEditor()
		{
			this.InitializeComponent();
		}

		private void OnBrowseClicked(object sender, RoutedEventArgs e)
		{
			if (GameDataService.ModelTypes == null)
				return;

			ActorViewModel? vm = this.DataContext as ActorViewModel;

			if (vm == null)
				return;

			ModelType? selected = null;

			if (GameDataService.ModelTypes.Contains(vm.ModelType))
				selected = GameDataService.ModelTypes.Get(vm.ModelType);

			SelectorDrawer.Show<ModelTypeSelector, ModelType>("Model Type", selected, (v) => { vm.ModelType = v.Key; });
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.GetModelName();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ActorViewModel? actorVm = this.DataContext as ActorViewModel;

			if (actorVm == null)
				return;

			actorVm.PropertyChanged += this.OnActorVmPropertyChanged;
		}

		private void OnActorVmPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ActorViewModel.ModelType))
			{
				this.GetModelName();
			}
		}

		private void GetModelName()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				ActorViewModel? actorVm = this.DataContext as ActorViewModel;

				if (actorVm == null)
					return;

				int modelTypeId = actorVm.ModelType;

				this.ModelName.Text = null;

				if (GameDataService.ModelTypes!.Contains(actorVm.ModelType))
				{
					ModelType entry = GameDataService.ModelTypes.Get(actorVm.ModelType);
					this.ModelName.Text = entry.Name;
				}
			});
		}
	}
}
