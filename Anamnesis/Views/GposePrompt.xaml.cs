// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for GposePrompt.xaml.
	/// </summary>
	public partial class GposePrompt : UserControl, IDialog<bool>
	{
		public GposePrompt(bool destination, string message)
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.Result = false;
			this.Destination = destination;
			this.Message = message;
			this.Waiting = true;

			Task.Run(this.Wait);
		}

		public event DialogEvent? Close;

		public bool Waiting { get; set; }
		public bool Result { get; set; }
		public bool Destination { get; set; }
		public string Message { get; set; }

		public static async Task<bool> WaitForChange(bool destination, string message)
		{
			if (GposeService.Instance.IsGpose == destination)
				return true;

			GposePrompt prompt = new GposePrompt(destination, message);
			return await ViewService.ShowDialog<GposePrompt, bool>("Gpose", prompt);
		}

		public void Cancel()
		{
			this.Result = false;
			this.Waiting = false;
			this.Close?.Invoke();
		}

		private void OnSkipClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			this.Cancel();
		}

		private async Task Wait()
		{
			while (this.Waiting && GposeService.Instance.IsGpose != this.Destination)
			{
				await Task.Delay(500);
			}

			this.Waiting = false;

			while (GposeService.Instance.IsChangingState)
				await Task.Delay(250);

			this.Result = true;

			await Dispatch.MainThread();
			this.Close?.Invoke();
		}
	}
}
