// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
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

		public event DialogEvent Close;

		public PoseFile.Configuration Result
		{
			get;
			private set;
		}

		public bool Hair
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Hair) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Hair, value);
		}

		public bool Face
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Face) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Face, value);
		}

		public bool Torso
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Torso) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Torso, value);
		}

		public bool LeftArm
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.LeftArm) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.LeftArm, value);
		}

		public bool RightArm
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.RightArm) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.RightArm, value);
		}

		public bool LeftHand
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.LeftHand) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.LeftHand, value);
		}

		public bool RightHand
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.RightHand) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.RightHand, value);
		}

		public bool LeftLeg
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.LeftLeg) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.LeftLeg, value);
		}

		public bool RightLeg
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.RightLeg) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.RightLeg, value);
		}

		public bool Clothes
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Clothes) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Clothes, value);
		}

		public bool Equipment
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Equipment) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Equipment, value);
		}

		public bool Tail
		{
			get => this.Result?.Groups.HasFlag(PoseFile.Groups.Tail) ?? false;
			set => this.Result.Groups = this.Result.Groups.SetFlag(PoseFile.Groups.Tail, value);
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
