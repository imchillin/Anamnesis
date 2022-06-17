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
}

public interface IPanel
{
	public enum AlignmentModes
	{
		SideRight,
	}

	string Title { get; set; }
	IconChar Icon { get; set; }
	Rect Rect { get; set; }
	bool ShowBackground { get; set; }
	bool AllowAutoClose { get; set; }

	void DragMove();

	void Align(IPanelGroupHost other, AlignmentModes mode = AlignmentModes.SideRight)
	{
		Rect otherRect = other.Rect;
		Rect selfRect = this.Rect;

		double x = otherRect.Left;
		double y = otherRect.Top;

		if (mode == AlignmentModes.SideRight)
		{
			x = otherRect.Left + otherRect.Width;
			y = otherRect.Top;
		}
		else
		{
			throw new NotImplementedException();
		}

		this.Rect = new(x, y, selfRect.Width, selfRect.Height);
	}
}