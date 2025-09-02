// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;

public static class VersionInfo
{
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
	public static readonly Version ApplicationVersion = new Version(7, 31, 0, 0);

#if CI_BUILD
	public static readonly bool IsDevelopmentBuild = false;
#else
	public static readonly bool IsDevelopmentBuild = true;
#endif

	/// <summary>
	/// The latest game version that the tool has been validated for.
	/// </summary>
	public static readonly string ValidatedGameVersion = "2025.08.22.0000.0000";
}
