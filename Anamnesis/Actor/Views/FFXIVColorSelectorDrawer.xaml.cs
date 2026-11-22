// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Utilities;
using Anamnesis.Services;
using PropertyChanged;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

/// <summary>
/// Interaction logic for FxivColorSelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class FxivColorSelectorDrawer : UserControl, IDrawer
{
	private readonly List<Item> items = new();
	private Item? selectedItem;

	public FxivColorSelectorDrawer(ColorData.Entry[] colors, int selectedIndex)
	{
		this.InitializeComponent();

		this.Selected = selectedIndex;
		for (int i = 0; i < colors.Length; i++)
		{
			if (colors[i].Skip)
				continue;

			var item = new Item
			{
				Entry = colors[i],
				Index = i,
			};
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

	public event DrawerEvent? OnClosing;
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
		this.OnClosing?.Invoke();
	}

	public class Item
	{
		private ColorData.Entry entry;
		private SolidColorBrush? brush;

		public ColorData.Entry Entry
		{
			get => this.entry;
			set
			{
				this.entry = value;
				this.brush = null;
			}
		}

		public int Index { get; set; }

		public SolidColorBrush Brush
		{
			get
			{
				if (this.brush == null)
				{
					SolidColorBrush created = new(this.entry.WpfColor);
					created.Freeze();
					this.brush = created;
				}

				return this.brush;
			}
		}
	}
}
