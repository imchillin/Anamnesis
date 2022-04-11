// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class DummyActor : ActorMemory
	{
		public DummyActor()
		{
			this.Address = (IntPtr)1;
			this.ObjectKind = ActorTypes.Player;
			this.Nickname = "Dummy Actor";
		}

		public override void Tick()
		{
		}

		public override void SetAddress(IntPtr address)
		{
		}
	}
}
