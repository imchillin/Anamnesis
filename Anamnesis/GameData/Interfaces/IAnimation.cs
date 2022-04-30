// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Interfaces;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;

public interface IAnimation
{
	public enum AnimationPurpose
	{
		Unknown,
		Raw,
		Action,
		Standard,
		Intro,
		Ground,
		Chair,
		Blend,
	}

	public string? DisplayName { get; }
	public ImageReference? Icon { get; }
	public ActionTimeline? Timeline { get; }
	public AnimationPurpose Purpose { get; }
}
