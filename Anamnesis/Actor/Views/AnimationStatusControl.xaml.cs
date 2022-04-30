// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Windows.Controls;
using Anamnesis.Memory;
using XivToolsWpf.DependencyProperties;

public partial class AnimationStatusControl : UserControl
{
	public static readonly IBind<float> SpeedDp = Binder.Register<float, AnimationStatusControl>(nameof(Speed));
	public static readonly IBind<ushort> AnimationIdDp = Binder.Register<ushort, AnimationStatusControl>(nameof(AnimationId));

	public AnimationStatusControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ushort AnimationId
	{
		get => AnimationIdDp.Get(this);
		set => AnimationIdDp.Set(this, value);
	}

	public float Speed
	{
		get => SpeedDp.Get(this);
		set => SpeedDp.Set(this, value);
	}

	public AnimationMemory.AnimationSlots Slot { get; set; }
}
