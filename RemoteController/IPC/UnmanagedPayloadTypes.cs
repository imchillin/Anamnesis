// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.IPC;

using RemoteController.Interop;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// The hook registration payload structure.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct HookRegistrationData
{
	private const int MAX_KEY_LENGTH = 128;

	public int DelegateKeyLength;
	public fixed byte DelegateKey[MAX_KEY_LENGTH];
	public nint Address;
	public HookType HookType;
	public HookBehavior HookBehavior;

	/// <summary>
	/// Retrieves the delegate key from the internal byte array
	/// as a UTF-8 encoded string.
	/// </summary>
	/// <returns>The encoded delegate key string.</returns>
	public readonly string GetKey()
	{
		fixed (byte* ptr = this.DelegateKey)
		{
			return Encoding.UTF8.GetString(ptr, this.DelegateKeyLength);
		}
	}

	/// <summary>
	/// Sets the delegate key into the internal byte array
	/// </summary>
	/// <param name="key">The delegate key string to set.</param>
	/// <exception cref="ArgumentException">
	/// Thrown if the key exceeds the maximum allowed length.
	/// </exception>
	public void SetKey(string key)
	{
		int byteCount = Encoding.UTF8.GetByteCount(key);
		if (byteCount > MAX_KEY_LENGTH)
			throw new ArgumentException($"Key exceeds maximum length of {MAX_KEY_LENGTH} bytes.");

		this.DelegateKeyLength = byteCount;
		fixed (byte* ptr = this.DelegateKey)
		{
			Encoding.UTF8.GetBytes(key, new Span<byte>(ptr, MAX_KEY_LENGTH));
		}
	}
}
