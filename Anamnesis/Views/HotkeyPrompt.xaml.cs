// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Text;
	using System.Windows.Input;
	using Anamnesis.Keyboard;
	using XivToolsWpf.DependencyProperties;

	/// <summary>
	/// Interaction logic for HotkeyPrompt.xaml.
	/// </summary>
	public partial class HotkeyPrompt : System.Windows.Controls.TextBlock
	{
		public static readonly IBind<string> FunctionDp = Binder.Register<string, HotkeyPrompt>(nameof(Function), OnKeyChanged, BindMode.OneWay);

		public HotkeyPrompt()
		{
			this.InitializeComponent();
			this.LoadString();
		}

		public string? Function { get; set; }

		public static void OnKeyChanged(HotkeyPrompt sender, string val)
		{
			sender.Function = val;
			sender.LoadString();
		}

		private void LoadString()
		{
			this.Text = null;

			if (string.IsNullOrEmpty(this.Function))
				return;

			if (!HotkeyService.Exists)
				return;

			KeyCombination? keys = HotkeyService.GetBind(this.Function);
			if (keys == null)
				return;

			StringBuilder str = new StringBuilder();

			if (keys.Modifiers.HasFlag(ModifierKeys.Control))
			{
				str.Append("[CTRL] +");
			}

			if (keys.Modifiers.HasFlag(ModifierKeys.Alt))
			{
				str.Append("[ALT] +");
			}

			if (keys.Modifiers.HasFlag(ModifierKeys.Shift))
			{
				str.Append("[SHIFT] +");
			}

			str.Append('[');
			str.Append(keys.Key);
			str.Append(']');

			this.Text = str.ToString();
		}
	}
}
