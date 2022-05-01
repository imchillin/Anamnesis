// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System.Windows.Controls;
using XivToolsWpf.Behaviours;
using XivToolsWpf.DragAndDrop;

public static class Behaviours
{
	public static void SetIsReorderable(ItemsControl items, bool enable)
	{
		items.AttachHandler<Reorderable>(enable);
	}
}