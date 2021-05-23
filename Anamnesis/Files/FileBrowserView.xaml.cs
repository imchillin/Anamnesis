// © Anamnesis.
// Developed by W and A Walsh.
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
	using Serilog;
	using static Anamnesis.Files.IFileSource;

	/// <summary>
	/// Interaction logic for FileBrowserView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FileBrowserView : UserControl, IDrawer, INotifyPropertyChanged
	{
		private static IFileSource? currentFileSource;
		private static Stack<IFileSource.IDirectory> currentPath = new Stack<IFileSource.IDirectory>();
		private static bool isFlattened;

		private FileInfoBase[] fileInfos;
		private Modes mode;
		private string? fileName;
		private EntryWrapper? selected;
		private bool updatingEntries = false;

		public FileBrowserView(FileInfoBase fileInfo, Modes mode)
			: this(new[] { fileInfo }, mode)
		{
			Type? optionsType = mode == Modes.Load ? fileInfo.LoadOptionsViewType : fileInfo.SaveOptionsViewType;
			if (optionsType != null)
			{
				this.OptionsControl = (UserControl?)Activator.CreateInstance(optionsType);
			}

			this.SelectButton.Text = mode == Modes.Load ? LocalizationService.GetString("Common_OpenFile") : LocalizationService.GetString("Common_SaveFile");
			this.Selector.SearchEnabled = mode == Modes.Load;
		}

		public FileBrowserView(FileInfoBase[] fileInfos, Modes mode)
		{
			this.mode = mode;
			this.fileInfos = fileInfos;
			this.InitializeComponent();

			this.SelectButton.Text = mode == Modes.Load ? LocalizationService.GetString("Common_OpenFile") : LocalizationService.GetString("Common_SaveFile");
			this.Selector.SearchEnabled = mode == Modes.Load;

			this.ContentArea.DataContext = this;

			foreach (FileInfoBase info in fileInfos)
			{
				IFileSource[]? sources = info.GetFileSources();

				if (sources == null)
					continue;

				foreach (IFileSource source in sources)
				{
					if (this.FileSources.Contains(source))
						continue;

					this.FileSources.Add(source);
				}
			}

			this.FileSource = this.GetDefaultFileSource();

			this.IsOpen = true;

			if (this.mode == Modes.Save)
			{
				this.FileName = "New " + fileInfos[0].Name;

				Task.Run(async () =>
				{
					await Task.Delay(100);
					await Dispatch.MainThread();
					this.FileNameInputBox.Focus();
					this.FileNameInputBox.SelectAll();

					FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), this.FileNameInputBox);
					Keyboard.Focus(this.FileNameInputBox);
				});
			}

			Task.Run(this.UpdateEntries);

			Type? optionsType = mode == Modes.Load ? fileInfos[0].LoadOptionsViewType : fileInfos[0].SaveOptionsViewType;
			if (optionsType != null)
			{
				this.OptionsControl = (UserControl?)Activator.CreateInstance(optionsType);
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

		public bool ShowOptions
		{
			get => SettingsService.Current.ShowAdvancedOptions;
			set => SettingsService.Current.ShowAdvancedOptions = value;
		}

		public string? FilePath { get; private set; }
		public bool UseFileBrowser { get; set; }
		public UserControl? OptionsControl { get; set; }

		public ObservableCollection<IFileSource> FileSources { get; private set; } = new ObservableCollection<IFileSource>();

		public EntryWrapper? Selected
		{
			get
			{
				return this.selected;
			}

			set
			{
				this.selected = value;

				if (this.mode == Modes.Save && this.selected?.Entry is IFile)
				{
					this.FileName = this.selected?.Name ?? string.Empty;
				}

				if (this.selected != null && this.selected.Entry is IDirectory)
				{
					this.SelectButton.Text = LocalizationService.GetString("Common_OpenDir");
				}
				else
				{
					this.SelectButton.Text = this.mode == Modes.Load ? LocalizationService.GetString("Common_OpenFile") : LocalizationService.GetString("Common_SaveFile");
				}

				// show the options panel for the selected file type
				if (this.selected != null && this.selected.Entry is IFile file)
				{
					FileInfoBase? fileType = file.Type;
					Type? optionsViewType = this.mode == Modes.Load ? fileType?.LoadOptionsViewType : fileType?.SaveOptionsViewType;

					if (this.OptionsControl == null && optionsViewType == null)
						return;

					if (this.OptionsControl?.GetType() == optionsViewType)
						return;

					if (optionsViewType == null)
					{
						this.OptionsControl = null;
						return;
					}

					this.OptionsControl = (UserControl?)Activator.CreateInstance(optionsViewType);
				}
				else
				{
					////this.OptionsControl = null;
				}
			}
		}

		public bool IsFlattened
		{
			get
			{
				return isFlattened;
			}

			set
			{
				isFlattened = value;
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
				return currentFileSource;
			}
			set
			{
				if (value == null)
					return;

				currentFileSource = value;
				currentPath.Clear();
				this.CurrentDir = value?.GetDefaultDirectory();
			}
		}

		public bool CanGoUp
		{
			get
			{
				return currentPath.Count > 1;
			}
		}

		public string CurrentPath
		{
			get
			{
				string str = string.Empty;
				foreach (IFileSource.IDirectory dir in currentPath.Reverse())
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
				if (currentPath.Count <= 0)
					return null;

				return currentPath.Peek();
			}
			private set
			{
				if (value == null)
					return;

				currentPath.Push(value);

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanGoUp)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentPath)));
				Task.Run(this.UpdateEntries);
				this.Selected = null;
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
					if (this.FileSource?.CanWrite != true)
						return false;

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

		public void OnClosed()
		{
		}

		/// <summary>
		/// Gets the last used file source if it is available, else returns "Local Files" or first source.
		/// </summary>
		private IFileSource GetDefaultFileSource()
		{
			if (currentFileSource != null)
			{
				foreach (IFileSource source in this.FileSources)
				{
					if (source == currentFileSource)
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

			while (this.updatingEntries)
				await Task.Delay(10);

			this.updatingEntries = true;
			IEnumerable<IFileSource.IEntry> entries = await this.FileSource.GetEntries(this.CurrentDir, this.IsFlattened, this.fileInfos);
			this.Selector.ClearItems();

			foreach (IFileSource.IEntry entry in entries)
			{
				this.Selector.AddItem(new EntryWrapper(entry, this));
			}

			this.Selector.FilterItems();
			this.updatingEntries = false;
		}

		private void OnClose()
		{
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

			////this.Close?.Invoke();
		}

		private void OnSelectionChanged()
		{
			this.Selected = this.Selector.Value as EntryWrapper;
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is EntryWrapper item)
			{
				bool matches = false;

				matches |= SearchUtility.Matches(item.Name, search);
				matches |= SearchUtility.Matches(item.Directory, search);

				return matches;
			}

			return false;
		}

		private void OnGoUpClicked(object? sender, RoutedEventArgs e)
		{
			currentPath.Pop();

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanGoUp)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentPath)));
			Task.Run(this.UpdateEntries);
		}

		private async void OnCreateFolderClicked(object? sender, RoutedEventArgs e)
		{
			if (this.CurrentDir == null)
				return;

			IDirectory newDir = this.CurrentDir.CreateSubDirectory();
			await this.UpdateEntries();
			this.Select(newDir);

			if (this.Selected == null)
				return;

			await Task.Delay(50);
			await Dispatch.MainThread();

			this.Selected.Rename = this.Selected.Name;
			this.Selected.IsRenaming = true;
		}

		private async void OnSelectClicked(object? sender, RoutedEventArgs? e)
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

				string finalPath = this.FilePath + this.FileExtension;
				if (File.Exists(finalPath))
				{
					string fileName = Path.GetFileNameWithoutExtension(finalPath);
					bool? overwrite = await GenericDialog.Show(LocalizationService.GetStringFormatted("FileBrowser_ReplaceMessage", fileName), LocalizationService.GetString("FileBrowser_ReplaceTitle"), MessageBoxButton.YesNo);
					if (overwrite != true)
					{
						return;
					}
				}
			}

			if (this.Selected != null && this.Selected.Entry is IDirectory dir)
			{
				this.CurrentDir = dir;
				return;
			}

			this.CloseDrawer();
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

		private void OnRenameClick(object sender, RoutedEventArgs e)
		{
			if (this.Selected == null)
				return;

			this.Selected.Rename = this.Selected.Name;
			this.Selected.IsRenaming = true;
		}

		private void CloseDrawer()
		{
			this.IsOpen = false;
			this.Close?.Invoke();
		}

		private void Select(IEntry entry)
		{
			foreach (EntryWrapper? wrapper in this.Selector.Entries)
			{
				if (wrapper == null)
					continue;

				if (wrapper.Entry.Path == entry.Path)
				{
					this.selected = wrapper;
				}
			}
		}

		[AddINotifyPropertyChangedInterface]
		public class EntryWrapper
		{
			public readonly IFileSource.IEntry Entry;
			public readonly FileBrowserView View;

			public EntryWrapper(IFileSource.IEntry entry, FileBrowserView view)
			{
				this.Entry = entry;
				this.View = view;
			}

			public bool? CanWrite => this.View.FileSource?.CanWrite;
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

			public bool IsRenaming { get; set; }

			public string? Rename
			{
				get => this.Name;
				set
				{
					this.IsRenaming = false;

					if (string.IsNullOrEmpty(value))
						return;

					foreach (char c in System.IO.Path.GetInvalidFileNameChars())
					{
						if (value.Contains(c))
						{
							return;
						}
					}

					Task.Run(async () =>
					{
						value = value.Trim();
						await this.Entry.Rename(value);
						await this.View.UpdateEntries();
					});
				}
			}

			public string? Metadata
			{
				get
				{
					if (this.Entry == null)
						return null;

					if (!string.IsNullOrEmpty(this.Directory))
					{
						return this.Directory + " - " + this.Entry.Metadata;
					}
					else
					{
						return this.Entry.Metadata;
					}
				}
			}

			public string? Directory
			{
				get
				{
					if (this.View.CurrentDir == null || this.View.CurrentDir.Path == null)
						return string.Empty;

					string relativePath = this.Entry.Path?.Replace(this.View.CurrentDir.Path, string.Empty) ?? string.Empty;

					if (relativePath != null)
					{
						while (relativePath.StartsWith("\\") && relativePath.Length > 0)
						{
							relativePath = relativePath.Substring(1);
						}
					}

					string? dirName = Path.GetDirectoryName(relativePath);

					if (string.IsNullOrEmpty(dirName))
						return null;

					return "\\" + dirName + "\\";
				}
			}
		}
	}
}
