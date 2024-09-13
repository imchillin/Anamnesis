// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Numerics;

public interface ITransform
{
	bool CanTranslate { get; }
	Vector3 Position { get; set; }

	bool CanRotate { get; }
	public Quaternion Rotation { get; set; }

	bool CanScale { get; }
	bool CanLinkScale { get; }
	bool ScaleLinked { get; }
	public Vector3 Scale { get; set; }
}
