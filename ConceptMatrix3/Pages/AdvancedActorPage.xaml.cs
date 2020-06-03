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
		}

		public ActorTypes ActorType { get; set; }

		private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			this.SetActor(this.DataContext as Actor);
		}

		private void SetActor(Actor actor)
		{
			BindUtility.ClearAll(this);

			if (actor == null)
				return;

			IMemory<ActorTypes> memory = actor.GetMemory(Offsets.Main.ActorType);
			memory.Bind(this, nameof(this.ActorType));
		}
	}
}
