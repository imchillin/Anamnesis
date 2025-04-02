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

	/// <summary>The name of the animation object.</summary>
	public string? Name { get; }

	/// <summary>An image reference to the animation object's icon.</summary>
	public ImgRef? Icon { get; }

	/// <summary>The timeline of the animation.</summary>
	public ActionTimeline? Timeline { get; }

	/// <summary>The purpose of the animation.</summary>
	public AnimationPurpose Purpose { get; }
}
