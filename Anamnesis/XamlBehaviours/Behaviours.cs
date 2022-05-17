// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.XamlBehaviours;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.Behaviours;
using XivToolsWpf.DragAndDrop;

public static class Behaviours
{
	public static void SetIsReorderable(ItemsControl items, bool enable)
	{
		items.AttachHandler<Reorderable>(enable);
	}

	public static void SetTooltip(DependencyObject host, string key)
	{
		host.AttachHandler<LocalizedTooltipBehaiour>(true, key);
	}
}