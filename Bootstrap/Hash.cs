// © Anamnesis.
// Licensed under the MIT license.

namespace Bootstrap;

using System.Security.Cryptography;

internal class Hash
{
	public static string Compute(Stream stream)
	{
		stream.Position = 0;
		using var hashMethod = SHA512.Create();
		byte[] hashByte = hashMethod.ComputeHash(stream);
		return Convert.ToHexString(hashByte);
	}
}
