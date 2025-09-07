// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.GUI.Dialogs;
using Anamnesis.GUI.Windows;
using Anamnesis.Services;
using Microsoft.Win32;
using PropertyChanged;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

/// <summary>
/// Interaction logic for FileMetaEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class FileMetaEditor : UserControl
{
	private readonly Dialog dlg;

	public FileMetaEditor(Dialog dlg, FileSystemInfo info, FileBase file)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.Info = info;
		this.File = file;
		this.dlg = dlg;

		file.Author ??= SettingsService.Current.DefaultAuthor;
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
		var dlg = new Dialog();
		dlg.TitleText.Text = info.Name;
		var content = new FileMetaEditor(dlg, info, file);
		dlg.ContentArea.Content = content;
		dlg.ShowDialog();
	}

	private void OnImageBrowseClicked(object sender, RoutedEventArgs e)
	{
		string? fileDir = Path.GetDirectoryName(this.Info.FullName);

#pragma warning disable IDE0270
		if (fileDir == null)
			throw new Exception($"Failed to get file directory: {this.Info.FullName}");
#pragma warning restore IDE0270

		var dlg = new OpenFileDialog
		{
			Filter = "Images|*.jpg;*.jpeg;*.png;",
			InitialDirectory = fileDir,
			Title = "Image",
		};

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

		var encoder = new JpegBitmapEncoder
		{
			QualityLevel = 90,
		};

		using var stream = new MemoryStream();

		encoder.Frames.Add(BitmapFrame.Create(src));
		encoder.Save(stream);
		byte[] bytes = stream.ToArray();
		stream.Close();

		this.File.SetImage(bytes);
		this.ImageSource = null;
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		using var stream = new FileStream(this.Info.FullName, FileMode.Create);
		this.File.Serialize(stream);
		this.dlg.Close();
	}
}
