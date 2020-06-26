// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media.Animation;
	using Anamnesis;
	using PropertyChanged;
	using Styles.Drawers;

	/// <summary>
	/// Interaction logic for ModelTypeEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ModelTypeEditor : UserControl
	{
		private Actor actor;

		private IMemory<int> modelTypeMem;

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

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.modelTypeMem?.Dispose();

			this.actor = this.DataContext as Actor;

			Application.Current.Dispatcher.Invoke(() => { this.IsEnabled = this.actor != null; });

			if (this.actor == null)
				return;

			this.modelTypeMem = this.actor.GetMemory(Offsets.Main.ModelType);
			this.modelTypeMem.ValueChanged += this.ModelTypeMem_ValueChanged;
			this.ModelTypeMem_ValueChanged(null, null);
		}

		private void ModelTypeMem_ValueChanged(object sender = null, object value = null)
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

			List<GenericSelector.Item> items = new List<GenericSelector.Item>();
			GenericSelector.Item current = null;

			foreach (ModelTypes model in Module.ModelTypes)
			{
				GenericSelector.Item item = new GenericSelector.Item(model.Name, model.Id);

				if (model.Id == this.ModelType)
					current = item;

				items.Add(item);
			}

			GenericSelector.Show("Model Type", current, items, (i) => { this.ModelType = (int)i.Data; });
		}
	}
}
