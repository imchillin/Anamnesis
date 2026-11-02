// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Dialogs;

using Anamnesis.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;

/// <summary>
/// Interaction logic for GenericInputDialog.xaml.
/// </summary>
public partial class GenericInputDialog : UserControl, IDialog<string?>
{
	public GenericInputDialog()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event DialogEvent? Close;

	public string? Result { get; set; }
	public string Message { get; set; } = string.Empty;
	public string Text { get; set; } = string.Empty;
	public int MaxLength { get; set; } = 0;

	public string? Left { get; set; }
	public string? Right { get; set; }

	public static async Task<string?> ShowAsync(string message, string caption, string defaultText = "", int maxLength = 0)
	{
		await Dispatch.MainThread();

		var dlg = new GenericInputDialog
		{
			Message = message,
			Text = defaultText,
			MaxLength = maxLength,
			Left = "Cancel",
			Right = "OK",
		};

		return await ViewService.ShowDialog<GenericInputDialog, string?>(caption, dlg);
	}

	public void Cancel()
	{
		this.Result = null;
		this.Close?.Invoke();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.InputBox.Focus();
		this.InputBox.SelectAll();
	}

	private void OnLeftClick(object sender, RoutedEventArgs e)
	{
		this.Cancel();
	}

	private void OnRightClick(object sender, RoutedEventArgs e)
	{
		this.Result = this.Text;
		this.Close?.Invoke();
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			this.OnRightClick(sender, e);
		}
		else if (e.Key == Key.Escape)
		{
			this.OnLeftClick(sender, e);
		}
	}
}
