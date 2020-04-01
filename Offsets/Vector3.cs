// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;
	using System.Xml.Serialization;

	[Serializable]
	public struct Vector3
	{
		[XmlAttribute("Base")]
		public string Base { get; set; }

		public string X { get; set; }
		public string Y { get; set; }
		public string Z { get; set; }
	}
}
