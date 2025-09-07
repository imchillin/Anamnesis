// © Anamnesis.
// Licensed under the MIT license.

using System.Security.Cryptography;
using System.Text;

namespace Scripts;

public static class HashUtility
{
	// A salt is generated randomly each time we start, will amek all hashes unique to that run
	// of Anamnesis.
	public static readonly string Salt = GenerateRandomString(10);

	public static byte[] GetHash(string inputString, bool salt = false)
	{
		string finalString = inputString;
		if (salt)
			finalString += Salt;
		return SHA256.HashData(Encoding.UTF8.GetBytes(finalString));
	}

	public static string GetHashString(string inputString, bool salt = false)
	{
		var sb = new StringBuilder();

		foreach (byte b in GetHash(inputString, salt))
		{
			sb.Append(b.ToString("X2"));
		}

		return sb.ToString();
	}

	private static string GenerateRandomString(int length)
	{
		var rand = new Random();

		int randValue;
		var builder = new StringBuilder();
		char letter;
		for (int i = 0; i < length; i++)
		{
			randValue = rand.Next(0, 26);
			letter = Convert.ToChar(randValue + 65);
			builder.Append(letter);
		}

		return builder.ToString();
	}
}