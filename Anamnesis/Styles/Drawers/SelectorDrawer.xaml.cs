// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Drawers
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for SelectorDrawer.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SelectorDrawer : UserControl, INotifyPropertyChanged, IDrawer
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(SelectorDrawer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(SelectorDrawer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

		private static readonly Dictionary<Type, string?> SearchInputs = new Dictionary<Type, string?>();
		private static readonly Dictionary<Type, double> ScrollPositions = new Dictionary<Type, double>();
		private readonly List<ItemEntry> entries = new List<ItemEntry>();

		private Type? objectType;
		private bool searching = false;
		private bool idle = true;
		private string[]? searchQuery;
		private bool xamlLoading = false;
		private bool abortSearch = false;

		public SelectorDrawer()
		{
			this.InitializeComponent();
			this.xamlLoading = true;
			this.ContentArea.DataContext = this;

			this.PropertyChanged += this.OnPropertyChanged;
			this.ProgressBar.Visibility = Visibility.Visible;
		}

		public delegate bool FilterEvent(object item, string[]? search);
		public delegate int SortEvent(object itemA, object itemB);
		public delegate Task GetItemsEvent();

		public event PropertyChangedEventHandler? PropertyChanged;
		public event DrawerEvent? Close;
		public event FilterEvent? Filter;
		public event SortEvent? Sort;
		public event DrawerEvent? SelectionChanged;
		public event GetItemsEvent? LoadItems;

		public interface ISelectorView : IDrawer
		{
			SelectorDrawer Selector { get; }
		}

		public ObservableCollection<object> FilteredItems { get; set; } = new ObservableCollection<object>();

		public bool SearchEnabled { get; set; } = true;
		public bool HasSearch { get; set; } = false;

		public IEnumerable<object> Entries
		{
			get
			{
				List<object> values = new List<object>();
				foreach (ItemEntry entry in this.entries)
					values.Add(entry.Item);

				return values;
			}
		}

		public object? Value
		{
			get => this.GetValue(ValueProperty);
			set => this.SetValue(ValueProperty, value);
		}

		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)this.GetValue(ItemTemplateProperty);
			set => this.SetValue(ItemTemplateProperty, value);
		}

		public double ScrollPosition
		{
			get
			{
				ScrollViewer? scroll = this.ScrollViewer;
				if (scroll == null)
					return 0;

				return scroll.VerticalOffset;
			}

			set
			{
				ScrollViewer? scroll = this.ScrollViewer;
				if (scroll == null)
					return;

				scroll.ScrollToVerticalOffset(value);
			}
		}

		private static ILogger Log => Serilog.Log.ForContext<SelectorDrawer>();

		private ScrollViewer? ScrollViewer
		{
			get
			{
				Decorator? border = VisualTreeHelper.GetChild(this.ListBox, 0) as Decorator;
				if (border == null)
					return null;

				return border.Child as ScrollViewer;
			}
		}

		public static TView Show<TView, TValue>(TValue? current, Action<TValue> changed)
			where TView : ISelectorView
			where TValue : class
		{
			TView view = Activator.CreateInstance<TView>();
			Show(view, current, changed);
			return view;
		}

		public static async Task<TValue?> ShowAsync<TView, TValue>(TValue? current)
			where TView : ISelectorView
			where TValue : class
		{
			ISelectorView view = Activator.CreateInstance<TView>();

			view.Selector.objectType = typeof(TValue);
			view.Selector.Value = current;
			await ViewService.ShowDrawer(view);

			bool open = true;
			view.Close += () => open = false;

			while (open)
				await Task.Delay(100);

			return view.Selector.Value as TValue;
		}

		public static void Show<TValue>(ISelectorView view, TValue? current, Action<TValue> changed)
			where TValue : class
		{
			view.Selector.objectType = typeof(TValue);
			view.Selector.Value = current;
			view.Selector.SelectionChanged += () =>
			{
				object? v = view.Selector.Value;
				if (v is TValue tval)
				{
					changed?.Invoke(tval);
				}
			};

			Task.Run(() => ViewService.ShowDrawer(view));
		}

		public void OnClosed()
		{
		}

		public void ClearItems()
		{
			lock (this.entries)
			{
				this.entries.Clear();
			}
		}

		public void AddItem(object item)
		{
			lock (this.entries)
			{
				ItemEntry entry = default;
				entry.Item = item;
				entry.OriginalIndex = this.entries.Count;
				this.entries.Add(entry);

				if (this.objectType == null)
				{
					this.objectType = item.GetType();
				}
			}
		}

		public void AddItems(IEnumerable<object> items)
		{
			lock (this.entries)
			{
				foreach (object item in items)
				{
					ItemEntry entry = default;
					entry.Item = item;
					entry.OriginalIndex = this.entries.Count;
					this.entries.Add(entry);

					if (this.objectType == null)
					{
						this.objectType = item.GetType();
					}
				}
			}
		}

		public void FilterItems()
		{
			Task.Run(this.DoFilter);
		}

		public Task FilterItemsAsync()
		{
			return this.DoFilter();
		}

		public void RaiseSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is SelectorDrawer view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (this.objectType != null)
			{
				if (SearchInputs.ContainsKey(this.objectType))
					this.SearchBox.Text = SearchInputs[this.objectType];

				if (ScrollPositions.ContainsKey(this.objectType))
				{
					this.ScrollPosition = ScrollPositions[this.objectType];
				}
			}

			Keyboard.Focus(this.SearchBox);
			this.SearchBox.CaretIndex = int.MaxValue;
			this.xamlLoading = false;

			if (this.LoadItems != null)
			{
				Task.Run(async () =>
				{
					await Dispatch.NonUiThread();
					await this.LoadItems.Invoke();

					await this.FilterItemsAsync();

					await Dispatch.MainThread();
					this.ProgressBar.Visibility = Visibility.Collapsed;

					this.ListBox.ScrollIntoView(this.Value);
				});
			}
			else
			{
				this.ProgressBar.Visibility = Visibility.Collapsed;
			}
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (this.objectType == null)
				return;

			if (!SearchInputs.ContainsKey(this.objectType))
				SearchInputs.Add(this.objectType, null);

			SearchInputs[this.objectType] = this.SearchBox.Text;

			if (!ScrollPositions.ContainsKey(this.objectType))
				ScrollPositions.Add(this.objectType, 0);

			ScrollPositions[this.objectType] = this.ScrollPosition;
		}

		private void OnSearchChanged(object sender, TextChangedEventArgs e)
		{
			if (this.objectType == null)
				return;

			string str = this.SearchBox.Text;

			this.HasSearch = !string.IsNullOrWhiteSpace(str);

			SearchInputs[this.objectType] = str;
			Task.Run(async () => { await this.Search(str); });

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HasSearch)));
		}

		private void OnClearSearchClicked(object sender, RoutedEventArgs e)
		{
			this.SearchBox.Text = string.Empty;
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(this.entries))
			{
				Task.Run(this.DoFilter);
			}
		}

		private async Task Search(string str)
		{
			this.idle = false;
			this.abortSearch = true;

			if (!this.xamlLoading)
				await Task.Delay(50);

			try
			{
				while (this.searching)
					await Task.Delay(100);

				this.searching = true;
				string currentInput = await Application.Current.Dispatcher.InvokeAsync<string>(() =>
				{
					return this.SearchBox.Text;
				});

				// If the input was changed, abort this task
				if (str != currentInput)
				{
					this.searching = false;
					return;
				}

				if (string.IsNullOrEmpty(str))
				{
					this.searchQuery = null;
				}
				else
				{
					str = str.ToLower();
					this.searchQuery = str.Split(' ');
				}

				this.abortSearch = false;
				await Task.Run(this.DoFilter);
				this.searching = false;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to perform search");
			}

			this.idle = true;
		}

		private async Task DoFilter()
		{
			this.idle = false;

			if (!this.SearchEnabled)
				this.searchQuery = null;

			ConcurrentQueue<ItemEntry> entries;
			lock (this.entries)
			{
				entries = new ConcurrentQueue<ItemEntry>(this.entries);
			}

			ConcurrentBag<ItemEntry> filteredEntries = new ConcurrentBag<ItemEntry>();

			int threads = 4;
			List<Task> tasks = new List<Task>();
			for (int i = 0; i < threads; i++)
			{
				Task t = Task.Run(() =>
				{
					while (!entries.IsEmpty)
					{
						ItemEntry entry;
						if (!entries.TryDequeue(out entry))
							continue;

						try
						{
							if (this.Filter != null && !this.Filter.Invoke(entry.Item, this.searchQuery))
								continue;
						}
						catch (Exception ex)
						{
							Log.Error(ex, $"Failed to filter selector item: {entry.Item}");
						}

						filteredEntries.Add(entry);

						if (this.abortSearch)
						{
							entries.Clear();
						}
					}
				});

				tasks.Add(t);
			}

			await Task.WhenAll(tasks.ToArray());

			IOrderedEnumerable<ItemEntry>? sortedFilteredEntries = filteredEntries.OrderBy(cc => cc.OriginalIndex);

			if (this.Sort != null)
			{
				Compare comp = new Compare(this.Sort);
				sortedFilteredEntries = sortedFilteredEntries.OrderBy(cc => cc.Item, comp);
			}

			await Application.Current.Dispatcher.InvokeAsync(() =>
			{
				this.FilteredItems.Clear();

				foreach (ItemEntry obj in sortedFilteredEntries)
				{
					this.FilteredItems.Add(obj.Item);
				}

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FilteredItems)));
			});

			this.idle = true;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			if (this.searching)
				return;

			this.RaiseSelectionChanged();
		}

		private async void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			while (!this.idle)
				await Task.Delay(10);

			if (this.FilteredItems.Count <= 0)
				return;

			this.Value = this.FilteredItems[0];
		}

		private void OnDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Point pos = e.GetPosition(this.ListBox);

			// over scrollbar
			if (pos.X > this.ListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth)
				return;

			this.Close?.Invoke();
		}

		private struct ItemEntry
		{
			public object Item;
			public int OriginalIndex;
		}

		private class Compare : IComparer<object>
		{
			private readonly SortEvent filter;

			public Compare(SortEvent filter)
			{
				this.filter = filter;
			}

			int IComparer<object>.Compare(object? x, object? y)
			{
				if (x == null || y == null)
					return 0;

				return this.filter.Invoke(x, y);
			}
		}
	}
}
