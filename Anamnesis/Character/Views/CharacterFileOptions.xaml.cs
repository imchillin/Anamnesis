// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
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
	public partial class CharacterFileOptions : UserControl
	{
		public static CharacterFile.SaveModes Result;

		public CharacterFileOptions()
		{
			this.InitializeComponent();
			this.DataContext = this;
		}

		public event DialogEvent? Close;

		public bool EquipmentGear
		{
			get => Result.HasFlag(CharacterFile.SaveModes.EquipmentGear);
			set => Result = Result.SetFlag(CharacterFile.SaveModes.EquipmentGear, value);
		}

		public bool EquipmentAccessories
		{
			get => Result.HasFlag(CharacterFile.SaveModes.EquipmentAccessories);
			set => Result = Result.SetFlag(CharacterFile.SaveModes.EquipmentAccessories, value);
		}

		public bool EquipmentWeapons
		{
			get => Result.HasFlag(CharacterFile.SaveModes.EquipmentWeapons);
			set => Result = Result.SetFlag(CharacterFile.SaveModes.EquipmentWeapons, value);
		}

		public bool AppearanceHair
		{
			get => Result.HasFlag(CharacterFile.SaveModes.AppearanceHair);
			set => Result = Result.SetFlag(CharacterFile.SaveModes.AppearanceHair, value);
		}

		public bool AppearanceFace
		{
			get => Result.HasFlag(CharacterFile.SaveModes.AppearanceFace);
			set => Result = Result.SetFlag(CharacterFile.SaveModes.AppearanceFace, value);
		}

		public bool AppearanceBody
		{
			get => Result.HasFlag(CharacterFile.SaveModes.AppearanceBody);
			set => Result = Result.SetFlag(CharacterFile.SaveModes.AppearanceBody, value);
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
