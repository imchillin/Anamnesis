// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Anamnesis.Core.Memory;
	using Anamnesis.Styles;
	using FontAwesome.Sharp;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ActorBasicViewModel : MemoryViewModelBase<Actor>
	{
		private static readonly Dictionary<string, string> NicknameLookup = new Dictionary<string, string>();

		public ActorBasicViewModel(IntPtr pointer)
			: base(pointer, null)
		{
		}

		[ModelField] public byte[]? NameBytes { get; set; }
		[ModelField] public int DataId { get; set; }
		[ModelField] [Refresh] public ActorTypes ObjectKind { get; set; }

		[ModelField] public byte SubKind { get; set; }
		[ModelField] public byte DistanceFromPlayerX { get; set; }
		[ModelField] public byte DistanceFromPlayerY { get; set; }
		[ModelField] [Refresh] public int ModelType { get; set; }
		[ModelField] public bool IsAnimating { get; set; }
		[ModelField] [Refresh] public RenderModes RenderMode { get; set; }
		[ModelField] public float Transparency { get; set; }

		public string Id => this.Name + this.DataId;
		public string Name => SeString.FromSeStringBytes(this.NameBytes);
		public IconChar Icon => this.ObjectKind.GetIcon();

		public double DistanceFromPlayer => Math.Sqrt(((int)this.DistanceFromPlayerX ^ 2) + ((int)this.DistanceFromPlayerY ^ 2));

		[AlsoNotifyFor(nameof(ActorViewModel.DisplayName))]
		public string? Nickname
		{
			get
			{
				if (NicknameLookup.ContainsKey(this.Id))
					return NicknameLookup[this.Id];

				return null;
			}

			set
			{
				if (value == null)
				{
					if (NicknameLookup.ContainsKey(this.Id))
					{
						NicknameLookup.Remove(this.Id);
					}
				}
				else
				{
					if (!NicknameLookup.ContainsKey(this.Id))
						NicknameLookup.Add(this.Id, string.Empty);

					NicknameLookup[this.Id] = value;
				}
			}
		}

		/// <summary>
		/// Gets the Nickname or if not set, the Name.
		/// </summary>
		public string DisplayName
		{
			get
			{
				if (this.Nickname == null)
					return this.Name;

				return this.Nickname;
			}
		}

		public int ObjectKindInt
		{
			get => (int)this.ObjectKind;
			set => this.ObjectKind = (ActorTypes)value;
		}

		public virtual void OnRetargeted()
		{
		}

		public void SetObjectKindDirect(ActorTypes type, IntPtr? actorPointer)
		{
			if (actorPointer == null)
				return;

			IntPtr objectKindPointer = ((IntPtr)actorPointer) + Actor.ObjectKindOffset;
			MemoryService.Write(objectKindPointer, (byte)type, "Set ObjectKind Direct");
		}

		private class RefreshAttribute : Attribute
		{
			public static bool IsSet(PropertyInfo property)
			{
				return property.GetCustomAttribute<RefreshAttribute>() != null;
			}
		}
	}
}
