// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Anamnesis.GUI.Windows;
using Anamnesis.Services;
using Anamnesis.Windows;
using Microsoft.Win32;
using PropertyChanged;

/// <summary>
/// Interaction logic for FileMetaEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class FileMetaEditor : UserControl
{
	public FileMetaEditor(FileSystemInfo info, FileBase file)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.Info = info;
		this.File = file;

		if (file.Author == null)
		{
			file.Author = SettingsService.Current.DefaultAuthor;
		}
	}

	public FileSystemInfo Info { get; private set; }
	public FileBase File { get; private set; }

	public BitmapImage? ImageSource
	{
		get => this.File.ImageSource;
		set { }
	}

	public static void Show(FileSystemInfo info, FileBase file)
	{
		throw new NotImplementedException();
		/*Dialog dlg = new Dialog();
		dlg.TitleText.Text = info.Name;
		FileMetaEditor content = new FileMetaEditor(dlg, info, file);
		dlg.ContentArea.Content = content;
		dlg.ShowDialog();*/
	}

	private void OnImageBrowseClicked(object sender, RoutedEventArgs e)
	{
		string? fileDir = Path.GetDirectoryName(this.Info.FullName);

		if (fileDir == null)
			throw new Exception($"Failed to get file directory: {this.Info.FullName}");

		OpenFileDialog dlg = new OpenFileDialog();
		dlg.Filter = "Images|*.jpg;*.jpeg;*.png;";
		dlg.InitialDirectory = fileDir;
		dlg.Title = "Image";
		bool? result = dlg.ShowDialog();

		if (result != true)
			return;

		byte[] bytes = System.IO.File.ReadAllBytes(dlg.FileName);

		if (bytes.Length > 6 * 1024 * 1024)
		{
			GenericDialog.Show("image too big", "Error");
			return;
		}

		this.File.SetImage(bytes);
		this.ImageSource = null;
	}

	private void OnImageClipboardClicked(object sender, RoutedEventArgs e)
	{
		BitmapSource? src = Clipboard.GetImage();

		if (src == null)
			return;

		JpegBitmapEncoder encoder = new JpegBitmapEncoder();
		encoder.QualityLevel = 90;
		using MemoryStream stream = new MemoryStream();

		encoder.Frames.Add(BitmapFrame.Create(src));
		encoder.Save(stream);
		byte[] bytes = stream.ToArray();
		stream.Close();

		this.File.SetImage(bytes);
		this.ImageSource = null;
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		using FileStream stream = new FileStream(this.Info.FullName, FileMode.Create);
		this.File.Serialize(stream);
	}
}
