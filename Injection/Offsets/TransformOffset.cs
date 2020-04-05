// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;
	using System.Windows.Media.Media3D;

	[Serializable]
	public class TransformOffset
	{
		public Offset<Vector3D> Position { get; internal set; }
		public Offset<Quaternion> Rotation { get; internal set; }
		public Offset<Vector3D> Scale { get; internal set; }
	}
}
