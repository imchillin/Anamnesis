// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using System;
	using System.Collections.Generic;

	// TODO: split into multiple files.
	#pragma warning disable SA1201

	public interface IGameDataService : IService
	{
		IData<IRace> Races { get; }
		IData<ITribe> Tribes { get; }
		IData<IItem> Items { get; }
		IData<IDye> Dyes { get; }
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

		int ModelBaseId { get; }
		int ModelVariantId { get; }
		int ModelId { get; }

		bool FitsInSlot(ItemSlots slot);
	}

	public enum ItemSlots
	{
		MainHand,

		Head,
		Body,
		Hands,
		Waist,
		Legs,
		Feet,

		OffHand,
		Ears,
		Neck,
		Wrists,
		RightRing,
		LeftRing,
	}

	public interface IDye : IDataObject
	{
		string Name { get; }
		string Description { get; }
		IImage Icon { get; }
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
