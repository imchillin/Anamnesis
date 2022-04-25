// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public struct Utf8String
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
	public byte[]? Bytes;

	public static bool operator ==(Utf8String left, Utf8String right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Utf8String left, Utf8String right)
	{
		return !(left == right);
	}

	public override string ToString()
	{
		if (this.Bytes == null)
			return string.Empty;

		int i;
		for (i = 0; i < this.Bytes.Length; i++)
		{
			if (this.Bytes[i] == 0)
				break;
		}

		return Encoding.UTF8.GetString(this.Bytes, 0, i);
	}

	public void FromString(string self)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(self);

		if (bytes.Length >= 30)
			throw new Exception($"SeString value {self} excedes 30 byte limit");

		this.Bytes = bytes;
	}

	public override bool Equals(object? obj)
	{
		if (obj is Utf8String other)
		{
			if (this.Bytes == null)
				return other.Bytes == null;

			if (other.Bytes == null)
				return false;

			return this.Bytes.SequenceEqual(other.Bytes);
		}

		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Bytes?.GetHashCode());
	}
}
