// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.Services;

	/// <summary>
	/// Interaction logic for GposePrompt.xaml.
	/// </summary>
	public partial class GposePrompt : UserControl, IDialog<bool>
	{
		public GposePrompt(bool destination, string message)
		{
			this.InitializeComponent();
			this.Result = false;
			this.Destination = destination;
			this.Message = message;
		}

		public event DialogEvent? Close;

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
			this.Close?.Invoke();
		}

		public async Task Wait()
		{
			while (GposeService.Instance.IsGpose != this.Destination)
			{
				await Task.Delay(500);
			}

			this.Result = true;
			this.Close?.Invoke();
		}
	}
}
