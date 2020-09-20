// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using Anamnesis.Services;
	using PropertyChanged;

	public enum RenderModes : int
	{
		Draw = 0,
		Unload = 2,
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Actor
	{
		[FieldOffset(0x0030)]
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
		public string Name;

		[FieldOffset(0x0074)] public int ActorId;
		[FieldOffset(0x0080)] public int DataId;
		[FieldOffset(0x0084)] public int OwnerId;
		[FieldOffset(0x008c)] public ActorTypes ObjectKind;
		[FieldOffset(0x008D)] public byte SubKind;
		[FieldOffset(0x00F0)] public IntPtr ModelObject;
		[FieldOffset(0x0104)] public RenderModes RenderMode;
		[FieldOffset(0x01F0)] public int PlayerCharacterTargetActorId;
		[FieldOffset(0x1450)] public Weapon MainHand;
		[FieldOffset(0x14B8)] public Weapon OffHand;
		[FieldOffset(0x1708)] public Equipment Equipment;
		[FieldOffset(0x17B8)] public Appearance Customize;
		[FieldOffset(0x17F8)] public int BattleNpcTargetActorId;
		[FieldOffset(0x1868)] public int NameId;
		[FieldOffset(0x1888)] public int ModelType;
	}

	[AddINotifyPropertyChangedInterface]
	public class ActorViewModel : MemoryViewModelBase<Actor>
	{
		public ActorViewModel(IntPtr pointer)
			: base(pointer)
		{
		}

		[ModelField] public string Name { get; set; } = string.Empty;
		[ModelField] public int ActorId { get; set; }
		[ModelField] public int DataId { get; set; }
		[ModelField] public int OwnerId { get; set; }
		[ModelField] public ActorTypes ObjectKind { get; set; }
		[ModelField] public byte SubKind { get; set; }
		[ModelField] public AppearanceViewModel? Customize { get; set; }
		[ModelField] public int PlayerCharacterTargetActorId { get; set; }
		[ModelField] public int BattleNpcTargetActorId { get; set; }
		[ModelField] public int NameId { get; set; }
		[ModelField] public int ModelType { get; set; }
		[ModelField] public RenderModes RenderMode { get; set; }
		[ModelField] public EquipmentViewModel? Equipment { get; set; }
		[ModelField] public WeaponViewModel? MainHand { get; set; }
		[ModelField] public WeaponViewModel? OffHand { get; set; }

		[ModelField] public ModelViewModel? ModelObject { get; set; }

		protected override void OnViewToModel(string fieldName, object? value)
		{
			// Do not allow actor view model changes to push into memory while we are refreshing.
			if (ActorRefreshService.Instance.IsRefreshing)
				return;

			base.OnViewToModel(fieldName, value);
		}
	}
}
