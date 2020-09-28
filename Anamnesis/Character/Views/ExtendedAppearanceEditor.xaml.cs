// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using PropertyChanged;

	using Color = Anamnesis.Memory.Color;
	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ExtendedAppearanceEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class ExtendedAppearanceEditor : UserControl
	{
		public ExtendedAppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public ExtendedAppearanceViewModel? ExtendedAppearance { get; set; }

		public Color MainHandTint { get; set; }
		public Vector MainHandScale { get; set; }
		public Color OffHandTint { get; set; }
		public Vector OffHandScale { get; set; }
		public float Transparency { get; set; }
		public Vector BustScale { get; set; }
		public float FeatureScale { get; set; }

		private void Button_Click(object sender, RoutedEventArgs e)
		{
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ActorViewModel? vm = this.DataContext as ActorViewModel;
			if (vm == null)
				return;

			this.ExtendedAppearance = vm.ModelObject?.ExtendedAppearance;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnDataContextChanged(sender, default);
		}
	}
}
