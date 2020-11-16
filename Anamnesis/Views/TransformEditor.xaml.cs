// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Controls
{
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Styles.DependencyProperties;

	/// <summary>
	/// Interaction logic for BoneEditor.xaml.
	/// </summary>
	public partial class TransformEditor : UserControl
	{
		public static readonly IBind<object> ValueDp = Binder.Register<object, TransformEditor>(nameof(Value));

		public TransformEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public object Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}
	}
}
