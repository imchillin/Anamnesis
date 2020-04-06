// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using System;

	[Serializable]
	public class TransformOffset
	{
		public Offset<Vector> Position { get; internal set; }
		public Offset<Quaternion> Rotation { get; internal set; }
		public Offset<Vector> Scale { get; internal set; }
	}
}
