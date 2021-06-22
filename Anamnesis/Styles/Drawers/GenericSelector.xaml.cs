// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Drawers
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Services;

	/// <summary>
	/// Interaction logic for GenericSelector.xaml.
	/// </summary>
	public partial class GenericSelector : UserControl, SelectorDrawer.ISelectorView
	{
		public GenericSelector(IEnumerable<ISelectable> options)
		{
			this.InitializeComponent();

			this.Selector.AddItems(options);
			this.Selector.FilterItems();
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;

		public ISelectable? Value
		{
			get
			{
				object? val = this.Selector.Value;

				if (val is ISelectable itemVal)
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

		public static void Show<T>(T? current, IEnumerable<T> options, Action<T> changed)
			where T : class, ISelectable
		{
			GenericSelector selector = new GenericSelector(options);
			SelectorDrawer.Show<ISelectable>(selector, current, (v) => changed.Invoke((T)v));
		}

		public void OnClosed()
		{
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
			if (obj is ISelectable item)
			{
				if (string.IsNullOrEmpty(item.Name))
					return false;

				if (!SearchUtility.Matches(item.Name, search))
					return false;

				return true;
			}

			return false;
		}
	}

	#pragma warning disable SA1201
	public interface ISelectable
	{
		string Name { get; }
		string? Description { get; }
	}
}
