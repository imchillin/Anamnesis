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
	/// Application version.
	/// </summary>
	/// <remarks>
	/// [!] Be sure to update this before making a new release.
	/// Format: [Major].[Minor].[Build].[Revision]
	/// - Major: Reflects the major version of the game.
	/// - Minor: Reflects the minor version of the game, padded to 2 digits.
	/// - Build: The version of the tool. This should reset to 0 on every minor or major release.
	///   - Bump the build number after feature releases, improvements, or bug fixes.
	/// - Revision: The revision of the tool. This should reset to 0 on every build.
	///   - Bump the revision number for hotfixes and urgent patches.
	/// </remarks>
	public static readonly Version ApplicationVersion = new Version(7, 30, 0, 0);

	/// <summary>
	/// The latest game version that the tool has been validated for.
	/// </summary>
	public static readonly string ValidatedGameVersion = "2025.08.07.0000.0000";
}
