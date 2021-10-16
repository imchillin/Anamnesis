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
		[Bind(0x0030)] public SeString NameBytes { get; set; }
		[Bind(0x0080)] public int DataId { get; set; }
		[Bind(0x008c, BindFlags.ActorRefresh)] public ActorTypes ObjectKind { get; set; }
		[Bind(0x0090)] public byte DistanceFromPlayerX { get; set; }
		[Bind(0x0092)] public byte DistanceFromPlayerY { get; set; }

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
