// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public interface IPanelGroupHost : IPanel
{
	ContentPresenter PanelGroupArea { get; }
	IPanelGroupHost? ParentHost { get; set; }
	bool IsVisible { get; }

	IEnumerable<IPanelGroupHost> Children { get; }

	void Show();
	void Show(IPanelGroupHost copy);
	void AddChild(IPanel panel);
	void RemoveChild(IPanel panel);
}

public interface IPanel
{
	string? TitleKey { get; set; }
	string? Title { get; set; }
	IconChar Icon { get; set; }
	Color? TitleColor { get; set; }
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