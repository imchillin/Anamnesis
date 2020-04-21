// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Drawers
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for SelectorDrawer.xaml.
	/// </summary>
	public partial class SelectorDrawer : UserControl, INotifyPropertyChanged, IDrawer
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(SelectorDrawer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(SelectorDrawer), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

		private string[] searchQuerry;
		private bool searching = false;
		private bool idle = true;
		private object oldValue;

		public SelectorDrawer()
		{
			this.InitializeComponent();
			this.DataContext = this;

			this.PropertyChanged += this.OnPropertyChanged;
			this.FilterItems();
		}

		public delegate bool FilterEvent(object item, string[] search);

		public event PropertyChangedEventHandler PropertyChanged;
		public event DrawerEvent Close;
		public event FilterEvent Filter;

		public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
		public ObservableCollection<object> FilteredItems { get; set; } = new ObservableCollection<object>();

		public object Value
		{
			get
			{
				return this.GetValue(ValueProperty);
			}

			set
			{
				this.SetValue(ValueProperty, value);
				this.oldValue = value;
			}
		}

		public DataTemplate ItemTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(ItemTemplateProperty);
			}

			set
			{
				this.SetValue(ItemTemplateProperty, value);
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
			Keyboard.Focus(this.SearchBox);
		}

		private void OnSearchChanged(object sender, TextChangedEventArgs e)
		{
			string str = this.SearchBox.Text;
			Task.Run(async () => { await this.Search(str); });
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(this.Items))
			{
				Task.Run(this.DoFilter);
			}
		}

		private async Task Search(string str)
		{
			this.idle = false;
			await Task.Delay(250);

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

				await Task.Run(this.DoFilter);
				this.searching = false;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}

			this.idle = true;
		}

		private void DoFilter()
		{
			this.idle = false;
			ObservableCollection<object> filteredItems = new ObservableCollection<object>();

			foreach (object item in this.Items)
			{
				if (this.Filter != null && !this.Filter.Invoke(item, this.searchQuerry))
					continue;

				filteredItems.Add(item);
			}

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				this.FilteredItems = filteredItems;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FilteredItems)));
			});

			this.idle = true;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0 || e.AddedItems[0] == this.oldValue)
				return;

			if (this.searching)
				return;

			this.Close?.Invoke();
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
	}
}
