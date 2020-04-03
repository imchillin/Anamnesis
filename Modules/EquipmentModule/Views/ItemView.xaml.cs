// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	public partial class ItemView : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty SlotProperty = DependencyProperty.Register(nameof(Slot), typeof(ItemSlots), typeof(ItemView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(IItem), typeof(ItemView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

		public ItemView()
		{
			this.InitializeComponent();
			this.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ItemSlots Slot
		{
			get
			{
				return (ItemSlots)this.GetValue(SlotProperty);
			}

			set
			{
				this.SetValue(SlotProperty, value);
			}
		}

		public IItem Value
		{
			get
			{
				return (IItem)this.GetValue(ValueProperty);
			}

			set
			{
				this.SetValue(ValueProperty, value);
			}
		}

		public string SlotName
		{
			get
			{
				return this.Slot.ToDisplayName();
			}
		}

		private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ItemView view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();
			EquipmentSelector selector = new EquipmentSelector(this.Slot);
			viewService.ShowDrawer(selector, "Select " + this.SlotName);
		}
	}
}
