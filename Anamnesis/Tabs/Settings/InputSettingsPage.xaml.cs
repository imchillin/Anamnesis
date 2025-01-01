// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Keyboard;
using Anamnesis.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

/// <summary>
/// Interaction logic for InputSettingsPage.xaml.
/// </summary>
public partial class InputSettingsPage : UserControl, ISettingSection
{
	public InputSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Initialize setting categories
		this.SettingCategories = new()
		{
			{ "Input", new SettingCategory("Input", this.InputGroupBox) },
			{ "Hotkeys", new SettingCategory("Hotkeys", this.HotkeysGroupBox) },
		};

		// Set up input category settings
		this.SettingCategories["Input"].Settings.Add(new Setting("Settings_WrapRotations", this.Input_Input_WrapRotations));

		// Set up hotkeys category settings
		this.SettingCategories["Hotkeys"].Settings.Add(new Setting("Settings_EnableHotkeys", this.Input_Hotkeys_EnableHotkeys));
		this.SettingCategories["Hotkeys"].Settings.Add(new Setting("Settings_EnableGameHotkeys", this.Input_Hotkeys_EnableGameHotkeys));
		this.SettingCategories["Hotkeys"].Settings.Add(new Setting("Settings_EnableForwardKeys", this.Input_Hotkeys_EnableForwardKeys));
		this.SettingCategories["Hotkeys"].Settings.Add(new Setting("Settings_KeysHeader", this.Input_Hotkeys_List));

		// Set up hotkey options
		this.Hotkeys = SettingsService.Current.KeyboardBindings.GetBinds()
			.Select(bind => new HotkeyOption(bind.Key, bind.Value))
			.ToList();

		ICollectionView view = CollectionViewSource.GetDefaultView(this.Hotkeys);
		view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
		view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
		view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		this.HotkeyList.ItemsSource = view;
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static int LabelColumnWidth => 150;
	public Dictionary<string, SettingCategory> SettingCategories { get; }
	public IEnumerable<HotkeyOption> Hotkeys { get; }

	public class HotkeyOption
	{
		private readonly KeyCombination keys;
		private readonly string function;

		public HotkeyOption(string function, KeyCombination keys)
		{
			this.keys = keys;
			this.function = function;

			string[] parts = this.function.Split('.');
			if (parts.Length == 2)
			{
				this.Category = LocalizationService.GetString("HotkeyCategory_" + parts[0], true);
				if (this.Category == string.Empty)
					this.Category = parts[0];

				this.Name = LocalizationService.GetString("Hotkey_" + parts[1], true);
				if (this.Name == string.Empty)
					this.Name = parts[1];
			}
			else
			{
				this.Category = string.Empty;
				this.Name = LocalizationService.GetString("Hotkey_" + function, true);
				if (this.Name == string.Empty)
					this.Name = function;
			}
		}

		public string Category { get; }
		public string Name { get; }

		public string KeyName => this.keys.Key.ToString();
		public string? ModifierName => this.keys.Modifiers == ModifierKeys.None ? null : this.GetModifierNames();

		private string GetModifierNames()
		{
			var builder = new StringBuilder();

			if (this.keys.Modifiers.HasFlag(ModifierKeys.Control))
				builder.Append("Ctrl + ");

			if (this.keys.Modifiers.HasFlag(ModifierKeys.Shift))
				builder.Append("Shift + ");

			if (this.keys.Modifiers.HasFlag(ModifierKeys.Alt))
				builder.Append("Alt + ");

			if (this.keys.Modifiers.HasFlag(ModifierKeys.Windows))
				builder.Append("Win + ");

			return builder.ToString().TrimEnd('+', ' ');
		}
	}
}
