// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public interface ITransform
	{
		bool CanTranslate { get; }
		Vector Position { get; set; }

		bool CanRotate { get; }
		public Quaternion Rotation { get; set; }

		bool CanScale { get; }
		public Vector Scale { get; set; }
	}
}
