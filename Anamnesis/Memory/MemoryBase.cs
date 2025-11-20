// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core.Extensions;
using Anamnesis.Services;
using Microsoft.Extensions.ObjectPool;
using PropertyChanged;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

/// <summary>
/// Provides data for the PropertyChanged event, including additional change context information
/// of type <see cref="PropertyChange"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MemObjPropertyChangedEventArgs"/> class.
/// </remarks>
/// <param name="propertyName">The name of the property that changed.</param>
/// <param name="context">The context of the property change.</param>
public class MemObjPropertyChangedEventArgs(string propertyName, PropertyChange context) : PropertyChangedEventArgs(propertyName)
{
	/// <summary>Gets the context of the property change.</summary>
	public PropertyChange Context { get; } = context;
}

/// <summary>
/// Represents the base class for memory operations, providing mechanisms for reading
/// from and writing to memory, synchronizing memory states, and handling and propagating
/// property changes.
/// </summary>
[AddINotifyPropertyChangedInterface]
public abstract class MemoryBase : INotifyPropertyChanged, IDisposable
{
	/// <summary>Dictionary of the object's property bindings.</summary>
	public readonly Dictionary<string, PropertyBindInfo> Binds = new();

	/// <summary>
	/// A thread-local variable that determines whether property notifications should be suppressed.
	/// </summary>
	/// <remarks>
	/// This is intended to be used to update properties while synchronization is taking place on
	/// a thread, without supressing property notifications for the rest of the application.
	/// </remarks>
	protected readonly ThreadLocal<bool> SuppressPropNotifications = new(() => false);

	/// <summary>List of child memory objects.</summary>
	protected readonly List<MemoryBase> Children = new();

	private static readonly ObjectPool<Queue<MemoryBase>> s_queuePool = ObjectPool.Create<Queue<MemoryBase>>();
	private static readonly ObjectPool<Stack<MemoryBase>> s_stackPool = ObjectPool.Create<Stack<MemoryBase>>();

	/// <summary>Lock object for thread synchronization.</summary>
	private readonly Lock lockObject = new();

	/// <summary>
	/// Represents the parent memory object, if any.
	/// </summary>
	/// <note>
	/// The private field is used to avoid property access overhead in performance-critical paths.
	/// </note>
	private MemoryBase? parent;

	/// <summary>
	/// Provides bind information for the parent memory object, if any.
	/// </summary>
	/// /// <note>
	/// The private field is used to avoid property access overhead in performance-critical paths.
	/// </note>
	private BindInfo? parentBind;

	/// <summary>A collection of delayed binds to be written later.</summary>
	/// <remarks>
	/// Stores binds that could not be written to memory immediately
	/// due to ongoing memory reads.
	/// </remarks>
	private ConcurrentQueue<BindInfo> delayedBinds = new();

	private int enableReading = 1;
	private int enableWriting = 1;
	private int pauseSynchronization = 0;
	private int isSynchronizing = 0;
	private bool disposed = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryBase"/> class.
	/// </summary>
	public MemoryBase()
	{
		// Enumerate through the class' properties to retrieve memory offsets.
		foreach (PropertyInfo property in this.GetType().GetProperties())
		{
			BindAttribute? attribute = property.GetCustomAttribute<BindAttribute>();
			if (attribute == null)
				continue;

			this.Binds.Add(property.Name, new PropertyBindInfo(this, property, attribute));
		}
	}

	/// <summary>
	/// Finalizes an instance of the <see cref="MemoryBase"/> class.
	/// </summary>
	/// <remarks>
	/// This takes care of releasing unmanaged resources.
	/// </remarks>
	~MemoryBase()
	{
		this.Dispose(false);
	}

	/// <summary>Event triggered when a property value changes.</summary>
	/// <remarks>
	/// It is important that the property change event is handled internally
	/// before it is invoked to notify the rest of the application.
	/// </remarks>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Gets or sets the object's memory address.</summary>
	public IntPtr Address { get; set; } = IntPtr.Zero;

	/// <summary>Gets a value indicating whether the object has been disposed.</summary>
	public bool IsDisposed => this.disposed;

	/// <summary>Gets or sets the parent memory object.</summary>
	[DoNotNotify]
	public MemoryBase? Parent
	{
		get => this.parent;
		set => this.parent = value;
	}

	/// <summary>Gets or sets the parent's bind information.</summary>
	[DoNotNotify]
	public BindInfo? ParentBind
	{
		get => this.parentBind;
		set => this.parentBind = value;
	}

	/// <summary>Gets or sets a value indicating whether reading is enabled.</summary>
	[DoNotNotify]
	public bool EnableReading
	{
		get => Interlocked.CompareExchange(ref this.enableReading, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.enableReading, value ? 1 : 0);
	}

	/// <summary>Gets or sets a value indicating whether writing is enabled.</summary>
	[DoNotNotify]
	public bool EnableWriting
	{
		get => Interlocked.CompareExchange(ref this.enableWriting, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.enableWriting, value ? 1 : 0);
	}

	/// <summary>Gets or sets a value indicating whether synchronization is paused.</summary>
	/// <remarks>
	/// Use this property if you want to pause synchronization of the object's memory state.
	/// Unlike <see cref="EnableReading"/>, this property does not claim the memory objects' locks.
	/// </remarks>
	[DoNotNotify]
	public bool PauseSynchronization
	{
		get => Interlocked.CompareExchange(ref this.pauseSynchronization, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.pauseSynchronization, value ? 1 : 0);
	}

	/// <summary>
	/// Gets the logger instance for the <see cref="MemoryBase"/> class.
	/// </summary>
	protected static ILogger Log => Serilog.Log.ForContext<MemoryBase>();

	/// <summary>
	/// Gets or sets a value indicating whether synchronization is in progress.
	/// </summary>
	[DoNotNotify]
	protected bool IsSynchronizing
	{
		get => Interlocked.CompareExchange(ref this.isSynchronizing, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.isSynchronizing, value ? 1 : 0);
	}

	/// <summary>Raises the property changed event.</summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <remarks>
	/// This is a custom event invoker that is used by Fody to notify of property changes.
	/// It can  be used to manually raise property changed events in cases where the property
	/// does not normally (e.g. the property has DoNotNotify attribute) or is not bound to a
	/// memory address.
	/// </remarks>
	public void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		if (!this.Binds.TryGetValue(propertyName, out PropertyBindInfo? bind))
		{
			// If the bind is not found, assume it is not bound to a memory address.
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return;
		}

		// Suppress property notifications
		// Ignore property changes which arise from sync with game memory and not from Anamnesis
		if (this.SuppressPropNotifications.Value)
			return;

		this.OnSelfPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Sets the object's memory address.
	/// </summary>
	/// <param name="address">The memory address.</param>
	/// <remarks>
	/// This method triggers a synchronization of the object's memory state.
	/// If a synchronization is already in progress, the request is cancelled.
	/// </remarks>
	public virtual void SetAddress(IntPtr address)
	{
		if (this.Address == address)
			return;

		this.Address = address;

		this.Synchronize();
	}

	/// <summary>Disposes the object and its children.</summary>
	/// <remarks>
	/// This method releases managed and unmanaged resources.
	/// </remarks>
	public virtual void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Synchronizes the memory object with the current memory state.
	/// </summary>
	/// <remarks>
	/// If a synchronization is already in progress, the request is cancelled.
	/// </remarks>
	public virtual void Synchronize()
	{
		if (this.Address == IntPtr.Zero)
			return;

		// A sync is already in progress, cancel request.
		if (Interlocked.CompareExchange(ref this.isSynchronizing, 0, 0) == 1)
			return;

		// If synchronization is paused, cancel request.
		if (Interlocked.CompareExchange(ref this.pauseSynchronization, 0, 0) == 1)
			return;

		this.ClaimLocks();
		try
		{
			this.SynchronizeInternal();
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to refresh {this.GetType().Name} from memory address: {this.Address}", ex);
		}
		finally
		{
			// Write delayed binds to memory after synchronization.
			// This ensures that writes are not blocked by ongoing reads.
			this.WriteDelayedBindsInternal();

			this.ReleaseLocks();
		}
	}

	/// <summary>Writes delayed binds to memory.</summary>
	/// <remarks>
	/// Used to write binds that could not be written immediately due to
	/// ongoing memory reads.
	/// </remarks>
	public virtual void WriteDelayedBinds()
	{
		this.ClaimLocks();
		try
		{
			this.WriteDelayedBindsInternal();
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to write delayed binds for {this.GetType().Name}", ex);
		}
		finally
		{
			// Sync object immediately after writing to memory to
			// ensure that the latest state is propagated to the application.
			this.SynchronizeInternal();

			this.ReleaseLocks();
		}
	}

	/// <summary>Gets the address of a property by its name.</summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <returns>The address of the property.</returns>
	/// <exception cref="KeyNotFoundException">
	/// Thrown when the property is not found with the given property name.
	/// </exception>
	public IntPtr GetAddressOfProperty(string propertyName)
	{
		if (!this.Binds.TryGetValue(propertyName, out PropertyBindInfo? bind))
			throw new KeyNotFoundException($"Failed to find bound property with name \"{propertyName}\".");

		return bind.GetAddress();
	}

	/// <summary>Claims locks on the specified memory object.</summary>
	/// <param name="memory">The memory object.</param>
	/// <remarks>
	/// The lock claim covers the memory object and all its descendants.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void ClaimLocksOn(MemoryBase memory)
	{
		memory.ClaimLocks();
	}

	/// <summary>Releases locks on the specified memory object.</summary>
	/// <param name="memory">The memory object.</param>
	/// <remarks>
	/// The lock release covers the memory object and all its descendants.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void ReleaseLocksOn(MemoryBase memory)
	{
		memory.ReleaseLocks();
	}

	/// <summary>Disposes the object and its descendants.</summary>
	/// <param name="managedResources">
	/// Indicates whether managed resources should be disposed.
	/// </param>
	protected virtual void Dispose(bool managedResources)
	{
		if (!this.disposed)
		{
			if (managedResources)
			{
				/* Dispose managed resources here */
				this.parent?.Children.Remove(this);

				this.Address = IntPtr.Zero;
				this.parent = null;
				this.parentBind = null;

				for (int i = this.Children.Count - 1; i >= 0; i--)
				{
					this.Children[i].Dispose();
				}

				this.Children.Clear();
				this.delayedBinds.Clear();
				this.SuppressPropNotifications.Dispose();
			}

			/* Dispose unmanaged resources here if any */

			this.disposed = true;
		}
	}

	/// <summary>Checks if a bound property's value is frozen.</summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <returns>True if the property is frozen; otherwise, false.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when the property is not found.</exception>
	protected bool IsFrozen(string propertyName)
	{
		if (!this.Binds.TryGetValue(propertyName, out PropertyBindInfo? bind))
			throw new KeyNotFoundException($"Failed to find bound property with name \"{propertyName}\".");

		return bind.FreezeValue != null;
	}

	/// <summary>Sets the frozen state of a bound property.</summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="freeze">True to freeze the property; otherwise, false.</param>
	/// <param name="value">The value to freeze.</param>
	/// <exception cref="KeyNotFoundException">Thrown when the property is not found.</exception>
	protected void SetFrozen(string propertyName, bool freeze, object? value = null)
	{
		if (!this.Binds.TryGetValue(propertyName, out PropertyBindInfo? bind))
			throw new KeyNotFoundException($"Failed to find bound property with name \"{propertyName}\".");

		if (freeze)
		{
			value ??= bind.Property.GetValue(this);

			bind.FreezeValue = value;
			bind.Property.SetValue(this, value);
		}
		else
		{
			bind.FreezeValue = null;
		}
	}

	/// <summary>Determines if a bind can be read from.</summary>
	/// <param name="bind">The bind information.</param>
	/// <returns>True if the bind can be read; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual bool CanRead(BindInfo bind)
	{
		MemoryBase? current = this;
		while (current != null)
		{
			if (Interlocked.CompareExchange(ref current.enableReading, 0, 0) != 1)
				return false;

			current = current.parent;
		}

		return true;
	}

	/// <summary>Determines if a bind can be written to.</summary>
	/// <param name="bind">The bind information.</param>
	/// <returns>True if the bind can be written; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual bool CanWrite(BindInfo bind)
	{
		MemoryBase? current = this;
		while (current != null)
		{
			if (Interlocked.CompareExchange(ref current.enableWriting, 0, 0) != 1)
				return false;

			current = current.parent;
		}

		return true;
	}

	/// <summary>
	/// Handles property changes for the current object.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName))
			return;

		if (!this.Binds.TryGetValue(e.PropertyName, out PropertyBindInfo? bind))
		{
			// If the bind is not found, assume it is not bound to a memory address.
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
			return;
		}

		object? currentValue = bind.Property.GetValue(this);
		if (currentValue == null || bind.Flags.HasFlagUnsafe(BindFlags.Pointer))
			return;

		// Frozen binds should not be updated when prompted by the application (i.e. the user)
		if (bind.FreezeValue != null)
			bind.FreezeValue = currentValue;

		// If the value hasn't changed, we don't need to write it to memory
		if (bind.LastValue == currentValue)
			return;

		if (!this.CanWrite(bind) || Interlocked.CompareExchange(ref this.isSynchronizing, 0, 0) == 1)
		{
			// If this bind couldn't be written right now, add it to the delayed bind list
			// to attempt to write later.
			this.delayedBinds.Enqueue(bind);
			return;
		}

		object? oldValue = bind.LastValue;
		this.ClaimLocks();
		try
		{
			// Make sure that all delayed binds are written before this bind to preserve order
			this.WriteDelayedBindsInternal();

			this.WriteToMemory(bind);
		}
		finally
		{
			this.ReleaseLocks();
		}

		bind.LastValue = currentValue;

		// Propagate the property changed event to the rest of the application after write to memory
		var origin = PropertyChange.Origins.User;
		if (!bind.Flags.HasFlagUnsafe(BindFlags.DontRecordHistory) && HistoryService.InstanceOrNull?.IsRestoring == true)
		{
			origin = PropertyChange.Origins.History;
		}

		var change = new PropertyChange(bind, oldValue, bind.LastValue, origin);
		this.PropagatePropertyChanged(bind.Name, change);
	}

	/// <summary>
	/// Claims locks for the current object and its descendants.
	/// </summary>
	protected void ClaimLocks()
	{
		var queue = s_queuePool.Get();
		try
		{
			queue.Enqueue(this);
			while (queue.Count > 0)
			{
				MemoryBase current = queue.Dequeue();
				if (!current.lockObject.TryEnter(5000))
					throw new Exception("Failed to claim lock on memory object. Possible deadlock?");

				foreach (MemoryBase child in current.Children)
				{
					queue.Enqueue(child);
				}
			}
		}
		finally
		{
			queue.Clear();
			s_queuePool.Return(queue);
		}
	}

	/// <summary>
	/// Releases locks for the current object and its descendants.
	/// </summary>
	protected void ReleaseLocks()
	{
		var stack = s_stackPool.Get();
		try
		{
			stack.Push(this);
			while (stack.Count > 0)
			{
				MemoryBase current = stack.Pop();
				current.lockObject.Exit();

				foreach (MemoryBase child in current.Children)
				{
					stack.Push(child);
				}
			}
		}
		finally
		{
			stack.Clear();
			s_stackPool.Return(stack);
		}
	}

	/// <summary>
	/// Sets the synchronization state for the current object and its descendants.
	/// </summary>
	/// <param name="value">The synchronization state.</param>
	protected void SetIsSynchronizing(bool value)
	{
		var stack = s_stackPool.Get();

		try
		{
			stack.Push(this);
			while (stack.Count > 0)
			{
				MemoryBase current = stack.Pop();
				Interlocked.Exchange(ref current.isSynchronizing, value ? 1 : 0);

				foreach (MemoryBase child in current.Children)
				{
					stack.Push(child);
				}
			}
		}
		finally
		{
			stack.Clear();
			s_stackPool.Return(stack);
		}
	}

	/// <summary>Reads a bound property value from memory.</summary>
	/// <param name="bind">The property bind information.</param>
	protected virtual void ReadFromMemory(PropertyBindInfo bind)
	{
		if (!this.CanRead(bind))
			return;

		if (bind.IsWriting)
			throw new Exception("Cannot read memory while we're writing to it");

		if (bind.Flags.HasFlagUnsafe(BindFlags.OnlyInGPose) && GposeService.InstanceOrNull?.IsGpose != true)
			return;

		bind.IsReading = true;
		object? oldValue = bind.LastValue;

		try
		{
			IntPtr bindAddress = bind.GetAddress();

			if (bindAddress == bind.LastFailureAddress)
				return;

			if (typeof(MemoryBase).IsAssignableFrom(bind.Type))
			{
				MemoryBase? memory = bind.Property.GetValue(this) as MemoryBase;

				bool isNew = false;

				if (memory == null && bindAddress != IntPtr.Zero)
				{
					isNew = true;
					memory = Activator.CreateInstance(bind.Type) as MemoryBase;

					if (memory == null)
					{
						throw new Exception($"Failed to create instance of child memory type: {bind.Type}");
					}
				}

				if (memory == null)
					return;

				if (memory.Address == bindAddress)
					return;

				// Invalidate all delayed binds if they were created prior to the memory address change
				// Note: This is only relevant to MemoryBase objects as they are reference type objects

				// Filter out the invalidated delayed binds
				var transientQueue = new ConcurrentQueue<BindInfo>();
				while (this.delayedBinds.TryDequeue(out BindInfo? delayedBind))
				{
					if (delayedBind != bind)
						transientQueue.Enqueue(delayedBind);
				}

				// Replace the delayed binds queue
				this.delayedBinds = transientQueue;

				try
				{
					if (bindAddress == IntPtr.Zero)
					{
						this.SetValueWithoutNotification(bind, null);
						bind.LastValue = null;
						memory.ReleaseLocks();
						memory.Dispose();
						this.Children.Remove(memory);
					}
					else
					{
						memory.Address = bindAddress;
						this.SetValueWithoutNotification(bind, memory);
						bind.LastValue = memory;

						if (isNew)
						{
							memory.parent = this;
							memory.parentBind = bind;
							memory.ClaimLocks();
							this.Children.Add(memory);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning(ex, $"Failed to bind to child memory: {bind.Name}");
					bind.LastFailureAddress = bindAddress;
				}
			}
			else
			{
				if (bindAddress == IntPtr.Zero || bindAddress.ToInt64() < 0)
					return;

				object memValue = MemoryService.Read(bindAddress, bind.Type);

				// We're only interested in binds that have changed
				if (bind.LastValue != null && bind.LastValue.Equals(memValue))
					return;

				// If the bind is frozen, overwrite the memory value with the frozen value.
				// Note: A write is still necessary in case the bind address has changed
				// or the user has changed the value.
				if (bind.FreezeValue != null)
				{
					memValue = bind.FreezeValue;
					bind.IsReading = false;
					this.WriteToMemory(bind);
					bind.IsReading = true;
				}

				this.SetValueWithoutNotification(bind, memValue);
				bind.LastValue = memValue;
			}

			// Notify the application of the property change
			this.PropagatePropertyChanged(bind.Name, new PropertyChange(bind, oldValue, bind.LastValue, PropertyChange.Origins.Game));
		}
		finally
		{
			bind.IsReading = false;
		}
	}

	/// <summary>Writes a bound property's value to memory.</summary>
	/// <param name="bind">The property bind information.</param>
	protected virtual void WriteToMemory(PropertyBindInfo bind)
	{
		if (!this.CanWrite(bind))
			return;

		if (bind.IsReading)
			throw new Exception("Attempt to write memory while reading it");

		bind.IsWriting = true;

		try
		{
			if (bind.Flags.HasFlagUnsafe(BindFlags.Pointer))
				throw new NotSupportedException("Attempt to write a pointer value to memory.");

			IntPtr bindAddress = bind.GetAddress();
			object? val = bind.Property.GetValue(this) ?? throw new Exception("Attempt to write null value to memory");
			MemoryService.Write(bindAddress, val);
			bind.LastValue = val;
		}
		finally
		{
			bind.IsWriting = false;
		}
	}

	/// <summary>
	/// Internal method to synchronize the memory object.
	/// </summary>
	/// <remarks>
	/// An internal method is used to allow for recursion on locked objects.
	/// </remarks>
	private void SynchronizeInternal()
	{
		if (this.Address == IntPtr.Zero)
			return;

		try
		{
			this.SetIsSynchronizing(true);

			var stack = s_stackPool.Get();

			try
			{
				stack.Push(this);

				while (stack.Count > 0)
				{
					MemoryBase current = stack.Pop();

					if (current is IArrayMemory arrayMemory)
					{
						arrayMemory.ReadArrayMemory();
					}
					else
					{
						// Go through all binds that belong to this memory object.
						foreach (PropertyBindInfo bind in current.Binds.Values)
						{
							// Skip if we can't read this bind right now.
							if (!current.CanRead(bind))
								continue;

							try
							{
								current.ReadFromMemory(bind);
							}
							catch (Exception ex)
							{
								throw new Exception($"Failed to read {current.GetType()} - {bind.Name}", ex);
							}
						}
					}

					// Go through all child memory objects.
					foreach (MemoryBase child in current.Children)
					{
						// If the child has no parent bind, then it is not a bind, and should not be refreshed.
						if (child.parentBind == null)
							continue;

						// If the child is a bind, and the parent bind is not readable, then skip it.
						if (!current.CanRead(child.parentBind))
							continue;

						// If the child is a bind but only applies in gpose and we are not in gpose, then skip it.
						if (child.parentBind.Flags.HasFlagUnsafe(BindFlags.OnlyInGPose) && GposeService.InstanceOrNull?.IsGpose != true)
							continue;

						stack.Push(child);
					}
				}
			}
			finally
			{
				stack.Clear();
				s_stackPool.Return(stack);
			}
		}
		finally
		{
			this.SetIsSynchronizing(false);
		}
	}

	/// <summary>
	/// Internal method to write delayed binds to memory.
	/// </summary>
	/// <remarks>
	/// An internal method is used to allow for recursion on locked objects.
	/// </remarks>
	private void WriteDelayedBindsInternal()
	{
		var stack = s_stackPool.Get();
		try
		{
			stack.Push(this);

			while (stack.Count > 0)
			{
				MemoryBase current = stack.Pop();

				var remainingBinds = ArrayPool<BindInfo>.Shared.Rent(current.delayedBinds.Count);
				int remainingCount = 0;

				try
				{
					while (current.delayedBinds.TryDequeue(out BindInfo? bind))
					{
						if (bind is not PropertyBindInfo propertyBind)
							continue;

						// If we still can't write this bind, just skip it.
						if (!current.CanWrite(propertyBind))
						{
							remainingBinds[remainingCount++] = propertyBind;
							continue;
						}

						// Store the old value before it is overwritten by the new value in WriteToMemory
						object? oldValue = propertyBind.LastValue;

						current.WriteToMemory(propertyBind);

						// Propagate property changed event to the rest of the application after writing to memory
						var origin = PropertyChange.Origins.User;
						if (!propertyBind.Flags.HasFlagUnsafe(BindFlags.DontRecordHistory) && HistoryService.InstanceOrNull?.IsRestoring == true)
						{
							origin = PropertyChange.Origins.History;
						}

						var change = new PropertyChange(propertyBind, oldValue, propertyBind.LastValue, origin);
						current.PropagatePropertyChanged(propertyBind.Name, change);
					}
				}
				finally
				{
					for (int i = 0; i < remainingCount; i++)
					{
						current.delayedBinds.Enqueue(remainingBinds[i]);
					}

					ArrayPool<BindInfo>.Shared.Return(remainingBinds, clearArray: true);

					if (!current.delayedBinds.IsEmpty)
						Log.Warning($"Failed to write all delayed binds, remaining: {current.delayedBinds.Count}");
				}

				foreach (var child in current.Children)
				{
					stack.Push(child);
				}
			}
		}
		finally
		{
			stack.Clear();
			s_stackPool.Return(stack);
		}
	}

	/// <summary>
	/// Sets the value of the specified property bind without triggering property change notifications.
	/// </summary>
	/// <param name="bind">The property bind information.</param>
	/// <param name="value">The value to set.</param>
	/// <remarks>
	/// This is used in <see cref="ReadFromMemory(PropertyBindInfo)"/> to ensure that memory changes
	/// that originate from the game do not get processed via the OnSelfPropertyChanged, which is
	/// intended to be called only for user-initiated changes (incl. history).
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetValueWithoutNotification(PropertyBindInfo bind, object? value)
	{
		this.SuppressPropNotifications.Value = true;
		bind.Property.SetValue(this, value);
		this.SuppressPropNotifications.Value = false;
	}

	/// <summary>
	/// Recursively propagates the property changed event all ancestors of the current object.
	/// </summary>
	/// <param name="propertyName">The name of the property that changed.</param>
	/// <param name="context">The context of the property change.</param>
	/// <exception cref="Exception">Thrown when the parent is not null but the parent bind is null.</exception>
	private void PropagatePropertyChanged(string propertyName, PropertyChange context)
	{
		MemoryBase? current = this;
		int ancestorCount = context.BindPath.Count;
		while (current != null)
		{
			if (current.parent == null)
				break;

			if (current.parentBind == null)
				throw new Exception("Parent was not null, but parent bind was!");

			ancestorCount++;
			current = current.parent;
		}

		// Resize the bind info list that that we know total capacity
		context.BindPath.Capacity = ancestorCount;

		current = this;
		while (current != null)
		{
			current.PropertyChanged?.Invoke(current, new MemObjPropertyChangedEventArgs(propertyName, context));

			if (current.parent == null)
				break;

			context.AddPath(current.parentBind!);
			current = current.parent;
		}
	}
}
