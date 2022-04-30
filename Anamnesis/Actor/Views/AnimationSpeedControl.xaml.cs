// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Windows;
using System.Windows.Controls;
using Anamnesis.Memory;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

public partial class AnimationSpeedControl : UserControl
{
	public static readonly IBind<float> SpeedDp = Binder.Register<float, AnimationSpeedControl>(nameof(Speed));

	public AnimationSpeedControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public float Speed
	{
		get => SpeedDp.Get(this);
		set => SpeedDp.Set(this, value);
	}

	public AnimationMemory.AnimationSlots Slot { get; set; }
	public string? SlotNameOverride { get; set; } = null;

	[DependsOn(nameof(Slot), nameof(SlotNameOverride))]
	public string SlotName => string.IsNullOrEmpty(this.SlotNameOverride) ? this.Slot.ToString() : this.SlotNameOverride;

	private void OnPause(object sender, RoutedEventArgs e)
	{
		this.Speed = 0.0f;
	}

	private void OnResume(object sender, RoutedEventArgs e)
	{
		this.Speed = 1.0f;
	}
}
