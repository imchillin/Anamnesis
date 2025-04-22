// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Selectors;

public static class GenericSelectorUtil
{
	public static void Show<T>(IEnumerable<T> items, Action<T> selected)
		where T : ISelectable
	{
		Task.Run(async () => await ShowAsync<T>(items, selected));
	}

	public static async Task ShowAsync<T>(IEnumerable<T> items, Action<T> selected)
		where T : ISelectable
	{
		await Dispatch.MainThread();

		// Convert items to IEnumerable<ISelectable>
		IEnumerable<ISelectable> selectableItems = items.Cast<ISelectable>();

		GenericSelector sel = new(selectableItems);
		sel.SelectionChanged += (c) =>
		{
			if (sel.Value is not T tVal)
				return;

			selected.Invoke(tVal);
		};

		await ViewService.ShowDrawer(sel);
	}
}
