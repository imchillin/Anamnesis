// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dialogs
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using PropertyChanged;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for WaitDialog.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class WaitDialog : UserControl, IDialog, IDisposable
	{
		public WaitDialog()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event DialogEvent? Close;

		public string Message { get; private set; } = string.Empty;
		public double Progress { get; private set; } = 0;

		public static async Task<WaitDialog> ShowAsync(string message, string caption)
		{
			await Dispatch.MainThread();

			WaitDialog dlg = new();
			dlg.Message = message;

			_ = Task.Run(async () =>
			{
				await Dispatch.MainThread();
				await ViewService.ShowDialog(caption, dlg);
			});

			return dlg;
		}

		public async Task SetProgress(double progress)
		{
			await Dispatch.MainThread();
			this.Progress = progress;
		}

		public void Complete()
		{
			_ = Task.Run(async () =>
			{
				await Dispatch.MainThread();
				this.Close?.Invoke();
			});
		}

		public void Cancel()
		{
		}

		public void Dispose()
		{
			this.Complete();
		}
	}
}
