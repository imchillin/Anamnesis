// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Services;
	using PropertyChanged;
	using XivToolsWpf;

	using SearchUtility = Anamnesis.SearchUtility;

	/// <summary>
	/// Interaction logic for FileBrowserView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FileBrowserView : UserControl, IDrawer
	{
		private static bool isFlattened;
		private static Sort sortMode;

		private readonly Modes mode;
		private readonly HashSet<string> validExtensions;
		private EntryWrapper? selected;
		private bool updatingEntries = false;
		private DirectoryInfo currentDir;

		public FileBrowserView(DirectoryInfo[] directories, HashSet<string> extensions, string defaultName, Modes mode)
			: this(directories[0], extensions, defaultName, mode)
		{
			foreach (DirectoryInfo dir in directories)
			{
				this.FileSources.Add(dir);
			}
		}

		public FileBrowserView(DirectoryInfo baseDirectory, HashSet<string> extensions, string defaultName, Modes mode)
		{
			this.mode = mode;
			this.BaseDir = baseDirectory;
			this.currentDir = baseDirectory;
			this.validExtensions = extensions;

			this.InitializeComponent();

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

		public ObservableCollection<DirectoryInfo> FileSources { get; } = new ObservableCollection<DirectoryInfo>();

		public string? FilePath { get; private set; }
		public bool UseFileBrowser { get; set; }

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

		[AlsoNotifyFor(nameof(CanSelect))]
		public string? FileName { get; set; }

		public bool ShowFileName => this.mode == Modes.Save;
		public bool CanGoUp => this.CurrentDir.FullName.TrimEnd('\\') != this.BaseDir.FullName.TrimEnd('\\');
		public string? CurrentPath => this.CurrentDir?.FullName.Replace(this.BaseDir.FullName.Trim('\\'), string.Empty);
		public bool IsModeOpen => this.mode == Modes.Load;

		public DirectoryInfo BaseDir { get; set; }

		[AlsoNotifyFor(nameof(CanGoUp), nameof(CurrentPath))]
		public DirectoryInfo CurrentDir
		{
			get => this.currentDir;

			private set
			{
				this.currentDir = value;

				Task.Run(this.UpdateEntries);
				this.Selected = null;
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
					return !string.IsNullOrWhiteSpace(this.FileName);
				}
			}
		}

		public void OnClosed()
		{
		}

		private async Task UpdateEntries()
		{
			while (this.updatingEntries)
				await Task.Delay(10);

			this.updatingEntries = true;

			this.Selector.ClearItems();

			EnumerationOptions op = new EnumerationOptions();
			op.RecurseSubdirectories = this.IsFlattened;
			op.ReturnSpecialDirectories = false;

			if (!this.IsFlattened)
			{
				DirectoryInfo[] directories = this.CurrentDir.GetDirectories("*", op);
				foreach (DirectoryInfo dir in directories)
				{
					this.Selector.AddItem(new EntryWrapper(dir, this));
				}
			}

			FileInfo[] files = this.CurrentDir.GetFiles("*.*", op);
			foreach (FileInfo file in files)
			{
				if (!this.validExtensions.Contains(file.Extension))
					continue;

				this.Selector.AddItem(new EntryWrapper(file, this));
			}

			this.Selector.FilterItems();
			this.updatingEntries = false;
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

		private void OnGoUpClicked(object? sender, RoutedEventArgs e)
		{
			if (this.currentDir.Parent == null || this.currentDir == this.BaseDir)
				return;

			this.CurrentDir = this.currentDir.Parent;

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
				if (this.Selected != null && this.Selected.Entry is FileInfo file)
				{
					this.FilePath = file.FullName;
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

				this.FilePath = this.CurrentDir.FullName + "/" + this.FileName;
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

			bool? confirmed = await GenericDialog.Show("Are you sure you want to delete: \"" + this.Selected.Name + "\"", "Confirm", MessageBoxButton.OKCancel);

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

				if (wrapper.Entry == entry)
				{
					this.selected = wrapper;
				}
			}
		}

		private void OnSourceChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.Source is ComboBox cb && cb.SelectedItem is DirectoryInfo dir)
			{
				if (dir == this.BaseDir)
					return;

				this.BaseDir = dir;
				this.CurrentDir = dir;
			}
		}

		[AddINotifyPropertyChangedInterface]
		public class EntryWrapper
		{
			public readonly FileSystemInfo Entry;
			public readonly FileBrowserView View;

			public EntryWrapper(FileSystemInfo entry, FileBrowserView view)
			{
				this.Entry = entry;
				this.View = view;
			}

			public string Name => Path.GetFileNameWithoutExtension(this.Entry.Name);
			public DateTime? DateModified => this.Entry.LastWriteTime;

			public string Icon
			{
				get
				{
					if (this.Entry is DirectoryInfo)
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

						string newPath = this.Entry.FullName.Replace(this.Name, value);

						if (this.Entry is FileInfo file)
						{
							file.MoveTo(newPath);
						}
						else if (this.Entry is DirectoryInfo dir)
						{
							dir.MoveTo(newPath);
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