// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Styles;
using Anamnesis.Utils;
using FontAwesome.Sharp;
using PropertyChanged;
using System;
using System.Collections.Generic;

public class ActorBasicMemory : MemoryBase
{
	private ActorBasicMemory? owner;

	public enum RenderModes : uint
	{
		Draw = 0,
		Unload = 2,
		Load = 4,
	}

	[Bind(0x030)] public Utf8String NameBytes { get; set; }
	[Bind(0x078)] public uint ObjectId { get; set; }
	[Bind(0x084)] public uint DataId { get; set; }
	[Bind(0x088)] public uint OwnerId { get; set; }
	[Bind(0x08C)] public ushort ObjectIndex { get; set; }
	[Bind(0x090, BindFlags.ActorRefresh)] public ActorTypes ObjectKind { get; set; }
	[Bind(0x091)] public byte SubKind { get; set; }
	[Bind(0x094)] public byte DistanceFromPlayerX { get; set; }
	[Bind(0x096)] public byte DistanceFromPlayerY { get; set; }
	[Bind(0x00C4)] public float Scale { get; set; }
	[Bind(0x0100, BindFlags.Pointer)] public ActorModelMemory? ModelObject { get; set; }
	[Bind(0x0118)] public RenderModes RenderMode { get; set; }

	public string Id => $"n{this.NameHash}_d{this.DataId}_o{this.Address}";
	public string IdNoAddress => $"n{this.NameHash}_d{this.DataId}"; ////_k{this.ObjectKind}";
	public int Index => ActorService.Instance.GetActorTableIndex(this.Address);
	public IconChar Icon => this.ObjectKind.GetIcon();
	public double DistanceFromPlayer => Math.Sqrt(
		(this.DistanceFromPlayerX * this.DistanceFromPlayerX) +
		(this.DistanceFromPlayerY * this.DistanceFromPlayerY));

	public string NameHash => HashUtility.GetHashString(this.NameBytes.ToString(), true);

	[AlsoNotifyFor(nameof(DisplayName))]
	public string? Nickname { get; set; }

	[DependsOn(nameof(ObjectIndex))]
	public virtual bool IsGPoseActor => this.ObjectIndex >= ActorService.GPOSE_INDEX_START && this.ObjectIndex < ActorService.GPOSE_INDEX_END;

	[DependsOn(nameof(IsGPoseActor))]
	public bool IsOverworldActor => !this.IsGPoseActor;

	[DependsOn(nameof(RenderMode))]
	public bool IsHidden => this.RenderMode != RenderModes.Draw;

	/// <summary>
	/// Gets the Nickname or if not set, the Name.
	/// </summary>
	public string DisplayName => this.Nickname ?? this.Name;

	[DependsOn(nameof(NameBytes))]
	public string Name => this.NameBytes.ToString();

	[DependsOn(nameof(ObjectKind))]
	public int ObjectKindInt
	{
		get => (int)this.ObjectKind;
		set => this.ObjectKind = (ActorTypes)value;
	}

	[DependsOn(nameof(ObjectKind))]
	public bool IsPlayer => this.ObjectKind == ActorTypes.Player;

	[DependsOn(nameof(ObjectIndex), nameof(Address))]
	public bool IsValid
	{
		get => this.Address != IntPtr.Zero && ActorService.InstanceOrNull?.GetActorTableIndex(this.Address) == this.ObjectIndex;
	}

	/// <summary>
	/// Get owner will return the owner of a carbuncle or minion, however
	/// only while outside of gpose. Making this fucntion USELESS.
	/// once inside of gpose, the owner becomes itself. Thanks SQEX.
	/// </summary>
	/// <returns>This actors owner actor.</returns>
	public ActorBasicMemory? GetOwner()
	{
		// do we own ourselves?
		if (this.OwnerId == this.ObjectId)
			return null;

		// do we already have the correct owner?
		if (this.owner != null && this.owner.ObjectId == this.OwnerId)
			return this.owner;

		this.owner = null;

		List<ActorBasicMemory>? actors = ActorService.Instance.GetAllActors();

		foreach (ActorBasicMemory actor in actors)
		{
			if (actor.IsDisposed)
				continue;

			if (actor.ObjectKind != ActorTypes.Player &&
				actor.ObjectKind != ActorTypes.BattleNpc &&
				actor.ObjectKind != ActorTypes.EventNpc)
				continue;

			if (actor.ObjectId == this.OwnerId)
			{
				this.owner = actor;
				break;
			}
		}

		return this.owner;
	}

	public override string ToString() => $"{this.Id}";
}
