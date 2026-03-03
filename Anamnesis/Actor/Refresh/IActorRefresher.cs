// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using System.Threading.Tasks;
using Anamnesis.Memory;

/// <summary>
/// Specifies the result of checking whether an actor can be refreshed.
/// </summary>
public enum RefreshBlockedReason
{
	/// <summary>Refresh is allowed.</summary>
	None = 0,

	/// <summary>World positions are frozen.</summary>
	WorldFrozen,

	/// <summary>Posing mode is enabled.</summary>
	PoseEnabled,

	/// <summary>Actor is an overworld actor in GPose.</summary>
	OverworldInGpose,

	/// <summary>The refresher integration is not enabled in settings.</summary>
	IntegrationDisabled,
}

public interface IActorRefresher
{
	/// <summary>
	/// Determines whether the actor can be refreshed and provides a reason if not.
	/// </summary>
	/// <param name="actor">The actor to check.</param>
	/// <returns>
	/// <see cref="RefreshBlockedReason.None"/> if refresh is allowed;
	/// otherwise, the reason why refresh is blocked.
	/// </returns>
	RefreshBlockedReason GetRefreshAvailability(ActorMemory actor);

	/// <summary>
	/// Determines if the actor can be refreshed by this refresher.
	/// </summary>
	/// <param name="actor">The actor to check.</param>
	/// <returns>True if the actor can be refreshed, otherwise false.</returns>
	public virtual bool CanRefresh(ActorMemory actor) => this.GetRefreshAvailability(actor) == RefreshBlockedReason.None;

	/// <summary>
	/// Refreshes the actor.
	/// </summary>
	/// <remarks>
	/// It is assumed that a check was carried out
	/// with <see cref="CanRefresh(ActorMemory)"/> beforehand.
	/// </remarks>
	/// <param name="actor">The actor to refresh.</param>
	public Task RefreshActor(ActorMemory actor);
}
