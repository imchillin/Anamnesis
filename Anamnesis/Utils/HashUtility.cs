// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using System;
using System.Security.Cryptography;
using System.Text;

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

		using (HashAlgorithm algorithm = SHA256.Create())
		{
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(finalString));
		}
	}

	public static string GetHashString(string inputString, bool salt = false)
	{
		StringBuilder sb = new StringBuilder();

		foreach (byte b in GetHash(inputString, salt))
		{
			sb.Append(b.ToString("X2"));
		}

		return sb.ToString();
	}

	private static string GenerateRandomString(int length)
	{
		Random rand = new Random();

		int randValue;
		StringBuilder builder = new StringBuilder();
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
