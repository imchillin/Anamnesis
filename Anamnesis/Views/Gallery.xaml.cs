// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Animation;
	using Anamnesis.Serialization;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for Gallery.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class Gallery : UserControl
	{
		private const int ImageDelay = 5000;

		private bool isImage1 = true;
		private bool isRunning = false;
		private bool skip = false;

		public Gallery()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			SettingsService.SettingsChanged += this.OnSettingsChanged;
			this.OnSettingsChanged(null, null);
		}

		public string? Image1Path { get; set; } = null;
		public string Image1Author { get; set; } = string.Empty;
		public string? Image2Path { get; set; } = null;
		public string Image2Author { get; set; } = string.Empty;

		private void OnSettingsChanged(object? sender, PropertyChangedEventArgs? e)
		{
			this.Visibility = SettingsService.Current.ShowGallery ? Visibility.Visible : Visibility.Hidden;

			if (SettingsService.Current.ShowGallery && !this.isRunning)
			{
				Task.Run(this.Run);
			}
		}

		private async Task Run()
		{
			if (this.isRunning)
				return;

			while (!this.IsVisible)
				await Task.Delay(500);

			if (!SettingsService.Current.ShowGallery)
				return;

			this.isRunning = true;
			Random rnd = new Random();

			this.skip = true;

			while (this.IsVisible)
			{
				List<Entry> entries = SerializerService.DeserializeFile<List<Entry>>("Data/Images.json");

				while (this.IsVisible && entries.Count > 0)
				{
					if (!SettingsService.Current.ShowGallery)
					{
						this.isRunning = false;
						return;
					}

					int delay = 0;
					while (!this.skip && delay < ImageDelay)
					{
						delay += 100;
						await Task.Delay(100);
					}

					int index = rnd.Next(entries.Count);
					await this.Show(entries[index], rnd);
					entries.RemoveAt(index);

					this.skip = false;
				}
			}

			this.isRunning = false;
		}

		private async Task Show(Entry entry, Random rnd)
		{
			if (entry.Url == null || entry.Author == null)
				return;

			bool valid = true;

			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(entry.Url);
				request.Method = "HEAD";
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode != HttpStatusCode.OK)
				{
					Log.Information($"Failed to get image from url: {entry.Url}: {response.StatusCode}");
					valid = false;
				}

				response.Close();
			}
			catch (Exception ex)
			{
				Log.Information($"Failed to get image from url: {entry.Url}: {ex.Message}");
				valid = false;
			}

			if (!valid)
				return;

			await Dispatch.MainThread();

			UIElement oldHost = this.isImage1 ? this.Image1Host : this.Image2Host;

			this.isImage1 = !this.isImage1;
			UIElement host;
			Image image;
			RotateTransform rotate;

			if (this.isImage1)
			{
				this.Image1Path = entry.Url;
				this.Image1Author = entry.Author;
				image = this.Image1;
				host = this.Image1Host;
				rotate = this.Image1Rotate;
			}
			else
			{
				this.Image2Path = entry.Url;
				this.Image2Author = entry.Author;
				image = this.Image2;
				host = this.Image2Host;
				rotate = this.Image2Rotate;
			}

			rotate.Angle = -5 + (rnd.NextDouble() * 10.0);

			while (image.Source != null && !image.Source.CanFreeze)
				await Task.Delay(100);

			await Dispatch.MainThread();

			host.Opacity = 0.0;

			Storyboard? sb = this.Resources[this.isImage1 ? "StoryboardImage1" : "StoryboardImage2"] as Storyboard;

			if (sb == null)
				throw new System.Exception("Missing gallery storyboard");

			sb.SpeedRatio = this.skip ? 10 : 1;
			sb.Begin();

			await Task.Delay(this.skip ? 200 : 2000);

			oldHost.Opacity = 0.0;
		}

		private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.skip = true;
		}

		public class Entry
		{
			public string? Url { get; set; }
			public string? Author { get; set; }
		}
	}
}
