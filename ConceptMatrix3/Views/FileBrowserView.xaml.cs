// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Dialogs;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for FileBrowserView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class FileBrowserView : UserControl, IDrawer, INotifyPropertyChanged
	{
		private FileType[] fileTypes;
		private IFileSource fileSource;
		private Stack<IFileSource.IDirectory> currentPath = new Stack<IFileSource.IDirectory>();
		private Modes mode;
		private string fileName;
		private bool isFlattened;

		public FileBrowserView(List<IFileSource> sources, FileType[] fileTypes, Modes mode)
		{
			this.mode = mode;
			this.fileTypes = fileTypes;
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			foreach (IFileSource source in sources)
			{
				if (!this.CanOpenAny(source, fileTypes))
					continue;

				this.FileSources.Add(source);
			}

			this.IsOpen = true;

			this.FileSource = this.FileSources.Count > 0 ? this.FileSources[0] : null;

			if (this.mode == Modes.Save)
			{
				this.FileName = "New " + fileTypes[0].Name;

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

		public event DrawerEvent Close;
		public event PropertyChangedEventHandler PropertyChanged;

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

		public string FilePath { get; private set; }
		public bool AdvancedMode { get; private set; }
		public bool UseFileBrowser { get; set; }

		public ObservableCollection<IFileSource> FileSources { get; private set; } = new ObservableCollection<IFileSource>();
		public ObservableCollection<EntryWrapper> Entries { get; private set; } = new ObservableCollection<EntryWrapper>();
		public EntryWrapper Selected { get; set; }

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

		public string FileName
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
				return "." + this.fileTypes[0].Extension;
			}
		}

		public IFileSource FileSource
		{
			get
			{
				return this.fileSource;
			}
			set
			{
				this.fileSource = value;
				this.currentPath.Clear();
				this.CurrentDir = this.FileSource.GetDefaultDirectory(this.fileTypes);
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

		public IFileSource.IDirectory CurrentDir
		{
			get
			{
				if (this.currentPath.Count <= 0)
					return null;

				return this.currentPath.Peek();
			}
			private set
			{
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
					return this.fileTypes[0].SupportsAdvancedMode;
				}
			}
		}

		private async Task UpdateEntries()
		{
			IEnumerable<IFileSource.IEntry> entries = await this.FileSource.GetEntries(this.CurrentDir, this.fileTypes, this.IsFlattened);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.Entries.Clear();

				foreach (IFileSource.IEntry entry in entries)
				{
					this.Entries.Add(new EntryWrapper(entry, this));
				}
			});
		}

		private bool CanOpenAny(IFileSource source, FileType[] fileTypes)
		{
			foreach (FileType type in fileTypes)
			{
				if (source.CanOpen(type))
				{
					return true;
				}
			}

			return false;
		}

		private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

		private void OnGoUpClicked(object sender, RoutedEventArgs e)
		{
			this.currentPath.Pop();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanGoUp)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentPath)));
			Task.Run(this.UpdateEntries);
		}

		private void OnSelectClicked(object sender, RoutedEventArgs e)
		{
			if (!this.CanSelect)
				return;

			if (this.mode == Modes.Load)
			{
				if (this.Selected.Entry is IFileSource.IFile file)
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

			public string Name
			{
				get => this.Entry.Name;
			}

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
					if (this.View.mode == Modes.Save)
						return this.Entry is IFileSource.IDirectory;

					return true;
				}
			}

			public bool SupportsAdvanced
			{
				get
				{
					if (this.Entry is IFileSource.IFile file && file.Type != null)
					{
						return file.Type.SupportsAdvancedMode;
					}

					return false;
				}
			}

			public string Directory
			{
				get
				{
					string relativePath = this.Entry.Path.Replace(this.View.CurrentDir.Path, string.Empty);

					if (relativePath.StartsWith('\\'))
						relativePath = relativePath.Substring(2);

					string dirName = Path.GetDirectoryName(relativePath);
					return dirName;
				}
			}
		}
	}
}
