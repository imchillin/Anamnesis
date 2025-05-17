// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.GUI.Windows;
using Anamnesis.Services;
using Microsoft.Win32;
using PropertyChanged;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

	public string? Tags
	{
		get => this.File.Tags != null ? string.Join(", ", this.File.Tags) : null;
		set => this.File.Tags = string.IsNullOrWhiteSpace(value)
			? null
			: value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
	}

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

	public static byte[] CompressImageToJpeg(byte[] originalImageBytes, int maxSize = 512, long jpegQuality = 80L)
	{
		using var inputStream = new MemoryStream(originalImageBytes);
		using var originalImage = System.Drawing.Image.FromStream(inputStream);

		// Scale down if needed (keeping aspect ratio)
		int newWidth = originalImage.Width;
		int newHeight = originalImage.Height;
		if (originalImage.Width > maxSize || originalImage.Height > maxSize)
		{
			double ratio = Math.Min((double)maxSize / originalImage.Width, (double)maxSize / originalImage.Height);
			newWidth = (int)(originalImage.Width * ratio);
			newHeight = (int)(originalImage.Height * ratio);
		}

		using var scaledImage = new Bitmap(originalImage, new System.Drawing.Size(newWidth, newHeight));

		// Set JPEG encoder and compression quality
		var jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid) ?? throw new InvalidOperationException("JPEG encoder not found on this system.");
		var encoderParams = new EncoderParameters(1);
		encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);

		using var outputStream = new MemoryStream();
		scaledImage.Save(outputStream, jpegCodec, encoderParams);
		return outputStream.ToArray();
	}

	private void OnImageBrowseClicked(object sender, RoutedEventArgs e)
	{
		string? fileDir = Path.GetDirectoryName(this.Info.FullName)
			?? throw new Exception($"Failed to get file directory: {this.Info.FullName}");

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

		// COMPRESS/RESIZE here!
		byte[] compressed = CompressImageToJpeg(bytes);

		this.File.SetImage(compressed);
		this.ImageSource = null;
	}

	private void OnImageClipboardClicked(object sender, RoutedEventArgs e)
	{
		BitmapSource? src = Clipboard.GetImage();
		if (src == null)
			return;

		// Convert BitmapSource to byte[] (PNG, because BitmapSource stuff)
		using var ms = new MemoryStream();
		var encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(src));
		encoder.Save(ms);
		byte[] pngBytes = ms.ToArray();

		// COMPRESS/RESIZE here!
		byte[] compressed = CompressImageToJpeg(pngBytes);

		this.File.SetImage(compressed);
		this.ImageSource = null;
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		using var stream = new FileStream(this.Info.FullName, FileMode.Create);
		this.File.Serialize(stream);
		this.dlg.Close();
	}
}
