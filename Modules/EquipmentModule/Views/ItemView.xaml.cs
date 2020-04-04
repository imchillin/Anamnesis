// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	public partial class ItemView : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty SlotProperty = DependencyProperty.Register(nameof(Slot), typeof(ItemSlots), typeof(ItemView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(IItem), typeof(ItemView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty DyeProperty = DependencyProperty.Register(nameof(Dye), typeof(IDye), typeof(ItemView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));

		private IGameDataService gameData;

		public ItemView()
		{
			this.InitializeComponent();
			this.DataContext = this;

			this.gameData = Module.Services.Get<IGameDataService>();
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

		public IDye Dye
		{
			get
			{
				return (IDye)this.GetValue(DyeProperty);
			}

			set
			{
				this.SetValue(DyeProperty, value);
			}
		}

		[DependsOn(nameof(Slot))]
		public string SlotName
		{
			get
			{
				return this.Slot.ToDisplayName();
			}
		}

		[DependsOn(nameof(Value))]
		public string Key
		{
			get
			{
				return this.Value?.Key.ToString();
			}
			set
			{
				int val = int.Parse(value);
				this.Value = this.gameData.Items.Get(val);
			}
		}

		[DependsOn(nameof(Value))]
		public string ModelBaseId
		{
			get
			{
				return this.Value?.ModelBaseId.ToString();
			}
			set
			{
			}
		}

		[DependsOn(nameof(Value))]
		public string ModelVariantId
		{
			get
			{
				return this.Value?.ModelVariantId.ToString();
			}

			set
			{
			}
		}

		[DependsOn(nameof(Value))]
		public string ModelId
		{
			get
			{
				return this.Value?.ModelId.ToString();
			}

			set
			{
			}
		}

		private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ItemView view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
		}

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			EquipmentSelector selector = new EquipmentSelector(this.Slot);
			selector.Value = this.Value;
			await viewService.ShowDrawer(selector, "Select " + this.SlotName);
			this.Value = selector.Value;
		}

		private async void OnDyeClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			DyeSelector selector = new DyeSelector();
			selector.Value = this.Dye;
			await viewService.ShowDrawer(selector, "Select Dye");
			this.Dye = selector.Value;
		}

		private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}
	}
}
