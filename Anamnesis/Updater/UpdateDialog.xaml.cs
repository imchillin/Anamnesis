// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Updater;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Extensions;
using Anamnesis.Services;
using PropertyChanged;

/// <summary>
/// Interaction logic for UpdateDialog.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class UpdateDialog : UserControl
{
	public UpdateDialog()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public bool? Result { get; set; }
	public string? Changes { get; set; }
	public bool IsUpdating { get; set; }
	public double Progress { get; set; }

	public static Task Show(string? changes)
	{
		throw new NotImplementedException();
	}

	public void Cancel()
	{
		////this.Close?.Invoke();
	}

	private void OnSkipClicked(object sender, RoutedEventArgs e)
	{
		this.Cancel();
	}

	private async void OnUpdateClicked(object sender, RoutedEventArgs e)
	{
		this.IsUpdating = true;
		await UpdateService.Instance.DoUpdate(this.UpdateProgress);
		////this.Close?.Invoke();
	}

	private void UpdateProgress(double p)
	{
		this.Progress = p;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.Changes == null)
			return;

		this.Viewer.Document = Markdown.ToDocument(this.Changes);
	}
}
