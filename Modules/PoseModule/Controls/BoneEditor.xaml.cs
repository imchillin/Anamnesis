// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Controls
{
	using System.Windows.Controls;
	using Anamnesis.WpfStyles.DependencyProperties;

	/// <summary>
	/// Interaction logic for BoneEditor.xaml.
	/// </summary>
	public partial class BoneEditor : UserControl
	{
		public static readonly IBind<Bone> ValueDp = Binder.Register<Bone, BoneEditor>(nameof(Value));

		public BoneEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public Bone Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		public PoseService PoseService { get => PoseModule.PoseService.Instance; }
	}
}
