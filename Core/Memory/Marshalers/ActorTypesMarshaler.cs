// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class ActorTypesMarshaler : MarshalerBase<ActorTypes>
	{
		public ActorTypesMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 1)
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
