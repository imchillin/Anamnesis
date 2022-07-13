// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Collections.Generic;
using Anamnesis.Actor;
using Anamnesis.Services;
using Anamnesis.Styles;
using Anamnesis.Utils;
using FontAwesome.Sharp;
using PropertyChanged;

using MediaColor = System.Windows.Media.Color;

public class ActorBasicMemory : MemoryBase
{
	private ActorBasicMemory? owner;

	public ActorBasicMemory()
	{
		this.Names = new Names(this);
	}

	public enum RenderModes : uint
	{
		Draw = 0,
		Unload = 2,
		Load = 4,
	}

	[Bind(0x030)] public Utf8String NameBytes { get; set; }
	[Bind(0x074)] public uint ObjectId { get; set; }
	[Bind(0x080)] public uint DataId { get; set; }
	[Bind(0x084)] public uint OwnerId { get; set; }
	[Bind(0x088)] public ushort ObjectIndex { get; set; }
	[Bind(0x08c, BindFlags.ActorRefresh)] public ActorTypes ObjectKind { get; set; }
	[Bind(0x090)] public byte DistanceFromPlayerX { get; set; }
	[Bind(0x092)] public byte DistanceFromPlayerY { get; set; }
	[Bind(0x0104)] public RenderModes RenderMode { get; set; }

	public string Id => $"n{this.NameHash}_d{this.DataId}_o{this.Address}";
	public string IdNoAddress => $"n{this.NameHash}_d{this.DataId}"; ////_k{this.ObjectKind}";
	public int Index => ActorService.Instance.GetActorTableIndex(this.Address);

	public IconChar Icon => this.ObjectKind.GetIcon();
	public double DistanceFromPlayer => Math.Sqrt(((int)this.DistanceFromPlayerX ^ 2) + ((int)this.DistanceFromPlayerY ^ 2));
	public string NameHash => HashUtility.GetHashString(this.NameBytes.ToString(), true);

	[DependsOn(nameof(ObjectIndex))]
	public virtual bool IsGPoseActor => this.ObjectIndex >= 200 && this.ObjectIndex < 244;

	[DependsOn(nameof(IsGPoseActor))]
	public bool IsOverworldActor => !this.IsGPoseActor;

	[DependsOn(nameof(RenderMode))]
	public bool IsHidden => this.RenderMode != RenderModes.Draw;

	public Names Names { get; init; }
	public MediaColor Color { get; set; }

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
		get => this.Address != IntPtr.Zero && ActorService.Instance.GetActorTableIndex(this.Address) == this.ObjectIndex;
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

	public override string ToString()
	{
		return $"{this.Id}";
	}
}
