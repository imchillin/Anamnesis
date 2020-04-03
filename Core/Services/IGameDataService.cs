// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using System;
	using System.Collections.Generic;

	public interface IGameDataService : IService
	{
		IEnumerable<IRace> Races { get; }
		IEnumerable<ITribe> Tribes { get; }
		IEnumerable<IItem> Items { get; }
		IEnumerable<IStain> Stains { get; }
		IEnumerable<INpcBase> BaseNPCs { get; }
		IEnumerable<ITerritoryType> Territories { get; }
		IEnumerable<IWeather> Weathers { get; }
		IEnumerable<ICharaMakeCustomize> CharacterMakeCustomize { get; }
		IEnumerable<ICharaMakeType> CharacterMakeTypes { get; }
		IEnumerable<INpcResident> ResidentNPCs { get; }
		IEnumerable<ITitle> Titles { get; }
		IEnumerable<IStatus> Statuses { get; }
	}

	public interface IDataObject
	{
		int Key { get; }
	}

	public interface IRace : IDataObject
	{
	}

	public interface ITribe : IDataObject
	{
	}

	public interface IItem : IDataObject
	{
		string Name { get; }
		string Description { get; }
		IImage Icon { get; }
	}

	public interface IStain : IDataObject
	{
	}

	public interface INpcBase : IDataObject
	{
	}

	public interface INpcResident : IDataObject
	{
	}

	public interface ITerritoryType : IDataObject
	{
	}

	public interface IWeather : IDataObject
	{
	}

	public interface ICharaMakeCustomize : IDataObject
	{
	}

	public interface ICharaMakeType : IDataObject
	{
	}

	public interface ITitle : IDataObject
	{
	}

	public interface IStatus : IDataObject
	{
	}

	public interface IImage
	{
		IntPtr HBitmap { get; }
		int Width { get; }
		int Height { get; }
	}
}
