// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Windows;
using System.Windows.Controls;
using Anamnesis.Memory;

/// <summary>
/// Interaction logic for CharacterModelEditor.xaml.
/// </summary>
public partial class CharacterModelEditor : UserControl
{
	private WetMode wetMode = WetMode.Off;

	public CharacterModelEditor()
	{
		this.InitializeComponent();
	}

	public enum WetMode
	{
		Off,
		Wet,
		Drenched,
	}

	public ActorModelMemory? Model => (this.DataContext as ActorMemory)?.ModelObject;

	private void OnModeToggleClicked(object sender, RoutedEventArgs e)
	{
		this.ModeToggle.IsChecked = false;

		if (this.Model == null)
			return;

		switch (this.wetMode)
		{
			case WetMode.Off:
				{
					this.wetMode = WetMode.Wet;
					this.ModeIcon.Icon = FontAwesome.Sharp.IconChar.Tint;
					this.Model.Wetness = 1;
					this.Model.LockWetness = true;
					this.ModeTooltip.Key = "Character_Model_WetWet";
					break;
				}

			case WetMode.Wet:
				{
					this.wetMode = WetMode.Drenched;
					this.ModeIcon.Icon = FontAwesome.Sharp.IconChar.Water;
					this.Model.Drenched = 1;
					this.Model.ForceDrenched = true;
					this.ModeTooltip.Key = "Character_Model_WetDrenched";
					break;
				}

			case WetMode.Drenched:
				{
					this.wetMode = WetMode.Off;
					this.ModeIcon.Icon = FontAwesome.Sharp.IconChar.TintSlash;
					this.Model.Wetness = 0;
					this.Model.LockWetness = false;
					this.Model.Drenched = 0;
					this.Model.ForceDrenched = false;
					this.ModeTooltip.Key = "Character_Model_WetNone";
					break;
				}
		}
	}
}
