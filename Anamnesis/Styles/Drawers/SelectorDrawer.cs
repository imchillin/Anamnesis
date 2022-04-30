// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Drawers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Services;
using XivToolsWpf.Selectors;

public abstract class SelectorDrawer : UserControl, ISelectorView, IDrawer, INotifyPropertyChanged
{
	private Type? objectType;
	private object? currentValue;

	public SelectorDrawer()
		: base()
	{
		this.Loaded += this.OnLoaded;
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event DrawerEvent? OnClosing;
	public event SelectorSelectedEvent? SelectionChanged;

	public Selector Selector { get; private set; } = null!;

	public bool SearchEnabled
	{
		get => this.Selector.SearchEnabled;
		set => this.Selector.SearchEnabled = value;
	}

	public IEnumerable<object> Entries => this.Selector.Entries;

	public static TView Show<TView, TValue>(TValue? current, Action<TValue> changed)
		where TView : SelectorDrawer
		where TValue : class
	{
		TView view = Activator.CreateInstance<TView>();
		Show(view, current, changed);
		return view;
	}

	public static async Task<TValue?> ShowAsync<TView, TValue>(TValue? current)
		where TView : SelectorDrawer
		where TValue : class
	{
		SelectorDrawer view = Activator.CreateInstance<TView>();

		view.objectType = typeof(TValue);
		view.currentValue = current;
		await ViewService.ShowDrawer(view);

		bool open = true;
		((IDrawer)view).OnClosing += () => open = false;

		while (open)
			await Task.Delay(100);

		return view.Selector.Value as TValue;
	}

	public static void Show<TValue>(SelectorDrawer view, TValue? current, Action<TValue> changed)
		where TValue : class
	{
		view.objectType = typeof(TValue);
		view.currentValue = current;
		view.SelectionChanged += (close) =>
		{
			object? v = view.Selector.Value;
			if (v is TValue tval)
			{
				changed?.Invoke(tval);
			}

			if (close)
			{
				((IDrawer)view).Close();
			}
		};

		Task.Run(() => ViewService.ShowDrawer(view));
	}

	public void Close()
	{
		this.OnClosing?.Invoke();
	}

	public virtual void OnClosed()
	{
	}

	// forward selector APIs
	public void AddItems(IEnumerable<object> items) => this.Selector.AddItems(items);
	public void AddItem(object item) => this.Selector.AddItem(item);
	public void ClearItems() => this.Selector.ClearItems();
	public void FilterItems() => this.Selector.FilterItems();
	public Task FilterItemsAsync() => this.Selector.FilterItemsAsync();
	public void RaiseSelectionChanged() => this.Selector.RaiseSelectionChanged();

	protected abstract Task LoadItems();
	protected abstract bool Filter(object item, string[]? search);
	protected abstract int Compare(object itemA, object itemB);

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		var selector = this.FindChild<Selector>();

		if (selector == null)
			throw new Exception("Selector drawer missing selector component");

		this.Selector = selector;
		this.Selector.ObjectType = this.objectType;
		this.Selector.Value = this.currentValue;

		this.Selector.Filter += this.Filter;
		this.Selector.SelectionChanged += this.OnSelectionChanged;
		this.Selector.LoadItems += this.LoadItems;
		this.Selector.Sort += this.Compare;
	}

	private void OnSelectionChanged(bool close)
	{
		this.SelectionChanged?.Invoke(close);
	}
}

public abstract class SelectorDrawer<T> : SelectorDrawer, ISelectorView
{
	public T? Value
	{
		get => (T?)this.Selector.Value;
		set => this.Selector.Value = value;
	}

	protected virtual bool Filter(T item, string[]? search)
	{
		return true;
	}

	protected virtual int Compare(T itemA, T itemB)
	{
		return 0;
	}

	protected override sealed bool Filter(object item, string[]? search)
	{
		if (item is T tItem)
			return this.Filter(tItem, search);

		return false;
	}

	protected override sealed int Compare(object itemA, object itemB)
	{
		if (itemA == itemB)
			return 0;

		if (itemA is T tItemA && itemB is T tItemB)
			return this.Compare(tItemA, tItemB);

		return 0;
	}
}
