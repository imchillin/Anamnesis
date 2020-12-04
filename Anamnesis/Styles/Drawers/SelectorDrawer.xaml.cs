// Concept Matrix 3.
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
	using Anamnesis;
	using Anamnesis.Services;
	using PropertyChanged;
	using SimpleLog;

	/// <summary>
	/// Interaction logic for SelectorDrawer.xaml.
	/// </summary>
	public partial class SelectorDrawer : UserControl, INotifyPropertyChanged, IDrawer
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(SelectorDrawer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(SelectorDrawer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

		private static readonly SimpleLog.Logger Log = SimpleLog.Log.GetLogger<SelectorDrawer>();

		private static Dictionary<Type, string?> searchInputs = new Dictionary<Type, string?>();
		private List<ItemEntry> entries = new List<ItemEntry>();

		private Type? objectType;
		private bool searching = false;
		private bool idle = true;
		private string[]? searchQuerry;
		private bool loading = false;
		private bool abortSearch = false;

		public SelectorDrawer()
		{
			this.InitializeComponent();
			this.loading = true;
			this.DataContext = this;

			this.PropertyChanged += this.OnPropertyChanged;
		}

		public delegate bool FilterEvent(object item, string[]? search);

		public event PropertyChangedEventHandler? PropertyChanged;
		public event DrawerEvent? Close;
		public event FilterEvent? Filter;
		public event DrawerEvent? SelectionChanged;

		public interface ISelectorView : IDrawer
		{
			SelectorDrawer Selector { get; }
		}

		public ObservableCollection<object> FilteredItems { get; set; } = new ObservableCollection<object>();

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

		public static void Show<TView, TValue>(TValue? current, Action<TValue> changed)
			where TView : ISelectorView
			where TValue : class
		{
			ISelectorView view = Activator.CreateInstance<TView>();
			Show(view, current, changed);
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

			Task.Run(() =>
			{
				ViewService.ShowDrawer(view);
			});
		}

		public void AddItem(object item)
		{
			lock (this.entries)
			{
				ItemEntry entry = default;
				entry.Item = item;
				entry.OriginalIndex = this.entries.Count;
				this.entries.Add(entry);
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
				}
			}
		}

		public void FilterItems()
		{
			Task.Run(this.DoFilter);
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
			if (this.objectType != null && searchInputs.ContainsKey(this.objectType))
				this.SearchBox.Text = searchInputs[this.objectType];

			Keyboard.Focus(this.SearchBox);
			this.SearchBox.CaretIndex = int.MaxValue;
			this.loading = false;
		}

		private void OnSearchChanged(object sender, TextChangedEventArgs e)
		{
			if (this.objectType == null)
				return;

			if (!searchInputs.ContainsKey(this.objectType))
				searchInputs.Add(this.objectType, null);

			string str = this.SearchBox.Text;
			searchInputs[this.objectType] = str;
			Task.Run(async () => { await this.Search(str); });
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

			if (!this.loading)
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
					this.searchQuerry = null;
				}
				else
				{
					str = str.ToLower();
					this.searchQuerry = str.Split(' ');
				}

				this.abortSearch = false;
				await Task.Run(this.DoFilter);
				this.searching = false;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}

			this.idle = true;
		}

		private async Task DoFilter()
		{
			this.idle = false;

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
							if (this.Filter != null && !this.Filter.Invoke(entry.Item, this.searchQuerry))
								continue;
						}
						catch (Exception ex)
						{
							Log.Write(Severity.Error, new Exception($"Failed to filter selector item: {entry.Item}", ex));
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

			this.SelectionChanged?.Invoke();
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
			this.Close?.Invoke();
		}

		private struct ItemEntry
		{
			public object Item;
			public int OriginalIndex;
		}
	}
}
