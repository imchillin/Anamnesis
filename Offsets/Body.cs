// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;
	using System.Xml.Serialization;

	[Serializable]
	public struct Body
	{
		[XmlAttribute("Base")]
		public string Base { get; set; }

		public Position Position { get; set; }
		public Bones Bones { get; set; }
		public Vector3 Bust { get; set; }
		public string Height { get; set; }
		public string Wetness { get; set; }
		public string SWetness { get; set; }
		public Vector3 Scale { get; set; }
		public string MuscleTone { get; set; }
		public string TailSize { get; set; }
	}
}
