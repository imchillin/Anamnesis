// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Pages
{
	using System;
	using System.Windows.Controls;
	using ConceptMatrix.MemoryBinds;
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

			this.OnSelectionChanged(this.DataContext as Actor);
		}

		public ActorTypes ActorType { get; set; }

		[SuppressPropertyChangedWarnings]
		private void OnSelectionChanged(Actor selection)
		{
			BindUtility.ClearAll(this);

			if (selection == null)
				return;

			selection.BaseAddress.Bind(Offsets.Main.ActorType, this, nameof(this.ActorType));
		}
	}
}
