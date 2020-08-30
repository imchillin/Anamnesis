// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;

	internal class ActorTypesMarshaler : MarshalerBase<ActorTypes>
	{
		public ActorTypesMarshaler(params IMemoryOffset[] offsets)
			: base(offsets, 1)
		{
		}

		protected override ActorTypes Read(ref byte[] data)
		{
			return (ActorTypes)data[0];
		}

		protected override void Write(ActorTypes value, ref byte[] data)
		{
			data[0] = (byte)value;
		}
	}
}
