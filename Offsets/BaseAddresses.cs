// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;

	public enum BaseAddresses
	{
		None,
		GPose,
		Skeleton,
		Skeleton2,
		Skeleton3,
		Physics,
		Physics2,
		Camera,
		GPoseEntity,
	}

	#pragma warning disable SA1649
	public static class BaseAddressesExtensions
	{
		public static string GetOffset(this BaseAddresses address, OffsetsRoot root)
		{
			switch (address)
			{
				case BaseAddresses.None: return string.Empty;
				case BaseAddresses.GPose: return root.GposeOffset;
				case BaseAddresses.Skeleton: return root.SkeletonOffset;
				case BaseAddresses.Skeleton2: return root.SkeletonOffset2;
				case BaseAddresses.Skeleton3: return root.SkeletonOffset3;
				case BaseAddresses.Physics: return root.PhysicsOffset;
				case BaseAddresses.Physics2: return root.PhysicsOffset2;
				case BaseAddresses.Camera: return root.CameraOffset;
				case BaseAddresses.GPoseEntity: return root.GposeEntityOffset;
			}

			throw new Exception($"Unrecognized base address offset: {address}");
		}
	}
}
