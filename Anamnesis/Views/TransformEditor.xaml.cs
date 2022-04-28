// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Controls;

using System.Windows.Controls;
using Anamnesis.Memory;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for BoneEditor.xaml.
/// </summary>
public partial class TransformEditor : UserControl
{
	public static readonly IBind<ITransform> ValueDp = Binder.Register<ITransform, TransformEditor>(nameof(Value));
	public static readonly IBind<bool> CanTranslateDp = Binder.Register<bool, TransformEditor>(nameof(CanTranslate), BindMode.OneWay);

	public TransformEditor()
	{
		this.InitializeComponent();

		this.CanTranslate = true;

		this.ContentArea.DataContext = this;
	}

	public ITransform Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public bool CanTranslate
	{
		get => CanTranslateDp.Get(this);
		set => CanTranslateDp.Set(this, value);
	}
}
