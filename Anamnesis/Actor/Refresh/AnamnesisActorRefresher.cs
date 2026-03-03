// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Core;
using Anamnesis.Core.Extensions;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using RemoteController.Drivers;
using RemoteController.Interop.Types;
using RemoteController.IPC;
using Serilog;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using static Anamnesis.Files.CharacterFile;
using CharChangeType = Anamnesis.Files.CharacterFile.CharFileDiff.ChangeType;

public enum RedrawStage
{
	Before,
	After,
}

public class AnamnesisActorRefresher : IActorRefresher
{
	private const int ACTOR_REFRESH_TIMEOUT_MS = 1000;

	public delegate void RedrawEvent(ObjectHandle<ActorMemory> obj, RedrawStage stage);

	public event RedrawEvent? OnRedraw;

	public RefreshBlockedReason GetRefreshAvailability(ActorMemory actor)
	{
		if (PoseService.Instance.IsEnabled)
			return RefreshBlockedReason.PoseEnabled;

		// Ana can't refresh world frozen actors
		if (PoseService.Instance.FreezeWorldState)
			return RefreshBlockedReason.WorldFrozen;

		return RefreshBlockedReason.None;
	}

	public async Task RefreshActor(ActorMemory actor, bool forceReload = false)
	{
		int actorIndex = ActorService.Instance.ObjectTable.GetIndexOf(actor.Address);
		if (actorIndex == -1)
			return;

		var handle = ActorService.Instance.ObjectTable.Get<ActorMemory>(actor.Address);
		if (handle == null)
			return;

		// Capture current state annd compute change diff
		var currentSnapshot = CaptureSnapshot(handle);
		var diff = ComputeChangeset(actor, currentSnapshot);
		Log.Verbose($"Computed character file diff for redraw: {diff}");

		if (!forceReload && !diff.HasChanges)
		{
			Log.Verbose("No appearance changes detected, skipping actor redraw.");
			return;
		}

		bool doPartialRedraw = actor.IsHuman && !diff.Changes.HasFlagUnsafe(CharChangeType.Base) && !forceReload;
		var redrawType = doPartialRedraw ? RedrawType.Partial : RedrawType.Full;

		var (payload, length) = PackRedrawPayload(actor, actorIndex, redrawType, diff);
		try
		{
			this.OnRedraw?.Invoke(handle, RedrawStage.Before);

			byte[] response = await ExecuteRedraw(actor, redrawType, payload, length);
			if (response.Length > 0 && response[0] == 1)
			{
				UpdateSnapshot(actor, currentSnapshot); // Success, update the actor's snapshot
			}
			else if (doPartialRedraw)
			{
				Log.Warning($"Partial redraw failed on actor 0x{actor.Address:X}. Attempting full redraw as fallback...");
				payload[0] = (byte)RedrawType.Full;
				response = await ExecuteRedraw(actor, RedrawType.Full, payload, Unsafe.SizeOf<RedrawHeader>());
				if (response.Length > 0 && response[0] == 1)
				{
					Log.Information("Successful fallback redraw attempt.");
					UpdateSnapshot(actor, currentSnapshot); // Success, update the actor's snapshot
				}
				else
				{
					Log.Error($"Both partial and full redraw attempts failed on actor: 0x{actor.Address:X}");
				}
			}
			else
			{
				Log.Error($"Full actor redraw failed on actor: 0x{actor.Address:X}");
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(payload);
			this.OnRedraw?.Invoke(handle, RedrawStage.After);
		}
	}

	private static CharacterFile CaptureSnapshot(ObjectHandle<ActorMemory> actor)
	{
		var snapshot = new CharacterFile();
		snapshot.WriteToFile(actor, CharacterFile.SaveModes.All);
		Log.Verbose($"Captured appearance snapshot for actor at 0x{actor.Address:X}");
		return snapshot;
	}

	private static CharacterFile.CharFileDiff ComputeChangeset(ActorMemory actor, CharacterFile currentSnapshot)
	{
		if (actor.LastAppearanceSnapshot == null)
			return new CharFileDiff { Changes = CharChangeType.All };

		return actor.LastAppearanceSnapshot.CompareTo(currentSnapshot, CharacterFile.SaveModes.All);
	}

	private static void UpdateSnapshot(ActorMemory actor, CharacterFile snapshot)
	{
		actor.LastAppearanceSnapshot = snapshot;
		Log.Verbose($"Updated appearance snapshot for actor at 0x{actor.Address:X}");
	}

	private static (byte[] Buffer, int Length) PackRedrawPayload(ActorMemory actor, int index, RedrawType type, CharFileDiff diff)
	{
		int maxSize = Unsafe.SizeOf<RedrawHeader>() + 1 + (Unsafe.SizeOf<WeaponModelId>() * 2) + 4 + HumanDrawData.SIZE;
		byte[] buffer = ArrayPool<byte>.Shared.Rent(maxSize);
		var writer = new BufferWriter(buffer);

		writer.Write(new RedrawHeader { Type = type, ObjectIndex = index });

		if (type == RedrawType.Partial)
		{
			RedrawFlags flags = RedrawFlags.None;
			if (diff.Changes.HasFlagUnsafe(CharChangeType.Appearance) || diff.Changes.HasFlagUnsafe(CharChangeType.Equipment))
				flags |= RedrawFlags.Appearance;

			if (diff.Changes.HasFlagUnsafe(CharChangeType.Weapon))
				flags |= RedrawFlags.Weapons;

			if (diff.Changes.HasFlagUnsafe(CharChangeType.Facewear))
				flags |= RedrawFlags.Facewear;

			writer.WriteByte((byte)flags);

			if (flags.HasFlag(RedrawFlags.Weapons))
			{
				writer.Write(actor.DrawData.MainHand?.WeaponModelId ?? default);
				writer.Write(actor.DrawData.OffHand?.WeaponModelId ?? default);
			}

			if (flags.HasFlag(RedrawFlags.Facewear))
			{
				writer.Write<ushort>(actor.DrawData.Glasses?.GlassesId ?? 0);
			}

			if (flags.HasFlag(RedrawFlags.Appearance))
			{
				writer.WriteSpan(actor.BuildDrawData().AsSpan());
			}
		}

		return (buffer, writer.Position);
	}

	private static async Task<byte[]> ExecuteRedraw(ActorMemory actor, RedrawType type, byte[] payload, int payloadLength)
	{
		if (SettingsService.Current.EnableNpcHack && type == RedrawType.Full && actor.ObjectKind == ObjectTypes.Player)
		{
			actor.ObjectKind = ObjectTypes.EventNpc;
			var res = ControllerService.Instance.SendDriverCommandRaw(DriverCommand.RedrawActor, payload.AsSpan(0, payloadLength), ACTOR_REFRESH_TIMEOUT_MS);
			actor.ObjectKind = ObjectTypes.Player;
			return res;
		}

		return ControllerService.Instance.SendDriverCommandRaw(DriverCommand.RedrawActor, payload.AsSpan(0, payloadLength), ACTOR_REFRESH_TIMEOUT_MS);
	}
}
