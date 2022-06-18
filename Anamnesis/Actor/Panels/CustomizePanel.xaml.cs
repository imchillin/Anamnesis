// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;

public partial class CustomizePanel : PanelBase
{
	public CustomizePanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
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
