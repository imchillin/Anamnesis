// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Selectors;

public static class GenericSelectorUtil
{
	public static void Show<T>(IEnumerable<T> items, Action<T> selected)
		where T : class, ISelectable
	{
		Task.Run(async () => await ShowAsync<T>(items, selected));
	}

	public static async Task ShowAsync<T>(IEnumerable<T> items, Action<T> selected)
		where T : class, ISelectable
	{
		await Dispatch.MainThread();

		GenericSelector sel = new(items);
		sel.SelectionChanged += (c) =>
		{
			if (sel.Value is not T tVal)
				return;

			selected.Invoke(tVal);
		};

		////await ViewService.ShowDrawer(sel);
		throw new NotImplementedException();
	}
}
