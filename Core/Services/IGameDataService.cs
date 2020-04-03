// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using System;
	using System.Collections.Generic;

	public interface IGameDataService : IService
	{
		IData<IRace> Races { get; }
		IData<ITribe> Tribes { get; }
		IData<IItem> Items { get; }
		IData<IStain> Stains { get; }
		IData<INpcBase> BaseNPCs { get; }
		IData<ITerritoryType> Territories { get; }
		IData<IWeather> Weathers { get; }
		IData<ICharaMakeCustomize> CharacterMakeCustomize { get; }
		IData<ICharaMakeType> CharacterMakeTypes { get; }
		IData<INpcResident> ResidentNPCs { get; }
		IData<ITitle> Titles { get; }
		IData<IStatus> Statuses { get; }
	}

	public interface IData<T>
		where T : IDataObject
	{
		IEnumerable<T> All { get; }

		T Get(int key);
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
