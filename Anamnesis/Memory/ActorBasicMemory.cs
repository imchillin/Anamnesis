// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using Anamnesis.Connect;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;
	using Anamnesis.Styles;
	using Anamnesis.Utils;
	using FontAwesome.Sharp;
	using PropertyChanged;

	public class ActorBasicMemory : MemoryBase
	{
		private ActorBasicMemory? owner;

		[Bind(0x030)] public Utf8String NameBytes { get; set; }
		[Bind(0x074)] public uint ObjectId { get; set; }
		[Bind(0x080)] public uint DataId { get; set; }
		[Bind(0x084)] public uint OwnerId { get; set; }

		[Bind(0x08c, BindFlags.ActorRefresh)] public ActorTypes ObjectKind { get; set; }
		[Bind(0x090)] public byte DistanceFromPlayerX { get; set; }
		[Bind(0x092)] public byte DistanceFromPlayerY { get; set; }

		public string Id => $"n{this.NameHash}_d{this.DataId}_o{this.Address}";
		public string IdNoAddress => $"n{this.NameHash}_d{this.DataId}";

		public IconChar Icon => this.ObjectKind.GetIcon();
		public double DistanceFromPlayer => Math.Sqrt(((int)this.DistanceFromPlayerX ^ 2) + ((int)this.DistanceFromPlayerY ^ 2));
		public string NameHash => HashUtility.GetHashString(this.NameBytes.ToString(), true);

		[AlsoNotifyFor(nameof(ActorMemory.DisplayName))]
		public string? Nickname { get; set; }

		[AlsoNotifyFor(nameof(ActorMemory.IsOverworldActor), nameof(ActorMemory.CanRefresh))]
		public bool IsGPoseActor { get; private set; } = false;

		[AlsoNotifyFor(nameof(IsGPoseActor))]
		public bool IsOverworldActor => !this.IsGPoseActor;

		[AlsoNotifyFor(nameof(IsGPoseActor))]
		public bool CanRefresh
		{
			get
			{
				if (PoseService.Instance.IsEnabled)
					return false;

				if (AnamnesisConnectService.IsPenumbraConnected)
					return true;

				return this.IsOverworldActor;
			}
		}

		/// <summary>
		/// Gets the Nickname or if not set, the Name.
		/// </summary>
		public string DisplayName => this.Nickname ?? this.Name;

		public string Name
		{
			get
			{
				string name = this.NameBytes.ToString();

				// Geting owner now can be expensive, so disable this for now.
				/*ActorBasicMemory? owner = this.GetOwner();

				if (owner != null)
					name += $" ({owner.DisplayName})";*/

				return name;
			}
		}

		public void SetAddress(IntPtr address, bool isGPose)
		{
			this.IsGPoseActor = isGPose;
			base.SetAddress(address);
		}

		public override void SetAddress(IntPtr address)
		{
			throw new InvalidOperationException("Use isGPose override");
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

			List<ActorBasicMemory>? actors = TargetService.GetAllActors();

			foreach(ActorBasicMemory actor in actors)
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
}
