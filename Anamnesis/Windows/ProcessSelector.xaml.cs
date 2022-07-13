// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Windows;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyChanged;
using XivToolsWpf;

/// <summary>
/// Interaction logic for ProcessSelector.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ProcessSelector : UserControl
{
	public Process? Selected;

	private bool isAutomatic = true;
	private bool showAll;

	public ProcessSelector()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		this.OnRefreshClicked(null, null);

		Task.Run(this.Scan);
	}

	public bool ShowAll
	{
		get => this.showAll;
		set
		{
			this.showAll = value;
			this.OnRefreshClicked(null, null);
		}
	}

	public static Process? FindProcess()
	{
		// early out
		Process[] processes = GetTargetProcesses();
		if (processes.Length == 1)
			return processes[0];

		throw new NotImplementedException();
		/*Dialog dlg = new Dialog();
		ProcessSelector proc = new ProcessSelector();
		proc.window = dlg;
		dlg.ContentArea.Content = proc;
		dlg.ShowDialog();
		return proc.Selected;*/
	}

	private static Process[] GetTargetProcesses()
	{
		Process[] processes = Process.GetProcesses();
		List<Process> results = new List<Process>();
		foreach (Process process in processes)
		{
			if (IsTargetProcess(process.ProcessName))
			{
				results.Add(process);
			}
		}

		return results.ToArray();
	}

	private static bool IsTargetProcess(string name)
	{
		return name.ToLower().Contains("ffxiv_dx11");
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
			if (this.ShowAll || IsTargetProcess(process.ProcessName))
			{
				options.Add(new Option(process));
			}
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
	}

	private void OnCancelClicked(object sender, RoutedEventArgs e)
	{
		this.Selected = null;
	}

	private async Task Scan()
	{
		bool loaded = true;
		while (loaded)
		{
			await Task.Delay(1000);

			if (!this.isAutomatic)
				continue;

			if (Application.Current == null)
				return;

			await Dispatch.MainThread();
			loaded = this.IsLoaded;

			if (!loaded)
				return;

			Process[]? processes = GetTargetProcesses();

			if (processes.Length == 1)
			{
				await Dispatch.MainThread();
				this.Selected = processes[0];
				return;
			}
			else if (processes.Length > 1)
			{
				this.ManualExpander.IsExpanded = true;
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
			this.WindowTitle = process.MainWindowTitle;
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
		public string WindowTitle { get; }
		public int Id { get; }
		public DateTime? StartTime { get; }
		public ImageSource? AppIcon { get; }
		public string? Path { get; }
	}
}
