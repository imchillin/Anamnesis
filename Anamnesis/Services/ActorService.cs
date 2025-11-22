// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

// TODO: Update documentation for ObjectTable as some of the functions were copied from ActorService
[AddINotifyPropertyChangedInterface]
public class ObjectTable : INotifyPropertyChanged, IDisposable
{
	private const int OBJECT_TABLE_SIZE = 819;

	private static readonly int s_objectTableSizeInBytes = OBJECT_TABLE_SIZE * IntPtr.Size;

	private readonly IntPtr[] objTable = new IntPtr[OBJECT_TABLE_SIZE];
	private readonly byte[] objTableBuffer;
	private readonly ReaderWriterLockSlim tableLock = new();
	private HashSet<IntPtr> objSet = [];

	public ObjectTable()
	{
		this.objTableBuffer = ArrayPool<byte>.Shared.Rent(s_objectTableSizeInBytes);
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event Action? TableChanged;

	public void Dispose()
	{
		ArrayPool<byte>.Shared.Return(this.objTableBuffer);
		this.tableLock.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Refreshes the actor object table by reading from game memory.
	/// </summary>
	/// <exception cref="Exception">
	/// Thrown if reading from memory at the given object table address fails.
	/// </exception>
	public void Refresh()
	{
		if (!MemoryService.Read(AddressService.ActorTable, this.objTableBuffer.AsSpan(0, s_objectTableSizeInBytes)))
			throw new Exception("Failed to read actor table from memory.");

		Span<IntPtr> newSpan = MemoryMarshal.Cast<byte, IntPtr>(this.objTableBuffer.AsSpan(0, s_objectTableSizeInBytes));
		bool hasChanged = false;

		this.tableLock.EnterUpgradeableReadLock();
		try
		{
			hasChanged = !this.objTable.AsSpan().SequenceEqual(newSpan);

			if (!hasChanged)
				return; // No changes detected, exit early.

			this.tableLock.EnterWriteLock();
			try
			{
				newSpan.CopyTo(this.objTable);
				this.objSet = this.objTable.ToHashSet();
			}
			finally
			{
				this.tableLock.ExitWriteLock();
			}
		}
		finally
		{
			this.tableLock.ExitUpgradeableReadLock();
		}

		TableChanged?.Invoke();
		Log.Verbose("Object table updated.");
	}

	/// <summary>
	/// Creates an object handle for the given address if found in the object table.
	/// </summary>
	/// <param name="address">The address of the object to search for.</param>
	/// <returns>A new <see cref="ObjectHandle"/> for the specified address, or null if not found.</returns>
	public ObjectHandle<T>? Get<T>(IntPtr address)
		where T : ActorBasicMemory, new()
	{
		if (!this.Contains(address))
			return null;

		return new ObjectHandle<T>(address, this);
	}

	/// <summary>
	/// Gets all objects from the object table.
	/// </summary>
	/// <remarks>
	/// This method can only return objects of type <see cref="ActorBasicMemory"/> as we cannot
	/// verify the type of each object in the object table without additional context.
	/// </remarks>
	/// <returns>
	/// A collection of all memory objects found in the object table.
	/// </returns>
	public List<ObjectHandle<ActorBasicMemory>> GetAll()
	{
		this.Refresh();

		List<ObjectHandle<ActorBasicMemory>> handles = [];

		foreach (var ptr in this.objTable)
		{
			if (ptr == IntPtr.Zero)
				continue;

			handles.Add(new ObjectHandle<ActorBasicMemory>(ptr, this));
		}

		return handles;
	}

	/// <summary>Gets the index of the actor in the actor table.</summary>
	/// <param name="ptr">The pointer to the actor.</param>
	/// <returns>The index of the actor in the actor table, or -1 if not found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetIndexOf(IntPtr ptr)
	{
		if (ptr == IntPtr.Zero)
			return -1;

		this.Refresh();

		this.tableLock.EnterReadLock();
		try
		{
			return Array.IndexOf(this.objTable, ptr);
		}
		finally
		{
			this.tableLock.ExitReadLock();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(IntPtr ptr)
	{
		this.tableLock.EnterReadLock();
		try
		{
			return this.objSet.Contains(ptr);
		}
		finally
		{
			this.tableLock.ExitReadLock();
		}
	}

	/// <summary>Determines if the actor is in the actor table.</summary>
	/// <param name="memObj">The memory of the target game memory object.</param>
	/// <returns>True if the actor is in the actor table, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(MemoryBase memObj) => this.Contains(memObj.Address);
}

// TODO: Create a ToString() function for the object handle to display essential information
[AddINotifyPropertyChangedInterface]
public class ObjectHandle<T> : INotifyPropertyChanged, IDisposable
	where T : ActorBasicMemory, new()
{
	private static readonly ConcurrentDictionary<(IntPtr, Type), CacheEntry> s_cache = [];
	private readonly ObjectTable table;
	private IntPtr ptr;
	private bool disposed = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="ObjectHandle{T}"/> class that
	/// represents an wrapped object in the game's object table.
	/// </summary>
	/// <param name="ptr">The address of the object.</param>
	/// <param name="table">A reference to the object table.</param>
	/// <exception cref="InvalidOperationException">
	/// Raised if the provided address is not part of the object table.
	/// </exception>
	public ObjectHandle(IntPtr ptr, ObjectTable table)
	{
		this.ptr = ptr;
		this.table = table;
		this.table.TableChanged += this.OnTableChanged;

		if (ptr == IntPtr.Zero || !this.table.Contains(ptr))
			throw new InvalidOperationException("Cannot create a handle for an object not part of the object table.");

		var cacheKey = (ptr, typeof(T));
		if (!s_cache.TryGetValue(cacheKey, out var entry))
		{
			try
			{
				var obj = new T();
				obj.SetAddress(ptr);
				s_cache.TryAdd(cacheKey, new CacheEntry(obj));
			}
			catch (Exception ex)
			{
				Log.Warning(ex, $"Failed to create basic actor memory object from address: {ptr}");
			}
		}
		else
		{
			Interlocked.Increment(ref entry.RefCount);
		}
	}

	/// <summary>
	/// Property changed event.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Occurs when the object represented by the <see cref="ObjectHandle{T}"/> is invalidated.
	/// </summary>
	/// <remarks>
	/// This event is triggered whenever the state of the object becomes invalid or requires revalidation.
	/// Use this event to handle scenarios where the object needs to be refreshed or updated.
	/// </remarks>
	public event Action<ObjectHandle<T>>? Invalidated;

	/// <summary>
	/// Gets the address represented by this handle.
	/// </summary>
	public IntPtr Address => this.ptr;

	/// <summary>
	/// Gets a value indicating whether this handle is still valid.
	/// </summary>
	public bool IsValid => !this.disposed && this.ptr != IntPtr.Zero && this.table.Contains(this.ptr);

	/// <summary>
	/// An unsafe reference to the underlying object.
	/// </summary>
	/// <remarks>
	/// This is intended only for UI data bindings and should not be used outside of that context.
	/// </remarks>
	public T? Unsafe => s_cache.TryGetValue((this.ptr, typeof(T)), out var entry) ? entry.Actor : null;

	/// <summary>Disposes resources used internally by the object handle.</summary>
	public void Dispose()
	{
		if (this.disposed)
			return;

		this.disposed = true;
		var cacheKey = (this.ptr, typeof(T));
		this.ptr = IntPtr.Zero;
		this.table.TableChanged -= this.OnTableChanged;

		if (s_cache.TryGetValue(cacheKey, out var entry))
		{
			int newCount = Interlocked.Decrement(ref entry.RefCount);
			if (newCount <= 0)
			{
				if (!entry.Actor.IsDisposed)
					entry.Actor.Dispose();

				s_cache.TryRemove(cacheKey, out _);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Unsafe)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsValid)));
				Invalidated?.Invoke(this);
			}
		}

		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Performs the specified action on the underlying object if the handle is valid.
	/// </summary>
	/// <param name="action">The action to perform.</param>
	public void Do(Action<T> action)
	{
		if (!this.IsValid)
			return;

		if (s_cache.TryGetValue((this.ptr, typeof(T)), out var entry))
			action(entry.Actor);
	}

	/// <summary>
	/// Performs the specified function on the underlying object if the handle is valid and returns the result.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="func">The function to perform.</param>
	/// <returns>The result of the function, or default if the handle is not valid.</returns>
	public TResult? Do<TResult>(Func<T, TResult> func)
	{
		if (!this.IsValid)
			return default;

		if (s_cache.TryGetValue((this.ptr, typeof(T)), out var entry))
			return func(entry.Actor);

		return default;
	}

	/// <summary>
	/// Performs the specified asynchronous action on the underlying object if the handle is valid.
	/// </summary>
	/// <param name="action">The asynchronous action to perform.</param>
	/// <returns>The task representing the asynchronous operation.</returns>
	public async Task DoAsync(Func<T, Task> action)
	{
		if (!this.IsValid)
			return;

		if (s_cache.TryGetValue((this.ptr, typeof(T)), out var entry))
			await action(entry.Actor);
	}

	/// <summary>
	/// Performs the specified asynchronous function on the underlying object if the handle is valid and returns the result.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="func">The asynchronous function to perform.</param>
	/// <returns>The result of the function, or default if the handle is not valid.</returns>
	public async Task<TResult?> DoAsync<TResult>(Func<T, Task<TResult>> func)
	{
		if (!this.IsValid)
			return default;

		if (s_cache.TryGetValue((this.ptr, typeof(T)), out var entry))
			return await func(entry.Actor);

		return default;
	}

	/// <summary>
	/// Emits an invalidation event if the underlying object is no longer valid.
	/// </summary>
	private void OnTableChanged()
	{
		if (!this.IsValid)
		{
			if (s_cache.TryRemove((this.ptr, typeof(T)), out var entry))
			{
				if (!entry.Actor.IsDisposed)
					entry.Actor.Dispose();
			}

			Invalidated?.Invoke(this);
		}
	}

	private sealed record class CacheEntry
	{
		public int RefCount;

		public CacheEntry(T actor)
		{
			this.Actor = actor;
			this.RefCount = 1;
		}

		public T Actor { get; init; }
	}
}

/// <summary>Service for managing and refreshing actors.</summary>
[AddINotifyPropertyChangedInterface]
public class ActorService : ServiceBase<ActorService>
{
	public const int GPOSE_INDEX_START = 200;
	public const int GPOSE_INDEX_END = 440;
	private const int TICK_DELAY = 16; // ms
	private const int OVERWORLD_PLAYER_INDEX = 0;
	private const int GPOSE_PLAYER_INDEX = 201;

	private readonly ObjectTable actorTable = new();

	/// <summary>
	/// Gets the instance of the actor object table.
	/// </summary>
	public ObjectTable ActorTable => this.actorTable;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance];

	/// <summary>Determines if the actor is in GPose.</summary>
	/// <param name="objectIndex">The index of the actor.</param>
	/// <returns>True if the actor is in GPose, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsGPoseActor(int objectIndex) => objectIndex >= GPOSE_INDEX_START && objectIndex < GPOSE_INDEX_END;

	/// <summary>Determines if the actor is in the overworld.</summary>
	/// <param name="objectIndex">The index of the actor.</param>
	/// <returns>True if the actor is in the overworld, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOverworldActor(int objectIndex) => !IsGPoseActor(objectIndex);

	/// <summary>Determines if the actor is the local overworld player.</summary>
	/// <param name="objectIndex">The index of the actor.</param>
	/// <returns>True if the actor is the local overworld player, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLocalOverworldPlayer(int objectIndex) => objectIndex == OVERWORLD_PLAYER_INDEX;

	/// <summary>Determines if the actor is the local GPose player.</summary>
	/// <param name="objectIndex">The index of the actor.</param>
	/// <returns>True if the actor is the local GPose player, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLocalGPosePlayer(int objectIndex) => objectIndex == GPOSE_PLAYER_INDEX;

	/// <summary>Determines if the actor is the local player.</summary>
	/// <param name="objectIndex">The index of the actor.</param>
	/// <returns>True if the actor is the local player, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLocalPlayer(int objectIndex) => IsLocalOverworldPlayer(objectIndex) || IsLocalGPosePlayer(objectIndex);

	/// <summary>Determines if the actor is in GPose.</summary>
	/// <param name="actorAddress">The address of the actor.</param>
	/// <returns>True if the actor is in GPose, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsGPoseActor(IntPtr actorAddress)
	{
		int objectIndex = this.actorTable.GetIndexOf(actorAddress);
		if (objectIndex == -1)
			return false;

		return this.IsGPoseActor(objectIndex);
	}

	/// <summary>Determines if the actor is in the overworld.</summary>
	/// <param name="actorAddress">The address of the actor.</param>
	/// <returns>True if the actor is in the overworld, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsOverworldActor(IntPtr actorAddress) => !this.IsGPoseActor(actorAddress);

	/// <summary>Determines if the actor is the local overworld player.</summary>
	/// <param name="actorAddress">The address of the actor.</param>
	/// <returns>True if the actor is the local overworld player, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsLocalOverworldPlayer(IntPtr actorAddress)
	{
		int objectIndex = this.actorTable.GetIndexOf(actorAddress);
		if (objectIndex == -1)
			return false;

		return this.IsLocalOverworldPlayer(objectIndex);
	}

	/// <summary>Determines if the actor is the local GPose player.</summary>
	/// <param name="actorAddress">The address of the actor.</param>
	/// <returns>True if the actor is the local GPose player, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsLocalGPosePlayer(IntPtr actorAddress)
	{
		int objectIndex = this.actorTable.GetIndexOf(actorAddress);
		if (objectIndex == -1)
			return false;

		return this.IsLocalGPosePlayer(objectIndex);
	}

	/// <summary>Determines if the actor is the local player.</summary>
	/// <param name="actorAddress">The address of the actor.</param>
	/// <returns>True if the actor is the local player, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsLocalPlayer(IntPtr actorAddress) => this.IsLocalOverworldPlayer(actorAddress) || this.IsLocalGPosePlayer(actorAddress);

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.Tick(this.CancellationToken));
		await base.OnStart();
	}

	/// <summary>
	/// A task that periodically refreshes the actor object table.
	/// </summary>
	private async Task Tick(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				if (GameService.Ready)
					this.actorTable.Refresh();

				await Task.Delay(TICK_DELAY, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
			}
		}
	}
}
