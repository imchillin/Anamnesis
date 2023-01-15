// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;

public static class VersionInfo
{
	/// <summary>
	/// The time this version was published.
	/// </summary>
	// This is written to by the build server. do not change.
	public static readonly DateTime Date = new DateTime(2000, 01, 01, 00, 00, 00, DateTimeKind.Utc);

	/// <summary>
	/// The latest game version that the tool has been validated for.
	/// </summary>
	public static readonly string ValidatedGameVersion = "2023.01.11.0000.0000";
}
