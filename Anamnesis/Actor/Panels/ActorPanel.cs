// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;

public abstract class ActorPanelBase : PanelBase
{
	protected ActorPanelBase(IPanelGroupHost host)
		: base(host)
	{
	}

	public ActorMemory? Actor
	{
		get
		{
			if (this.DataContext is ActorMemory actor)
				return actor;

			if (this.DataContext is PinnedActor pinnedActor)
				return pinnedActor.GetMemory();

			return null;
		}
	}
}
