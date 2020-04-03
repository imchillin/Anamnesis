// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class NpcResidentWrapper : ObjectWrapper<ENpcResident>, INpcResident
	{
		public NpcResidentWrapper(ENpcResident row)
			: base(row)
		{
		}
	}
}
