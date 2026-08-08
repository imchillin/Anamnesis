// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;

public static class VersionInfo
{
#if CI_BUILD
	public static readonly bool IsDevelopmentBuild = false;
#else
	public static readonly bool IsDevelopmentBuild = true;
#endif

	/// <summary>
	/// The latest game version that the tool has been validated for.
	/// </summary>
	public static readonly string ValidatedGameVersion = "2026.08.05.0000.0000";

	/// <summary>
	/// Application version.
	/// </summary>
	/// <remarks>
	/// [!] Do NOT manually update this field. It is automatically updated
	/// in the application bootstrapping process from the Velopack installation.
	///
	/// Format: [Major].[Minor].[Build].0
	/// - Major: Reflects the major version of the game.
	/// - Minor: Reflects the minor version of the game, padded to 2 digits.
	/// - Build: The version of the tool. This should reset to 0 on every minor or major release.
	///   - Bump the build number after feature releases, improvements, or bug fixes.
	/// - Revision: Unused, always set to 0.
	/// </remarks>
	public static Version ApplicationVersion = new(0, 0, 0, 0);
}
