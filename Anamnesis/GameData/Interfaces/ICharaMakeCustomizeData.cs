// © Anamnesis.
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
		List<ICharaMakeCustomize> GetFeatureOptions(Features feature, Customize.Tribes tribe, Customize.Genders gender);
		ICharaMakeCustomize? GetFeature(Features feature, Customize.Tribes tribe, Customize.Genders gender, byte featureId);
	}
}
