// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public enum Features
	{
		Hair,
		FacePaint,
	}

	public interface ICharaMakeCustomizeData : ISheet<ICharaMakeCustomize>
	{
		List<ICharaMakeCustomize> GetFeatureOptions(Features feature, Appearance.Tribes tribe, Appearance.Genders gender);
		ICharaMakeCustomize? GetFeature(Features feature, Appearance.Tribes tribe, Appearance.Genders gender, byte featureId);
	}
}
