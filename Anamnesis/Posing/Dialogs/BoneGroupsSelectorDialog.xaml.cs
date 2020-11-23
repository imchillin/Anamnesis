// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Files;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearnceModeSelectorDialog.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class BoneGroupsSelectorDialog : UserControl, IDialog<PoseFile.Configuration?>
	{
		public BoneGroupsSelectorDialog()
		{
			this.InitializeComponent();
			this.DataContext = this;
		}

		public event DialogEvent? Close;

		public bool UseSelection { get; set; }

		public PoseFile.Configuration? Result
		{
			get;
			set;
		}

		public void Cancel()
		{
			this.Result = null;
			this.Close?.Invoke();
		}

		public void Confirm()
		{
			this.Close?.Invoke();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (this.Result == null)
				this.Result = new PoseFile.Configuration();

			this.UseSelection = this.Result.UseSelection;
		}

		private void OnOKClick(object sender, RoutedEventArgs e)
		{
			this.Result!.UseSelection = this.UseSelection;
			this.Confirm();
		}

		private void OnCancelClick(object sender, RoutedEventArgs e)
		{
			this.Cancel();
		}
	}
}
