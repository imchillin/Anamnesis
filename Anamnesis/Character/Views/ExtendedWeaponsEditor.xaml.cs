// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ExtendedWeaponsEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ExtendedWeaponsEditor : UserControl
	{
		public ExtendedWeaponsEditor()
		{
			this.InitializeComponent();
		}
	}
}
