// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class DummyActor : ActorMemory
{
	public DummyActor(int id)
	{
		this.Address = (IntPtr)id;
		this.ObjectKind = ActorTypes.Player;
		this.Nickname = "Dummy Actor " + id;
	}

	public override bool IsGPoseActor => false;

	public override void Tick()
	{
	}

	public override void SetAddress(IntPtr address)
	{
	}
}
