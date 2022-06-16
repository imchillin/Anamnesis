// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Services;
using FontAwesome.Sharp;
using System;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class Shortcut : UserControl
{
	public static readonly IBind<string> LabelKeyDp = Binder.Register<string, Shortcut>(nameof(LabelKey), BindMode.OneWay);
	public static readonly IBind<string> ToolTipKeyDp = Binder.Register<string, Shortcut>(nameof(ToolTipKey), BindMode.OneWay);
	public static readonly IBind<IconChar> IconDp = Binder.Register<IconChar, Shortcut>(nameof(Icon), BindMode.OneWay);
	public static readonly IBind<string> DestinationDp = Binder.Register<string, Shortcut>(nameof(Destination), BindMode.OneWay);
	public static readonly IBind<object> ContextDp = Binder.Register<object, Shortcut>(nameof(Context), BindMode.OneWay);
	public static readonly IBind<bool> ShowTextDp = Binder.Register<bool, Shortcut>(nameof(ShowText), BindMode.OneWay);

	public Shortcut()
	{
		this.InitializeComponent();
		this.ButtonContent.DataContext = this;
	}

	public string LabelKey
	{
		get => LabelKeyDp.Get(this);
		set => LabelKeyDp.Set(this, value);
	}

	public string ToolTipKey
	{
		get => ToolTipKeyDp.Get(this);
		set => ToolTipKeyDp.Set(this, value);
	}

	public IconChar Icon
	{
		get => IconDp.Get(this);
		set => IconDp.Set(this, value);
	}

	public string Destination
	{
		get => DestinationDp.Get(this);
		set => DestinationDp.Set(this, value);
	}

	public object Context
	{
		get => ContextDp.Get(this);
		set => ContextDp.Set(this, value);
	}

	public bool ShowText
	{
		get => ShowTextDp.Get(this);
		set => ShowTextDp.Set(this, value);
	}

	private void OnClicked(object sender, RoutedEventArgs e)
	{
		NavigationService.Navigate(new(this.Destination, this.Context));
	}
}
