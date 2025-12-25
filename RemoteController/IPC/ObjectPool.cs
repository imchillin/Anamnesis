// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.IPC;

using System.Collections.Concurrent;

/// <summary>
/// A simple, lock-free object pool for reusing objects.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="ObjectPool{T}"/> class.
/// </remarks>
/// <param name="maxSize">
/// Maximum number of objects to retain in the pool.
/// </param>
public sealed class ObjectPool<T>(int maxSize = 32)
	where T : class, new()
{
	private readonly ConcurrentStack<T> pool = new();
	private readonly int maxSize = maxSize;

	private T? lastItem;

	/// <summary>
	/// Gets an object from the pool or creates a new one.
	/// </summary>
	/// <returns>
	/// An instance of <typeparamref name="T"/>.
	/// </returns>
	public T Get()
	{
		T? item = Interlocked.Exchange(ref this.lastItem, null);
		if (item != null)
			return item;

		return this.pool.TryPop(out item) ? item : new T();
	}

	/// <summary>
	/// Returns an object to the pool.
	/// </summary>
	/// <param name="item">The object to return.</param>
	public void Return(T item)
	{
		if (Interlocked.CompareExchange(ref this.lastItem, item, null) == null)
			return;

		if (this.pool.Count < this.maxSize)
		{
			this.pool.Push(item);
		}
	}
}
