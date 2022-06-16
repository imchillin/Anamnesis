// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels.Navigation;

using FontAwesome.Sharp;
using System;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class Shortcut : UserControl
{
	public static readonly IBind<string> LabelKeyDp = Binder.Register<string, Shortcut>(nameof(LabelKey));
	public static readonly IBind<string> ToolTipKeyDp = Binder.Register<string, Shortcut>(nameof(ToolTipKey));
	public static readonly IBind<IconChar> IconDp = Binder.Register<IconChar, Shortcut>(nameof(Icon));
	public static readonly IBind<Uri> UriDp = Binder.Register<Uri, Shortcut>(nameof(Uri));
	public static readonly IBind<bool> ShowTextDp = Binder.Register<bool, Shortcut>(nameof(ShowText));

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

	public Uri Uri
	{
		get => UriDp.Get(this);
		set => UriDp.Set(this, value);
	}

	public bool ShowText
	{
		get => ShowTextDp.Get(this);
		set => ShowTextDp.Set(this, value);
	}

	private void OnClicked(object sender, RoutedEventArgs e)
	{
	}
}
