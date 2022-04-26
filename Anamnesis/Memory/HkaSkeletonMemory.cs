// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class HkaSkeletonMemory : MemoryBase
{
	[Bind(0x010, BindFlags.Pointer)] public Utf8String Name { get; set; }
	[Bind(0x018)] public ParentingArrayMemory? ParentIndices { get; set; }
	[Bind(0x028)] public BoneArrayMemory? Bones { get; set; }

	public class ParentingArrayMemory : HkaArrayMemory<short>
	{
		public override int ElementSize => 2;
	}

	public class BoneArrayMemory : HkaArrayMemory<HkaBone>
	{
		public override int ElementSize => 16;
	}

	public abstract class HkaArrayMemory<T> : ArrayMemory<T, int>
	{
		public override int AddressOffset => 0x000;
		public override int CountOffset => 0x008;
	}

	public class HkaBone : MemoryBase
	{
		[Bind(0x000, BindFlags.Pointer)] public Utf8String Name { get; set; }

		public override void SetAddress(IntPtr address)
		{
			base.SetAddress(address);
		}
	}
}
