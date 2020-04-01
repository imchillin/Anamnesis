// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System.Xml.Serialization;

	[XmlRoot("Settings")]
	public struct OffsetsRoot
	{
		public string LastUpdated { get; set; }
		public string AoBOffset { get; set; }
		public string GposeOffset { get; set; }
		public string GposeEntityOffset { get; set; }
		public string GposeCheckOffset { get; set; }
		public string GposeCheck2Offset { get; set; }
		public string CameraOffset { get; set; }
		public string TimeOffset { get; set; }
		public string WeatherOffset { get; set; }
		public string TerritoryOffset { get; set; }
		public string TargetOffset { get; set; }
		public string GposeFilters { get; set; }
		public string MusicOffset { get; set; }
		public string SkeletonOffset { get; set; }
		public string SkeletonOffset2 { get; set; }
		public string SkeletonOffset3 { get; set; }
		public string PhysicsOffset { get; set; }
		public string PhysicsOffset2 { get; set; }
		public string CharacterRenderOffset { get; set; }
		public string CharacterRenderOffset2 { get; set; }
		public Character Character { get; set; }
		public Position Position { get; set; }
	}
}
