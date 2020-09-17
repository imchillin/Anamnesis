// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for ModelTypeEditor.xaml.
	/// </summary>
	public partial class ModelTypeEditor : UserControl
	{
		public ModelTypeEditor()
		{
			this.InitializeComponent();

			TargetService.ModeChanged += this.SelectionService_ModeChanged;
		}

		private void SelectionService_ModeChanged(Modes mode)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = mode == Modes.Overworld;
			});
		}

		private void OnBrowseClicked(object sender, RoutedEventArgs e)
		{
			if (Module.ModelTypes == null)
				return;

			ActorViewModel vm = this.DataContext as ActorViewModel;

			ModelTypes selected = null;
			foreach (ModelTypes modelType in Module.ModelTypes)
			{
				if (modelType.Id == vm.ModelType)
				{
					selected = modelType;
				}
			}

			SelectorDrawer.Show<ModelTypeSelector, ModelTypes>("Model Type", selected, (v) => { vm.ModelType = v.Id; });
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.GetModelName();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ActorViewModel actorVm = this.DataContext as ActorViewModel;

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
				ActorViewModel actorVm = this.DataContext as ActorViewModel;
				int modelTypeId = actorVm.ModelType;

				this.ModelName.Text = null;

				foreach (ModelTypes modelType in Module.ModelTypes)
				{
					if (modelType.Id == modelTypeId)
					{
						this.ModelName.Text = modelType.Name;
					}
				}
			});
		}
	}
}
