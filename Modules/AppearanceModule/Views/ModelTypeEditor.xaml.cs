// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media.Animation;
	using ConceptMatrix.Memory;
	using ConceptMatrix.WpfStyles.Drawers;
	using PropertyChanged;
	using Styles.Drawers;

	/// <summary>
	/// Interaction logic for ModelTypeEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ModelTypeEditor : UserControl
	{
		private Actor actor;

		private IMarshaler<int> modelTypeMem;

		public ModelTypeEditor()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			ISelectionService selectionService = Services.Get<ISelectionService>();
			selectionService.ModeChanged += this.SelectionService_ModeChanged;
		}

		public int ModelType
		{
			get
			{
				if (this.modelTypeMem == null || !this.modelTypeMem.Active)
					return 0;

				return this.modelTypeMem.Value;
			}
			set
			{
				if (this.modelTypeMem.Value != value)
				{
					this.modelTypeMem.Value = value;
					this.actor?.ActorRefresh();
				}
			}
		}

		[PropertyChanged.SuppressPropertyChangedWarnings]
		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.modelTypeMem?.Dispose();

			this.actor = this.DataContext as Actor;

			Application.Current.Dispatcher.Invoke(() => { this.IsEnabled = this.actor != null; });

			if (this.actor == null)
				return;

			this.modelTypeMem = this.actor.GetMemory(Offsets.Main.ModelType);
			this.modelTypeMem.ValueChanged += this.ModelTypeMem_ValueChanged;
			this.ModelTypeMem_ValueChanged(null, 0);
		}

		private void ModelTypeMem_ValueChanged(object sender = null, int value = 0)
		{
			this.ModelType = this.modelTypeMem.Value;
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

			ModelTypes selected = null;
			foreach (ModelTypes modelType in Module.ModelTypes)
			{
				if (modelType.Id == this.ModelType)
				{
					selected = modelType;
				}
			}

			SelectorDrawer.Show<ModelTypeSelector, ModelTypes>("Model Type", selected, (v) => { this.ModelType = v.Id; });
		}
	}
}
