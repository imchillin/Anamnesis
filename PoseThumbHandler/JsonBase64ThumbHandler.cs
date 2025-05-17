using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PoseFileCOMHandler {

	[ComVisible(true)]
	[DisplayName("Pose File Thumbnail Handler")]						// Friendly name in regasm logs, etc.
	[COMServerAssociation(AssociationType.ClassOfExtension, ".pose")]	// Says “this COM server handles .pose”
	[Guid("3908774c-cd56-4e9b-9ce2-871a978652a6")]						// The ShellEx thumbnail‐handler key
	public class JsonBase64ThumbHandler : SharpThumbnailHandler {

		[HandlerSubkey(false, "e357fccd-a995-4576-b01f-234630154e96")]
		public static readonly string ThumnailShellExSubKey = "";

		/// <summary>
		/// Explorer asks for a square thumbnail of up to `width` pixels.
		/// </summary>
		/// <param name="width">Max width and height of the thumbnail in pixels.</param>
		/// <returns>A Bitmap at or below that size, or null.</returns>
		protected override Bitmap GetThumbnailImage(uint width) {
			Debug.WriteLine("COM: Entered GetThumbnailImage");
			string base64 = null;

			// Using jsonReader to Stream the Json.
			using (var reader = new StreamReader(this.SelectedItemStream))
			using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader)) {
				while (jsonReader.Read()) {
					if (jsonReader.TokenType == Newtonsoft.Json.JsonToken.PropertyName &&
						(string)jsonReader.Value == "Base64Image") {
						jsonReader.Read();
						base64 = jsonReader.Value as string;
						Debug.WriteLine($"Base64 string length: {base64.Length}, first 40: {base64.Substring(0, 40)}");
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(base64)) {
				Debug.WriteLine("COM: Is Null");
				return null;
			}

			int comma = base64.IndexOf(',');
			if (comma >= 0) base64 = base64.Substring(comma + 1);

			byte[] bytes;
			try {
				bytes = Convert.FromBase64String(base64);
			} catch {
				return null;
			}

			using var ms = new MemoryStream(bytes);
			using var img = Image.FromStream(ms);

			// calculate scaled size
			double aspect = (double)img.Width / img.Height;
			int thumbW, thumbH;
			if (aspect >= 1) {
				thumbW = (int)width;
				thumbH = (int)(width / aspect);
			} else {
				thumbH = (int)width;
				thumbW = (int)(width * aspect);
			}

			var thumb = new Bitmap(thumbW, thumbH);
			using (var g = Graphics.FromImage(thumb))
				g.DrawImage(img, 0, 0, thumbW, thumbH);

			return thumb;
		}

	}
}