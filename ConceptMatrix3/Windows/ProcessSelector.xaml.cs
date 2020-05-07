// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Windows
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Interop;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;

	/// <summary>
	/// Interaction logic for ProcessSelector.xaml.
	/// </summary>
	public partial class ProcessSelector : UserControl
	{
		public Process Selected;

		private bool isAutomatic = true;
		private Dialog window;

		public ProcessSelector()
		{
			this.InitializeComponent();
			this.OnRefreshClicked(null, null);

			Task.Run(this.Scan);
		}

		public static Process FindProcess()
		{
			Dialog dlg = new Dialog();
			ProcessSelector proc = new ProcessSelector();
			proc.window = dlg;
			dlg.ContentArea.Content = proc;
			dlg.ShowDialog();
			return proc.Selected;
		}

		private static ImageSource IconToImageSource(Icon icon)
		{
			return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
		}

		private void OnRefreshClicked(object sender, RoutedEventArgs e)
		{
			List<Option> options = new List<Option>();
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				options.Add(new Option(process));
			}

			this.ProcessGrid.ItemsSource = options;
		}

		private void OnManualExpanded(object sender, RoutedEventArgs e)
		{
			this.isAutomatic = false;
			this.OkButton.Visibility = Visibility.Visible;
			this.Expander.Header = "Manual";
		}

		private void OnManualCollapsed(object sender, RoutedEventArgs e)
		{
			this.isAutomatic = true;
			this.OkButton.Visibility = Visibility.Collapsed;
			this.Expander.Header = "Scanning...";
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.isAutomatic)
				return;

			Option op = this.ProcessGrid.SelectedValue as Option;
			this.OkButton.IsEnabled = op != null && !op.Locked;
		}

		private void OnOkClicked(object sender, RoutedEventArgs e)
		{
			this.Selected = (this.ProcessGrid.SelectedValue as Option)?.Process;
			this.window.Close();
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			this.Selected = null;
			this.window.Close();
		}

		private async Task Scan()
		{
			bool loaded = true;
			while (loaded)
			{
				await Task.Delay(1000);

				if (!this.isAutomatic)
					continue;

				Application.Current.Dispatcher.Invoke(() =>
				{
					loaded = this.IsLoaded;
				});

				Process[] processes = Process.GetProcesses();
				foreach (Process process in processes)
				{
					if (process.ProcessName.ToLower().Contains("ffxiv_dx11"))
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							this.Selected = process;
							this.window.Close();
						});

						return;
					}
				}
			}
		}

		private class Option
		{
			public readonly Process Process;

			public Option(Process process)
			{
				this.Process = process;
				this.Name = process.ProcessName;
				this.Id = process.Id;

				try
				{
					this.StartTime = process.StartTime;
					this.AppIcon = IconToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(process.MainModule.FileName));
					this.Path = process.MainModule.FileName;
				}
				catch (Exception)
				{
					this.Locked = true;
				}
			}

			public bool Locked { get; }
			public string Name { get; }
			public int Id { get; }
			public DateTime StartTime { get; }
			public ImageSource AppIcon { get; }
			public string Path { get; }
		}
	}
}
