// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Files;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearnceModeSelectorDialog.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AppearanceModeSelectorDialog : UserControl, IDialog<CharacterFile.SaveModes>
	{
		public AppearanceModeSelectorDialog()
		{
			this.InitializeComponent();
			this.DataContext = this;
			this.Result = CharacterFile.SaveModes.All;
		}

		public event DialogEvent? Close;

		public CharacterFile.SaveModes Result
		{
			get;
			set;
		}

		public bool EquipmentGear
		{
			get => this.Result.HasFlag(CharacterFile.SaveModes.EquipmentGear);
			set => this.Result = this.Result.SetFlag(CharacterFile.SaveModes.EquipmentGear, value);
		}

		public bool EquipmentAccessories
		{
			get => this.Result.HasFlag(CharacterFile.SaveModes.EquipmentAccessories);
			set => this.Result = this.Result.SetFlag(CharacterFile.SaveModes.EquipmentAccessories, value);
		}

		public bool EquipmentWeapons
		{
			get => this.Result.HasFlag(CharacterFile.SaveModes.EquipmentWeapons);
			set => this.Result = this.Result.SetFlag(CharacterFile.SaveModes.EquipmentWeapons, value);
		}

		public bool AppearanceHair
		{
			get => this.Result.HasFlag(CharacterFile.SaveModes.AppearanceHair);
			set => this.Result = this.Result.SetFlag(CharacterFile.SaveModes.AppearanceHair, value);
		}

		public bool AppearanceFace
		{
			get => this.Result.HasFlag(CharacterFile.SaveModes.AppearanceFace);
			set => this.Result = this.Result.SetFlag(CharacterFile.SaveModes.AppearanceFace, value);
		}

		public bool AppearanceBody
		{
			get => this.Result.HasFlag(CharacterFile.SaveModes.AppearanceBody);
			set => this.Result = this.Result.SetFlag(CharacterFile.SaveModes.AppearanceBody, value);
		}

		public void Cancel()
		{
			this.Result = CharacterFile.SaveModes.None;
			this.Close?.Invoke();
		}

		public void Confirm()
		{
			this.Close?.Invoke();
		}

		private void OnOKClick(object sender, RoutedEventArgs e)
		{
			this.Confirm();
		}

		private void OnCancelClick(object sender, RoutedEventArgs e)
		{
			this.Cancel();
		}
	}

	#pragma warning disable SA1204, SA1402
	public static class SaveModeExtensions
	{
		public static CharacterFile.SaveModes SetFlag(this CharacterFile.SaveModes a, CharacterFile.SaveModes b, bool enabled)
		{
			if (enabled)
			{
				return a | b;
			}
			else
			{
				return a & ~b;
			}
		}
	}
}
