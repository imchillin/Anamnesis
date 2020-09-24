// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using Anamnesis;
	using Anamnesis.Files;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Services;
	using PropertyChanged;
	using static Anamnesis.Files.IFileSource;

	/// <summary>
	/// Interaction logic for FileBrowserView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class FileBrowserView : UserControl, IDrawer, INotifyPropertyChanged
	{
		private static IFileSource? lastFileSource;
		////private static IDirectory? lastDirectory;

		private FileInfoBase[] fileInfos;
		private IFileSource? fileSource;
		private Stack<IFileSource.IDirectory> currentPath = new Stack<IFileSource.IDirectory>();
		private Modes mode;
		private string? fileName;
		private bool isFlattened;
		private EntryWrapper? selected;

		public FileBrowserView(FileInfoBase fileInfo, Modes mode)
			: this(new[] { fileInfo }, mode)
		{
		}

		public FileBrowserView(FileInfoBase[] fileInfos, Modes mode)
		{
			this.mode = mode;
			this.fileInfos = fileInfos;
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			foreach (FileInfoBase info in fileInfos)
			{
				IFileSource? source = info.GetFileSource();
				this.FileSources.Add(source);
			}

			this.FileSource = this.GetDefaultFileSource();

			this.IsOpen = true;

			if (this.mode == Modes.Save)
			{
				this.FileName = "New " + fileInfos[0].Name;

				Task.Run(async () =>
				{
					await Task.Delay(100);
					Application.Current.Dispatcher.Invoke(() =>
					{
						this.FileNameInputBox.Focus();
						this.FileNameInputBox.SelectAll();

						FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), this.FileNameInputBox);
						Keyboard.Focus(this.FileNameInputBox);
					});
				});
			}
		}

		public event DrawerEvent? Close;
		public event PropertyChangedEventHandler? PropertyChanged;

		public enum Modes
		{
			Load,
			Save,
		}

		public bool IsOpen
		{
			get;
			private set;
		}

		public string? FilePath { get; private set; }
		public bool AdvancedMode { get; private set; }
		public bool UseFileBrowser { get; set; }

		public ObservableCollection<IFileSource> FileSources { get; private set; } = new ObservableCollection<IFileSource>();
		public ObservableCollection<EntryWrapper> Entries { get; private set; } = new ObservableCollection<EntryWrapper>();

		public EntryWrapper? Selected
		{
			get
			{
				return this.selected;
			}

			set
			{
				this.selected = value;

				if (this.mode == Modes.Save)
				{
					this.FileName = this.selected?.Name ?? string.Empty;
				}
			}
		}

		public bool IsFlattened
		{
			get
			{
				return this.isFlattened;
			}

			set
			{
				this.isFlattened = value;
				Task.Run(this.UpdateEntries);
			}
		}

		public string? FileName
		{
			get
			{
				return this.fileName;
			}
			set
			{
				this.fileName = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanSelect)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanSelectAdvanced)));
			}
		}

		public string FileExtension
		{
			get
			{
				return "." + this.fileInfos[0].Extension;
			}
		}

		public IFileSource? FileSource
		{
			get
			{
				return this.fileSource;
			}
			set
			{
				lastFileSource = value;
				this.fileSource = value;
				this.currentPath.Clear();
				this.CurrentDir = this.FileSource?.GetDefaultDirectory();
			}
		}

		public bool CanGoUp
		{
			get
			{
				return this.currentPath.Count > 1;
			}
		}

		public string CurrentPath
		{
			get
			{
				string str = string.Empty;
				foreach (IFileSource.IDirectory dir in this.currentPath.Reverse())
				{
					str += dir.Name + "/";
				}

				return str;
			}
		}

		public IFileSource.IDirectory? CurrentDir
		{
			get
			{
				if (this.currentPath.Count <= 0)
					return null;

				return this.currentPath.Peek();
			}
			private set
			{
				if (value == null)
					return;

				this.currentPath.Push(value);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanGoUp)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentPath)));
				Task.Run(this.UpdateEntries);
			}
		}

		public bool ShowFileName
		{
			get
			{
				return this.mode == Modes.Save;
			}
		}

		public bool CanSelect
		{
			get
			{
				if (this.mode == Modes.Load)
				{
					return this.Selected?.CanSelect ?? false;
				}
				else
				{
					return this.CurrentDir != null && !string.IsNullOrWhiteSpace(this.FileName);
				}
			}
		}

		public bool CanSelectAdvanced
		{
			get
			{
				if (!this.CanSelect)
					return false;

				if (this.mode == Modes.Load)
				{
					return true;
				}
				else
				{
					return true; //// this.fileInfos[0].SupportsAdvancedMode;
				}
			}
		}

		/// <summary>
		/// Gets the last used file source if it is available, else returns "Local Files" or first source.
		/// </summary>
		private IFileSource GetDefaultFileSource()
		{
			if (lastFileSource != null)
			{
				foreach (IFileSource source in this.FileSources)
				{
					if (source == lastFileSource)
					{
						return source;
					}
				}
			}

			if (this.FileSources == null || this.FileSources.Count <= 0)
				throw new Exception("No file sources");

			foreach (IFileSource source in this.FileSources)
			{
				// Bit of a hack, but always prefer the anamnesis file source if its available
				if (source.Name == "Local Files")
				{
					return source;
				}
			}

			return this.FileSources[0];
		}

		private async Task UpdateEntries()
		{
			if (this.FileSource == null || this.CurrentDir == null)
				return;

			IEnumerable<IFileSource.IEntry> entries = await this.FileSource.GetEntries(this.CurrentDir, this.IsFlattened);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.Entries.Clear();

				foreach (IFileSource.IEntry entry in entries)
				{
					this.Entries.Add(new EntryWrapper(entry, this));
				}
			});
		}

		private void OnMouseDoubleClick(object? sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			if (this.Selected == null)
				return;

			if (this.Selected.Entry is IFileSource.IDirectory directory)
			{
				this.CurrentDir = directory;
				return;
			}
			else if (this.Selected.Entry is IFileSource.IFile file)
			{
				this.OnSelectClicked(null, null);
			}
		}

		private void OnGoUpClicked(object? sender, RoutedEventArgs e)
		{
			this.currentPath.Pop();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanGoUp)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentPath)));
			Task.Run(this.UpdateEntries);
		}

		private void OnSelectClicked(object? sender, RoutedEventArgs? e)
		{
			if (!this.CanSelect)
				return;

			if (this.CurrentDir == null)
				return;

			if (this.mode == Modes.Load)
			{
				if (this.Selected != null && this.Selected.Entry is IFileSource.IFile file)
				{
					this.FilePath = file.Path;
				}
			}
			else
			{
				this.FilePath = this.CurrentDir.Path + "/" + this.FileName;
			}

			this.CloseDrawer();
		}

		private void OnAdvancedClicked(object sender, RoutedEventArgs e)
		{
			this.AdvancedMode = true;
			this.OnSelectClicked(sender, e);
		}

		private void OnBrowseClicked(object sender, RoutedEventArgs e)
		{
			this.UseFileBrowser = true;
			this.CloseDrawer();
		}

		private async void OnDeleteClick(object sender, RoutedEventArgs e)
		{
			if (this.Selected == null)
				return;

			bool? confirmed = await GenericDialog.Show("Are you sure you want to delete: \"" + this.Selected.Name + "\"", "Confirm", MessageBoxButton.OKCancel);

			if (confirmed != true)
				return;

			await this.Selected.Entry.Delete();

			_ = Task.Run(this.UpdateEntries);
		}

		private void CloseDrawer()
		{
			this.IsOpen = false;
			this.Close?.Invoke();
		}

		public class EntryWrapper
		{
			public readonly IFileSource.IEntry Entry;
			public readonly FileBrowserView View;

			public EntryWrapper(IFileSource.IEntry entry, FileBrowserView view)
			{
				this.Entry = entry;
				this.View = view;
			}

			public string? Name => this.Entry.Name;

			public string Icon
			{
				get
				{
					if (this.Entry is IFileSource.IDirectory)
						return "folder";

					return "file";
				}
			}

			public bool CanSelect
			{
				get
				{
					////if (this.View.mode == Modes.Save)
					////	return this.Entry is IFileSource.IDirectory;

					return true;
				}
			}

			public bool SupportsAdvanced
			{
				get
				{
					return true;
				}
			}

			public string Directory
			{
				get
				{
					if (this.View.CurrentDir == null || this.View.CurrentDir.Path == null)
						return string.Empty;

					string relativePath = this.Entry.Path?.Replace(this.View.CurrentDir.Path, string.Empty) ?? string.Empty;

					if (relativePath != null && relativePath.StartsWith('\\'))
						relativePath = relativePath.Substring(2);

					string? dirName = Path.GetDirectoryName(relativePath);

					if (dirName == null)
						return string.Empty;

					return dirName;
				}
			}
		}
	}
}
