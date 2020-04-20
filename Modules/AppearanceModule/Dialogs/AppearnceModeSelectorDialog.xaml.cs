// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.AppearanceModule.Files;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearnceModeSelectorDialog.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AppearanceModeSelectorDialog : UserControl, IDialog<AppearanceFile.SaveModes>
	{
		public AppearanceModeSelectorDialog()
		{
			this.InitializeComponent();
			this.DataContext = this;
			this.Result = AppearanceFile.SaveModes.All;
		}

		public event DialogEvent Close;

		public AppearanceFile.SaveModes Result
		{
			get;
			private set;
		}

		public bool EquipmentGear
		{
			get => this.Result.HasFlag(AppearanceFile.SaveModes.EquipmentGear);
			set => this.Result = this.Result.SetFlag(AppearanceFile.SaveModes.EquipmentGear, value);
		}

		public bool EquipmentAccessories
		{
			get => this.Result.HasFlag(AppearanceFile.SaveModes.EquipmentAccessories);
			set => this.Result = this.Result.SetFlag(AppearanceFile.SaveModes.EquipmentAccessories, value);
		}

		public bool AppearanceHair
		{
			get => this.Result.HasFlag(AppearanceFile.SaveModes.AppearanceHair);
			set => this.Result = this.Result.SetFlag(AppearanceFile.SaveModes.AppearanceHair, value);
		}

		public bool AppearanceFace
		{
			get => this.Result.HasFlag(AppearanceFile.SaveModes.AppearanceFace);
			set => this.Result = this.Result.SetFlag(AppearanceFile.SaveModes.AppearanceFace, value);
		}

		public bool AppearanceBody
		{
			get => this.Result.HasFlag(AppearanceFile.SaveModes.AppearanceBody);
			set => this.Result = this.Result.SetFlag(AppearanceFile.SaveModes.AppearanceBody, value);
		}

		private void OnOKClick(object sender, RoutedEventArgs e)
		{
			this.Close?.Invoke();
		}

		private void OnCancelClick(object sender, RoutedEventArgs e)
		{
			this.Result = AppearanceFile.SaveModes.None;
			this.Close?.Invoke();
		}
	}

	#pragma warning disable SA1204, SA1402
	public static class SaveModeExtensions
	{
		public static AppearanceFile.SaveModes SetFlag(this AppearanceFile.SaveModes a, AppearanceFile.SaveModes b, bool enabled)
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
