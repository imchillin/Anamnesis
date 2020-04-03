// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Input;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class EquipmentSelector : UserControl
	{
		private IGameDataService gameData;
		private ICollectionView collecitonView;

		private string[] searchQuerry;
		private Task searchTask;

		public EquipmentSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			this.gameData = Module.Services.Get<IGameDataService>();

			foreach (IItem item in this.gameData.Items.All)
			{
				this.Items.Add(item);
			}

			this.collecitonView = CollectionViewSource.GetDefaultView(this.Items);
			this.collecitonView.Filter = this.ItemFilter;
		}

		public ObservableCollection<IItem> Items { get; set; } = new ObservableCollection<IItem>();

		private bool ItemFilter(object obj)
		{
			if (obj is IItem item)
			{
				// skip items without names
				if (string.IsNullOrEmpty(item.Name))
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

			return false;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(this.SearchBox);
		}

		private void OnSearchChanged(object sender, TextChangedEventArgs e)
		{
			string str = this.SearchBox.Text;
			this.searchTask = Task.Run(async () => { await this.Search(str); });
		}

		private async Task Search(string str)
		{
			await Task.Delay(250);

			try
			{
				string currentInput = await App.Current.Dispatcher.InvokeAsync<string>(() =>
				{
					return this.SearchBox.Text;
				});

				// If the input was changed, abort this task
				if (str != currentInput)
					return;

				if (string.IsNullOrEmpty(str))
				{
					this.searchQuerry = null;
				}
				else
				{
					str = str.ToLower();
					this.searchQuerry = str.Split(' ');
				}

				await App.Current.Dispatcher.InvokeAsync(() =>
				{
					this.collecitonView.Refresh();
				});
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
