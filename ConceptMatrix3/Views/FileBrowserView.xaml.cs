// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using ConceptMatrix;
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

		public FileBrowserView(List<IFileSource> sources, FileType[] fileTypes)
		{
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
		}

		public event DrawerEvent Close;
		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsOpen
		{
			get;
			private set;
		}

		public string FilePath { get; private set; }
		public bool AdvancedLoad { get; private set; }
		public bool UseFileBrowser { get; set; }

		public ObservableCollection<IFileSource> FileSources { get; private set; } = new ObservableCollection<IFileSource>();
		public ObservableCollection<EntryWrapper> Entries { get; private set; } = new ObservableCollection<EntryWrapper>();
		public EntryWrapper Selected { get; set; }

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

		private async Task UpdateEntries()
		{
			IEnumerable<IFileSource.IEntry> entries = await this.FileSource.GetEntries(this.CurrentDir, this.fileTypes);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.Entries.Clear();

				foreach (IFileSource.IEntry entry in entries)
				{
					this.Entries.Add(new EntryWrapper(entry));
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
			if (this.Selected != null && this.Selected.Entry is IFileSource.IFile file)
			{
				this.FilePath = file.Path;
				this.AdvancedLoad = false;
				this.CloseDrawer();
			}
		}

		private void OnAdvancedClicked(object sender, RoutedEventArgs e)
		{
			if (this.Selected != null && this.Selected.Entry is IFileSource.IFile file)
			{
				this.FilePath = file.Path;
				this.AdvancedLoad = true;
				this.CloseDrawer();
			}
		}

		private void OnBrowseClicked(object sender, RoutedEventArgs e)
		{
			this.UseFileBrowser = true;
			this.CloseDrawer();
		}

		private void CloseDrawer()
		{
			this.IsOpen = false;
			this.Close?.Invoke();
		}

		public class EntryWrapper
		{
			public readonly IFileSource.IEntry Entry;

			public EntryWrapper(IFileSource.IEntry entry)
			{
				this.Entry = entry;
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
				get => this.Entry is IFileSource.IFile;
			}

			public bool SupportsAdvanced
			{
				get
				{
					if (this.Entry is IFileSource.IFile file && file.Type != null)
					{
						return file.Type.CanAdvancedLoad;
					}

					return false;
				}
			}
		}
	}
}
