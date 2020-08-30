// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;

	internal class FlagMarshaler : MarshalerBase<Flag>
	{
		private FlagOffset? flagOffset;

		public FlagMarshaler(params IMemoryOffset[] offsets)
			: base(offsets, GetMemoryLength(offsets))
		{
		}

		protected FlagOffset FlagOffset
		{
			get
			{
				if (this.flagOffset == null)
					this.flagOffset = GetFlagOffset(this.offsets);

				return this.flagOffset;
			}
		}

		protected override Flag Read(ref byte[] data)
		{
			if (Equals(this.FlagOffset.On, data))
				return Flag.Enabled;

			return Flag.Disabled;
		}

		protected override void Write(Flag value, ref byte[] data)
		{
			if (value.IsEnabled)
			{
				Array.Copy(this.FlagOffset.On, data, (int)this.FlagOffset.On.Length);
			}
			else
			{
				Array.Copy(this.FlagOffset.Off, data, (int)this.FlagOffset.Off.Length);
			}
		}

		private static ulong GetMemoryLength(IMemoryOffset[] offsets)
		{
			FlagOffset offset = GetFlagOffset(offsets);
			return (ulong)offset.On.Length;
		}

		private static FlagOffset GetFlagOffset(IMemoryOffset[] offsets)
		{
			foreach (IMemoryOffset offset in offsets)
			{
				if (offset is FlagOffset flagOffset)
				{
					return flagOffset;
				}
			}

			throw new Exception("No Flag offset in offsets array");
		}
	}
}
