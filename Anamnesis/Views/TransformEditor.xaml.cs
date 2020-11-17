// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Controls
{
	using System;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Styles.DependencyProperties;

	/// <summary>
	/// Interaction logic for BoneEditor.xaml.
	/// </summary>
	public partial class TransformEditor : UserControl
	{
		public static readonly IBind<ITransform> ValueDp = Binder.Register<ITransform, TransformEditor>(nameof(Value));

		public TransformEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public ITransform Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}
	}
}
