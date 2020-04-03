// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using System.Collections.Generic;

	public interface IGameDataService : IService
	{
		IEnumerable<IItem> Items { get; }
	}

	public interface IDataObject
	{
		string Name { get; }
	}

	public interface IItem : IDataObject
	{
	}
}
