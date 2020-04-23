// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Pages
{
	using System;
	using System.Windows.Controls;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AdvancedActorPage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AdvancedActorPage : UserControl
	{
		public AdvancedActorPage()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.ActorTypeComboBox.ItemsSource = Enum.GetValues(typeof(ActorTypes));

			ISelectionService selectionService = ConceptMatrix.Services.Get<ISelectionService>();
			selectionService.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selectionService.CurrentSelection);
		}

		public ActorTypes ActorType { get; set; }

		private void OnSelectionChanged(Selection selection)
		{
			selection.BaseAddress.Bind(Offsets.Main.ActorType, this, nameof(this.ActorType));
		}
	}
}
