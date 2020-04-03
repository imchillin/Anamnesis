// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class EquipmentSelector : UserControl, INotifyPropertyChanged
	{
		private IGameDataService gameData;

		private ItemSlots slot;
		private string[] searchQuerry;
		private bool searching = false;

		public EquipmentSelector(ItemSlots slot)
		{
			this.slot = slot;

			this.InitializeComponent();
			this.DataContext = this;

			this.gameData = Module.Services.Get<IGameDataService>();

			Task.Run(this.Filter);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ObservableCollection<IItem> Items { get; set; } = new ObservableCollection<IItem>();

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(this.SearchBox);
		}

		private void OnSearchChanged(object sender, TextChangedEventArgs e)
		{
			string str = this.SearchBox.Text;
			Task.Run(async () => { await this.Search(str); });
		}

		private async Task Search(string str)
		{
			await Task.Delay(250);

			try
			{
				while (this.searching)
					await Task.Delay(100);

				this.searching = true;
				string currentInput = await App.Current.Dispatcher.InvokeAsync<string>(() =>
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

				await Task.Run(this.Filter);
				this.searching = false;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void Filter()
		{
			ObservableCollection<IItem> filteredItems = new ObservableCollection<IItem>();

			foreach (IItem item in this.gameData.Items.All)
			{
				if (!this.ItemFilter(item))
					continue;

				filteredItems.Add(item);
			}

			App.Current.Dispatcher.InvokeAsync(() =>
			{
				this.Items = filteredItems;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Items)));
			});
		}

		private bool ItemFilter(IItem item)
		{
			// skip items without names
			if (string.IsNullOrEmpty(item.Name))
				return false;

			if (!item.FitsInSlot(this.slot))
				return false;

			if (this.searchQuerry != null)
			{
				bool matchesSearch = true;
				foreach (string str in this.searchQuerry)
					matchesSearch &= item.Name.ToLower().Contains(str);

				if (!matchesSearch)
				{
					return false;
				}
			}

			return true;
		}
	}
}
