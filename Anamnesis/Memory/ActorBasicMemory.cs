// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using Anamnesis.Styles;
	using FontAwesome.Sharp;
	using PropertyChanged;

	public class ActorBasicMemory : MemoryBase
	{
		[Bind(0x030)] public Utf8String NameBytes { get; set; }
		[Bind(0x074)] public uint ObjectId { get; set; }
		[Bind(0x080)] public uint DataId { get; set; }
		[Bind(0x084)] public uint OwnerId { get; set; }
		[Bind(0x08c, BindFlags.ActorRefresh)] public ActorTypes ObjectKind { get; set; }
		[Bind(0x090)] public byte DistanceFromPlayerX { get; set; }
		[Bind(0x092)] public byte DistanceFromPlayerY { get; set; }

		public string Id => this.Name + this.DataId;
		public string Name => this.NameBytes.ToString();
		public IconChar Icon => this.ObjectKind.GetIcon();
		public double DistanceFromPlayer => Math.Sqrt(((int)this.DistanceFromPlayerX ^ 2) + ((int)this.DistanceFromPlayerY ^ 2));

		[AlsoNotifyFor(nameof(ActorMemory.DisplayName))]
		public string? Nickname { get; set; }

		/// <summary>
		/// Gets the Nickname or if not set, the Name.
		/// </summary>
		public string DisplayName => this.Nickname ?? this.Name;
	}
}
