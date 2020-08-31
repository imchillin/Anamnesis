// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.AppearanceModule.Files;
	using Anamnesis.Services;
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

		public void Cancel()
		{
			this.Result = AppearanceFile.SaveModes.None;
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
