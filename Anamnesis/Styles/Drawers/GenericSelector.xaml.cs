// Concept Matrix 3.
// Licensed under the MIT license.

namespace Styles.Drawers
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for GenericSelector.xaml.
	/// </summary>
	public partial class GenericSelector : UserControl, SelectorDrawer.ISelectorView
	{
		public GenericSelector(List<Item> options)
		{
			this.InitializeComponent();

			foreach (Item item in options)
			{
				this.Selector.Items.Add(item);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;

		public Item? Value
		{
			get
			{
				object? val = this.Selector.Value;

				if (val is Item itemVal)
					return itemVal;

				return null;
			}

			set
			{
				this.Selector.Value = value;
			}
		}

		SelectorDrawer SelectorDrawer.ISelectorView.Selector
		{
			get
			{
				return this.Selector;
			}
		}

		public static void Show(string title, Item current, List<Item> options, Action<Item> changed)
		{
			GenericSelector selector = new GenericSelector(options);
			SelectorDrawer.Show<Item>(title, selector, current, changed);
		}

		private void OnClose()
		{
			this.Close?.Invoke();
		}

		private void OnSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		#pragma warning disable SA1011
		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is Item item)
			{
				if (!SearchUtility.Matches(item.Name, search))
					return false;

				return true;
			}

			return false;
		}

		public class Item
		{
			public Item()
			{
				this.Name = string.Empty;
			}

			public Item(string name, object? data = null, string? descroption = null)
			{
				this.Name = name;
				this.Description = descroption;
				this.Data = data;
			}

			public string Name { get; set; }
			public string? Description { get; set; }
			public object? Data { get; set; }
		}
	}
}
