// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearnceModeSelectorDialog.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class BoneGroupsSelectorDialog : UserControl, IDialog<PoseFile.Configuration>
	{
		public BoneGroupsSelectorDialog()
		{
			this.InitializeComponent();
			this.DataContext = this;
			this.Result = new PoseFile.Configuration();
		}

		public event DialogEvent? Close;

		public PoseFile.Configuration Result
		{
			get;
			private set;
		}

		public bool Body
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Body) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Body, value);
		}

		public bool Head
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Head) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Head, value);
		}

		public bool Hair
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Hair) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Hair, value);
		}

		public bool Met
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Met) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Met, value);
		}

		public bool Top
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Top) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Top, value);
		}

		public void Cancel()
		{
			this.Close?.Invoke();
		}

		public void Confirm()
		{
			this.Close?.Invoke();
		}

		private void OnAllClick(object sender, RoutedEventArgs e)
		{
			this.Result.Groups = PoseFile.Groups.All;
		}

		private void OnNoneClick(object sender, RoutedEventArgs e)
		{
			this.Result.Groups = PoseFile.Groups.None;
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
	public static class GroupsExtensions
	{
		public static PoseFile.Groups SetFlag(this PoseFile.Groups a, PoseFile.Groups b, bool enabled)
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
