// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using Anamnesis.Files;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for FileBrowserView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FileBrowserView : UserControl, IDrawer
	{
		private static bool isFlattened;
		private static Sort sortMode;

		private readonly Modes mode;
		private readonly IEnumerable<FileFilter> filters;
		private EntryWrapper? selected;
		private bool updatingEntries = false;

		public FileBrowserView(Shortcut[] shortcuts, IEnumerable<FileFilter> filters, DirectoryInfo? defaultDir, string? defaultName, Modes mode)
		{
			if (shortcuts.Length == 0)
				throw new Exception("At least one shortcut must be provided to the file browser constructor");

			this.InitializeComponent();

			if (defaultDir != null && !defaultDir.Exists)
				defaultDir = null;

			List<Shortcut> finalShortcuts = new List<Shortcut>();
			finalShortcuts.AddRange(shortcuts);
			finalShortcuts.Add(FileService.Desktop);

			Shortcut? defaultShortcut = null;
			foreach (Shortcut shortcut in finalShortcuts)
			{
				if (defaultDir == null && mode == Modes.Load && shortcut.Directory.Exists && defaultShortcut == null)
					defaultShortcut = shortcut;

				if (defaultDir != null)
				{
					string defaultDirName = defaultDir.FullName;

					if (!defaultDirName.EndsWith("\\"))
						defaultDirName += "\\";

					if (defaultDirName.Contains(shortcut.Directory.FullName))
					{
						defaultShortcut = shortcut;
					}
				}

				this.Shortcuts.Add(shortcut);
			}

			if (defaultShortcut == null)
			{
				defaultShortcut = this.Shortcuts[0];
				defaultDir = null;
			}

			if (defaultDir == null)
				defaultDir = defaultShortcut.Directory;

			this.BaseDir = defaultShortcut;
			this.CurrentDir = defaultDir;

			this.mode = mode;
			this.filters = filters;

			this.Selector.SearchEnabled = mode == Modes.Load;

			this.ContentArea.DataContext = this;

			this.IsOpen = true;

			if (this.mode == Modes.Save)
			{
				this.FileName = "New " + defaultName;

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

			////this.PropertyChanged?.Invoke(this, new(nameof(FileBrowserView.SortMode)));

			Task.Run(this.UpdateEntries);
		}

		public event DrawerEvent? Close;

		public enum Modes
		{
			Load,
			Save,
		}

		public enum Sort
		{
			None = -1,

			AlphaNumeric,
			Date,
		}

		public bool IsOpen
		{
			get;
			private set;
		}

		public int SortModeInt
		{
			get => (int)this.SortMode;
			set => this.SortMode = (Sort)value;
		}

		public Sort SortMode
		{
			get => sortMode;
			set
			{
				sortMode = value;
				Task.Run(this.UpdateEntries);
			}
		}

		public SettingsService SettingsService => SettingsService.Instance;
		public ObservableCollection<Shortcut> Shortcuts { get; } = new ObservableCollection<Shortcut>();

		public bool UseFileBrowser { get; set; }

		public FileSystemInfo? FinalSelection { get; private set; }

		public EntryWrapper? Selected
		{
			get => this.selected;

			set
			{
				this.selected = value;

				if (this.mode == Modes.Save && this.selected?.Entry is FileInfo)
				{
					this.FileName = this.selected?.Name ?? string.Empty;
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

		public bool ShowExtensions { get; set; } = false;

		[AlsoNotifyFor(nameof(CanSelect))]
		public string? FileName { get; set; }

		public bool ShowFileName => this.mode == Modes.Save;
		public bool CanGoUp => this.CurrentDir.FullName.TrimEnd('\\') != this.BaseDir.Directory.FullName.TrimEnd('\\');
		public string? CurrentPath => this.CurrentDir?.FullName.Replace(this.BaseDir.Directory.FullName.Trim('\\'), string.Empty);
		public bool IsModeOpen => this.mode == Modes.Load;

		[OnChangedMethod(nameof(OnBaseDirChanged))]
		public Shortcut BaseDir { get; set; }

		[AlsoNotifyFor(nameof(CanGoUp), nameof(CurrentPath))]
		[OnChangedMethod(nameof(OnCurrentDirChanged))]
		public DirectoryInfo CurrentDir { get; private set; }

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
					return !string.IsNullOrWhiteSpace(this.FileName);
				}
			}
		}

		public void OnClosed()
		{
		}

		private void OnBaseDirChanged()
		{
			this.CurrentDir = this.BaseDir.Directory;
		}

		private void OnCurrentDirChanged()
		{
			this.Selected = null;
			Task.Run(this.UpdateEntries);
		}

		private async Task UpdateEntries()
		{
			lock (this)
			{
			}

			while (this.updatingEntries)
				await Task.Delay(10);

			lock (this)
			{
				this.updatingEntries = true;
				this.Selector.ClearItems();

				try
				{
					List<EntryWrapper> results = new();
					this.GetEntries(this.CurrentDir, ref results);
					this.Selector.AddItems(results);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to update file list");
				}
			}

			await this.Selector.FilterItemsAsync();
			this.updatingEntries = false;
		}

		private void GetEntries(DirectoryInfo dir, ref List<EntryWrapper> results)
		{
			if (!dir.Exists)
				return;

			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				// file must pass at least one filter
				FileFilter? passedfilter = null;
				foreach (FileFilter filter in this.filters)
				{
					if (filter.Passes(file))
					{
						passedfilter = filter;
						break;
					}
				}

				if (passedfilter == null)
					continue;

				results.Add(new EntryWrapper(file, this, passedfilter));
			}

			DirectoryInfo[] directories = dir.GetDirectories();
			foreach (DirectoryInfo subDir in directories)
			{
				List<EntryWrapper> subResults = new();
				this.GetEntries(subDir, ref subResults);

				////if (subResults.Count == 0)
				////	continue;

				if (!this.IsFlattened)
				{
					results.Add(new EntryWrapper(subDir, this, null));
				}
				else
				{
					results.AddRange(subResults);
				}
			}
		}

		private void OnClose()
		{
			if (this.Selected == null)
				return;

			if (this.Selected.Entry is DirectoryInfo directory)
			{
				this.CurrentDir = directory;
				return;
			}
			else if (this.Selected.Entry is FileInfo file)
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

		private int OnSort(object itemA, object itemB)
		{
			if (itemA is EntryWrapper entryA && itemB is EntryWrapper entryB)
			{
				return this.OnSort(entryA, entryB);
			}

			return 0;
		}

		private int OnSort(EntryWrapper a, EntryWrapper b)
		{
			// Directoreis alweays go to the top.
			if (a.Entry is DirectoryInfo && b.Entry is FileInfo)
				return -1;

			if (a.Entry is FileInfo && b.Entry is DirectoryInfo)
				return 1;

			if (sortMode == Sort.None)
			{
				return 0;
			}
			else if (sortMode == Sort.AlphaNumeric)
			{
				if (a.Name == null || b.Name == null)
					return 0;

				return a.Name.CompareTo(b.Name);
			}
			else if (sortMode == Sort.Date)
			{
				if (a.DateModified == null || b.DateModified == null)
					return 0;

				DateTime dateA = (DateTime)a.DateModified;
				DateTime dateB = (DateTime)b.DateModified;
				return dateA.CompareTo(dateB);
			}

			return 0;
		}

		private void OnShortcutClicked(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.DataContext is Shortcut shortcut)
			{
				if (this.BaseDir != shortcut)
					this.BaseDir = shortcut;

				this.OnBaseDirChanged();
			}
		}

		private void OnGoUpClicked(object? sender, RoutedEventArgs e)
		{
			if (this.CurrentDir.Parent == null)
				return;

			this.CurrentDir = this.CurrentDir.Parent;

			Task.Run(this.UpdateEntries);
		}

		private async void OnCreateFolderClicked(object? sender, RoutedEventArgs e)
		{
			DirectoryInfo newDir = this.CurrentDir.CreateSubdirectory("New Folder");
			await this.UpdateEntries();
			this.Select(newDir);

			if (this.Selected == null)
				return;

			await Task.Delay(50);
			await Dispatch.MainThread();

			this.Selected.Rename = this.Selected.Name;
			this.Selected.IsRenaming = true;
		}

		private void OnFileNameKey(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return || e.Key == Key.Enter)
			{
				e.Handled = true;
				this.OnSelectClicked(sender, e);
			}
		}

		private void OnSelectClicked(object? sender, RoutedEventArgs? e)
		{
			if (!this.CanSelect)
				return;

			if (this.mode == Modes.Load)
			{
				if (this.Selected != null)
				{
					this.FinalSelection = this.Selected.Entry;
				}
			}
			else
			{
				if (this.FileName == null)
					return;

				foreach (char character in Path.GetInvalidFileNameChars())
				{
					if (this.FileName.Contains(character))
					{
						return;
					}
				}

				string ext = this.filters.First().Extension;

				this.FinalSelection = new FileInfo(this.CurrentDir.FullName + "\\" + this.FileName + ext);
			}

			if (this.Selected != null && this.Selected.Entry is DirectoryInfo dir)
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

			bool? confirmed = await GenericDialog.ShowAsync("Are you sure you want to delete: \"" + this.Selected.Name + "\"", "Confirm", MessageBoxButton.OKCancel);

			if (confirmed != true)
				return;

			this.Selected.Entry.Delete();

			_ = Task.Run(this.UpdateEntries);
		}

		private void OnRenameClick(object sender, RoutedEventArgs e)
		{
			if (this.Selected == null)
				return;

			this.Selected.Rename = this.Selected.Name;
			this.Selected.IsRenaming = true;
		}

		private void OnShowExtensionClicked(object sender, RoutedEventArgs e)
		{
			Task.Run(this.UpdateEntries);
		}

		private void CloseDrawer()
		{
			this.IsOpen = false;
			this.Close?.Invoke();
		}

		private void Select(FileSystemInfo entry)
		{
			foreach (EntryWrapper? wrapper in this.Selector.Entries)
			{
				if (wrapper == null)
					continue;

				if (wrapper.Entry.FullName == entry.FullName)
				{
					this.selected = wrapper;
				}
			}
		}

		[AddINotifyPropertyChangedInterface]
		public class EntryWrapper
		{
			public readonly FileSystemInfo Entry;
			public readonly FileBrowserView View;
			public readonly FileFilter? Filter;

			public EntryWrapper(FileSystemInfo entry, FileBrowserView view, FileFilter? filter)
			{
				this.Entry = entry;
				this.View = view;
				this.Filter = filter;
			}

			public string Name
			{
				get
				{
					if (this.Entry is DirectoryInfo)
						return this.Entry.Name;

					if (this.Filter != null && this.Filter.GetNameCallback != null && this.Filter.GetFullNameCallback != null)
					{
						if (SettingsService.Current.ShowFileExtensions || this.View.ShowExtensions)
							return this.Filter.GetFullNameCallback(this.Entry);

						return this.Filter.GetNameCallback(this.Entry);
					}

					if (SettingsService.Current.ShowFileExtensions || this.View.ShowExtensions)
						return Path.GetFileName(this.Entry.Name);

					return Path.GetFileNameWithoutExtension(this.Entry.Name);
				}
			}

			public DateTime? DateModified => this.Entry.LastWriteTime;

			public bool CanSelect
			{
				get
				{
					////if (this.View.mode == Modes.Save)
					////	return this.Entry is IFileSource.IDirectory;

					return true;
				}
			}

			public bool IsDirectory => this.Entry is DirectoryInfo;
			public bool IsRenaming { get; set; }

			public string? Rename
			{
				get => this.Name;
				set
				{
					if (string.IsNullOrEmpty(value))
						return;

					if (!this.IsRenaming)
						return;

					this.IsRenaming = false;

					foreach (char c in Path.GetInvalidFileNameChars())
					{
						if (value.Contains(c))
						{
							return;
						}
					}

					Task.Run(async () =>
					{
						value = value.Trim();

						string? dirPath = Path.GetDirectoryName(this.Entry.FullName);
						string? name = Path.GetFileNameWithoutExtension(this.Entry.FullName);

						if (dirPath == null || name == null)
							return;

						name = name.Replace(name, value);
						string newPath = Path.Combine(dirPath, name);

						if (newPath == this.Entry.FullName)
							return;

						try
						{
							if (this.Entry is FileInfo file)
							{
								string? extension = Path.GetExtension(this.Entry.FullName);
								newPath += extension;
								file.MoveTo(newPath);
							}
							else if (this.Entry is DirectoryInfo dir)
							{
								dir.MoveTo(newPath);
							}
						}
						catch (Exception ex)
						{
							Log.Error(ex, $"Failed to rename file entry to: {newPath}");
						}

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

					StringBuilder b = new StringBuilder();

					if (!string.IsNullOrEmpty(this.Directory))
					{
						b.Append(this.Directory);
						b.Append(" ");
					}

					b.Append(this.Entry.LastWriteTime);

					return b.ToString();
				}
			}

			public string? Directory
			{
				get
				{
					if (this.View.CurrentDir == null)
						return string.Empty;

					string relativePath = this.Entry.FullName?.Replace(this.View.CurrentDir.FullName, string.Empty) ?? string.Empty;

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
