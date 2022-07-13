// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Collections.Generic;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.Services;
using PropertyChanged;

/// <summary>
/// Interaction logic for FxivColorSelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class FxivColorSelectorDrawer : UserControl
{
	private readonly List<Item> items = new List<Item>();
	private Item? selectedItem;

	public FxivColorSelectorDrawer(ColorData.Entry[] colors, int selectedIndex)
	{
		this.InitializeComponent();

		this.Selected = selectedIndex;
		for (int i = 0; i < colors.Length; i++)
		{
			if (colors[i].Skip)
				continue;

			Item item = new Item();
			item.Entry = colors[i];
			item.Index = i;
			this.items.Add(item);

			if (i == selectedIndex)
			{
				this.SelectedItem = item;
			}
		}

		this.List.ItemsSource = this.items;

		this.ContentArea.DataContext = this;
	}

	public delegate void SelectorEvent(int value);

	public event SelectorEvent? SelectionChanged;

	public Item? SelectedItem
	{
		get
		{
			return this.selectedItem;
		}

		set
		{
			this.selectedItem = value;

			if (value == null)
				return;

			this.SelectionChanged?.Invoke(value.Index);
		}
	}

	public int Selected
	{
		get
		{
			if (this.selectedItem == null)
				return 0;

			return this.selectedItem.Index;
		}

		set
		{
			foreach (Item item in this.items)
			{
				if (item.Index == value)
				{
					this.SelectedItem = item;
					return;
				}
			}
		}
	}

	public void OnClosed()
	{
	}

	public void Close()
	{
	}

	public class Item
	{
		public ColorData.Entry Entry { get; set; }
		public int Index { get; set; }
	}
}
