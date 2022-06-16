// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System.Windows;
using System.Windows.Controls;

public interface IPanelGroupHost
{
	ContentPresenter PanelGroupArea { get; }
	string Title { get; set; }
	IconChar Icon { get; set; }
	Rect Rect { get; set; }

	void Show();
	void DragMove();

	void Align(IPanelGroupHost other, HorizontalAlignment horizontal = HorizontalAlignment.Center, VerticalAlignment vertical = VerticalAlignment.Center)
	{
		Rect otherRect = other.Rect;
		Rect selfRect = this.Rect;

		double x = otherRect.Left;
		if (horizontal == HorizontalAlignment.Left)
		{
			x = otherRect.Left - selfRect.Width;
		}
		else if (horizontal == HorizontalAlignment.Center)
		{
			x = (otherRect.Left + (otherRect.Width / 2)) - (selfRect.Width / 2);
		}
		else if (horizontal == HorizontalAlignment.Right)
		{
			x = otherRect.Left + otherRect.Width;
		}

		double y = otherRect.Top;
		if (vertical == VerticalAlignment.Top)
		{
			y = otherRect.Top - selfRect.Height;
		}
		else if (vertical == VerticalAlignment.Center)
		{
			y = (otherRect.Top + (otherRect.Height / 2)) - (selfRect.Height / 2);
		}
		else if (vertical == VerticalAlignment.Bottom)
		{
			y = otherRect.Top + otherRect.Height;
		}

		this.Rect = new(x, y, selfRect.Width, selfRect.Height);
	}
}
