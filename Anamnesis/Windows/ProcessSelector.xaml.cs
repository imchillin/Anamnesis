// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Windows
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
	using Anamnesis.Memory;

	/// <summary>
	/// Interaction logic for ProcessSelector.xaml.
	/// </summary>
	public partial class ProcessSelector : UserControl
	{
		public Process? Selected;

		private bool isAutomatic = true;
		private Dialog? window;

		public ProcessSelector()
		{
			this.InitializeComponent();
			this.OnRefreshClicked(null, null);

			Task.Run(this.Scan);
		}

		public static Process? FindProcess()
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

		private void OnRefreshClicked(object? sender, RoutedEventArgs? e)
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
			this.AutoLabel.Visibility = Visibility.Collapsed;
			this.ManualLabel.Visibility = Visibility.Visible;
			this.ScanProgress.Visibility = Visibility.Collapsed;
		}

		private void OnManualCollapsed(object sender, RoutedEventArgs e)
		{
			this.isAutomatic = true;
			this.OkButton.Visibility = Visibility.Collapsed;
			this.AutoLabel.Visibility = Visibility.Visible;
			this.ManualLabel.Visibility = Visibility.Collapsed;
			this.ScanProgress.Visibility = Visibility.Visible;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.isAutomatic)
				return;

			this.OkButton.IsEnabled = this.ProcessGrid.SelectedValue is Option op && !op.Locked;
		}

		private void OnOkClicked(object sender, RoutedEventArgs e)
		{
			this.Selected = (this.ProcessGrid.SelectedValue as Option)?.Process;
			MemoryService.Instance.EnableProcess = true;
			this.window?.Close();
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			this.Selected = null;
			this.window?.Close();
		}

		private async Task Scan()
		{
			bool loaded = true;
			while (loaded)
			{
				await Task.Delay(1000);

				if (!this.isAutomatic)
					continue;

				if (!MemoryService.Instance.EnableProcess)
					continue;

				if (Application.Current == null)
					return;

				await Dispatch.MainThread();
				loaded = this.IsLoaded;

				if (!loaded)
					return;

				Process[] processes = Process.GetProcesses();
				foreach (Process process in processes)
				{
					if (process.ProcessName.ToLower().Contains("ffxiv_dx11"))
					{
						await Dispatch.MainThread();
						this.Selected = process;
						this.window?.Close();

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

					if (process.MainModule?.FileName != null)
					{
						Icon? icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
						if (icon != null)
						{
							this.AppIcon = IconToImageSource(icon);
						}
					}

					this.Path = process.MainModule?.FileName ?? process.Id.ToString();
				}
				catch (Exception)
				{
					this.Locked = true;
				}
			}

			public bool Locked { get; }
			public string Name { get; }
			public int Id { get; }
			public DateTime? StartTime { get; }
			public ImageSource? AppIcon { get; }
			public string? Path { get; }
		}
	}
}
