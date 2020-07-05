// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Dialogs
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearnceModeSelectorDialog.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class BoneGroupsSelectorDialog : UserControl, IDialog<PoseFile.Groups>
	{
		public BoneGroupsSelectorDialog()
		{
			this.InitializeComponent();
			this.DataContext = this;
			this.Result = PoseFile.Groups.All;
		}

		public event DialogEvent Close;

		public PoseFile.Groups Result
		{
			get;
			private set;
		}

		public bool Hair
		{
			get => this.Result.HasFlag(PoseFile.Groups.Hair);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.Hair, value);
		}

		public bool Face
		{
			get => this.Result.HasFlag(PoseFile.Groups.Face);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.Face, value);
		}

		public bool Torso
		{
			get => this.Result.HasFlag(PoseFile.Groups.Torso);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.Torso, value);
		}

		public bool LeftArm
		{
			get => this.Result.HasFlag(PoseFile.Groups.LeftArm);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.LeftArm, value);
		}

		public bool RightArm
		{
			get => this.Result.HasFlag(PoseFile.Groups.RightArm);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.RightArm, value);
		}

		public bool LeftHand
		{
			get => this.Result.HasFlag(PoseFile.Groups.LeftHand);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.LeftHand, value);
		}

		public bool RightHand
		{
			get => this.Result.HasFlag(PoseFile.Groups.RightHand);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.RightHand, value);
		}

		public bool LeftLeg
		{
			get => this.Result.HasFlag(PoseFile.Groups.LeftLeg);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.LeftLeg, value);
		}

		public bool RightLeg
		{
			get => this.Result.HasFlag(PoseFile.Groups.RightLeg);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.RightLeg, value);
		}

		public bool Clothes
		{
			get => this.Result.HasFlag(PoseFile.Groups.Clothes);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.Clothes, value);
		}

		public bool Equipment
		{
			get => this.Result.HasFlag(PoseFile.Groups.Equipment);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.Equipment, value);
		}

		public bool Tail
		{
			get => this.Result.HasFlag(PoseFile.Groups.Tail);
			set => this.Result = this.Result.SetFlag(PoseFile.Groups.Tail, value);
		}

		public void Cancel()
		{
			this.Result = PoseFile.Groups.None;
			this.Close?.Invoke();
		}

		public void Confirm()
		{
			this.Close?.Invoke();
		}

		private void OnAllClick(object sender, RoutedEventArgs e)
		{
			this.Result = PoseFile.Groups.All;
		}

		private void OnNoneClick(object sender, RoutedEventArgs e)
		{
			this.Result = PoseFile.Groups.None;
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
