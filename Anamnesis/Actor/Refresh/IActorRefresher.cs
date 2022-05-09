// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using System.Threading.Tasks;
using Anamnesis.Memory;

public interface IActorRefresher
{
	public bool CanRefresh(ActorMemory actor);
	public Task RefreshActor(ActorMemory actor);
}
