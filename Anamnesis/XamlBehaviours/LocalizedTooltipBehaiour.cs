// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.XamlBehaviours;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using XivToolsWpf.Behaviours;

public class LocalizedTooltipBehaiour : Behaviour
{
	public readonly string Key;

	public LocalizedTooltipBehaiour(DependencyObject host, string key)
		: base(host, key)
	{
		this.Key = key;
	}

	public override void OnLoaded()
	{
		base.OnLoaded();

		XivToolsWpf.Controls.TextBlock text = new();
		text.Key = this.Key;

		if (this.Host is FrameworkElement frameworkElement)
		{
			frameworkElement.ToolTip = text;
		}
		else if (this.Host is RowDefinition rowDefinition)
		{
			Grid? grid = rowDefinition.Parent as Grid;

			if (grid == null)
				throw new Exception("No grid found for grid row");

			int columns = grid.ColumnDefinitions.Count;
			int targetRow = -1;

			for (int row = 0; row < grid.RowDefinitions.Count; row++)
			{
				if (grid.RowDefinitions[row] == rowDefinition)
				{
					targetRow = row;
				}
			}

			if (targetRow == -1)
				throw new Exception("Failed to locate row within grid");

			Rectangle rect = new();
			rect.Fill = new SolidColorBrush(Colors.Transparent);
			rect.ToolTip = text;

			if (columns > 0)
				Grid.SetColumnSpan(rect, columns);

			Grid.SetRow(rect, targetRow);

			grid.Children.Add(rect);
		}
		else
		{
			throw new Exception($"Tooltips not supported on objects of type: {this.Host}");
		}
	}
}
