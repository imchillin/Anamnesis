// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System;
using System.Windows;
using System.Windows.Controls;

public interface IPanelGroupHost : IPanel
{
	ContentPresenter PanelGroupArea { get; }
	void Show();
	void AddChild(IPanel panel);
}

public interface IPanel
{
	string? TitleKey { get; set; }
	string? Title { get; set; }
	IconChar Icon { get; set; }
	Rect Rect { get; set; }
	bool ShowBackground { get; set; }
	bool AllowAutoClose { get; set; }
	bool Topmost { get; set; }
	bool CanResize { get; set; }

	IPanelGroupHost Host { get; }

	void DragMove();
	void SetParent(IPanel other) => other.Host.AddChild(this);
	void Close();
}