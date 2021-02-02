// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using Anamnesis.Character;
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
		public const int ObjectKindOffset = 0x008c;
		public const int RenderModeOffset = 0x0104;

		[FieldOffset(0x0030)]
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
		public string Name;

		[FieldOffset(0x0080)] public int DataId;
		[FieldOffset(ObjectKindOffset)] public ActorTypes ObjectKind;
		[FieldOffset(0x008D)] public byte SubKind;
		[FieldOffset(0x0090)] public byte DistanceFromPlayerX;
		[FieldOffset(0x0092)] public byte DistanceFromPlayerY;
		[FieldOffset(0x00F0)] public IntPtr ModelObject;
		[FieldOffset(RenderModeOffset)] public RenderModes RenderMode;
		[FieldOffset(0x01B4)] public int ModelType;
		[FieldOffset(0x0F08)] public Weapon MainHand;
		[FieldOffset(0x0F70)] public Weapon OffHand;
		[FieldOffset(0x1040)] public Equipment Equipment;
		[FieldOffset(0x1808)] public float Transparency;
		[FieldOffset(0x1878)] public Appearance Customize;

		public string Id => this.Name + this.DataId;
	}

	[AddINotifyPropertyChangedInterface]
	public class ActorViewModel : MemoryViewModelBase<Actor>
	{
		private const short RefreshDelay = 250;

		private static Dictionary<string, string> nicknameLookup = new Dictionary<string, string>();

		private short refreshDelay;
		private Task? refreshTask;
		private bool wasPlayerForGPose;

		public ActorViewModel(IntPtr pointer)
			: base(pointer, null)
		{
		}

		[ModelField] public string Name { get; set; } = string.Empty;
		[ModelField] public int DataId { get; set; }
		[ModelField][Refresh] public ActorTypes ObjectKind { get; set; }
		[ModelField] public byte SubKind { get; set; }
		[ModelField][Refresh] public AppearanceViewModel? Customize { get; set; }
		[ModelField][Refresh] public int ModelType { get; set; }
		[ModelField][Refresh] public RenderModes RenderMode { get; set; }
		[ModelField] public float Transparency { get; set; }
		[ModelField][Refresh] public EquipmentViewModel? Equipment { get; set; }
		[ModelField][Refresh] public WeaponViewModel? MainHand { get; set; }
		[ModelField][Refresh] public WeaponViewModel? OffHand { get; set; }
		[ModelField] public ModelViewModel? ModelObject { get; set; }

		public bool AutomaticRefreshEnabled { get; set; } = true;
		public bool IsRefreshing { get; set; } = false;
		public bool PendingRefresh { get; set; } = false;

		public string Id => this.model.Id;

		[AlsoNotifyFor(nameof(ActorViewModel.DisplayName))]
		public string? Nickname
		{
			get
			{
				if (nicknameLookup.ContainsKey(this.Id))
					return nicknameLookup[this.Id];

				return null;
			}

			set
			{
				if (value == null)
				{
					if (nicknameLookup.ContainsKey(this.Id))
					{
						nicknameLookup.Remove(this.Id);
					}
				}
				else
				{
					if (!nicknameLookup.ContainsKey(this.Id))
						nicknameLookup.Add(this.Id, string.Empty);

					nicknameLookup[this.Id] = value;
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

		public void OnRetargeted()
		{
			if (this.Customize == null)
				return;

			GposeService gpose = GposeService.Instance;

			if (gpose.IsGpose && gpose.IsChangingState)
			{
				// Entering gpose
				if (this.ObjectKind == ActorTypes.Player)
				{
					this.wasPlayerForGPose = true;
					this.SetObjectKindDirect(ActorTypes.BattleNpc);
				}
			}
			else if (gpose.IsGpose && !gpose.IsChangingState)
			{
				// Entered gpose
				if (this.wasPlayerForGPose)
				{
					this.SetObjectKindDirect(ActorTypes.Player);
				}
			}
			else if (!gpose.IsGpose && !gpose.IsChangingState)
			{
				// left gpose
				if (this.wasPlayerForGPose)
				{
					this.SetObjectKindDirect(ActorTypes.Player);
					this.wasPlayerForGPose = false;
				}
			}
		}

		/// <summary>
		/// Refresh the actor to force the game to load any changed values for appearance.
		/// </summary>
		public void Refresh()
		{
			this.refreshDelay = RefreshDelay;

			if (this.refreshTask == null || this.refreshTask.IsCompleted)
			{
				this.refreshTask = Task.Run(this.RefreshTask);
			}
		}

		public void SetObjectKindDirect(ActorTypes type)
		{
			if (this.Pointer == null)
				return;

			IntPtr actorPointer = (IntPtr)this.Pointer;
			IntPtr objectKindPointer = actorPointer + Actor.ObjectKindOffset;
			MemoryService.Write(objectKindPointer, (byte)type, "Set ObjectKind Direct");
		}

		/// <summary>
		/// Refresh the actor to force the game to load any changed values for appearance.
		/// </summary>
		public async Task RefreshAsync()
		{
			if (this.IsRefreshing)
				return;

			if (this.Pointer == null)
				return;

			this.IsRefreshing = true;
			MemoryModes oldMode = this.MemoryMode;
			this.MemoryMode = MemoryModes.Read;

			// Use direct pointers here so that we can write the values we need to update
			// for the refresh without the rest of the model's values, as writing most of the other
			// values will crash the game.
			IntPtr actorPointer = (IntPtr)this.Pointer;
			IntPtr objectKindPointer = actorPointer + Actor.ObjectKindOffset;
			IntPtr renderModePointer = actorPointer + Actor.RenderModeOffset;

			if (this.ObjectKind == ActorTypes.Player)
			{
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.BattleNpc, "Actor Refresh (1 / 5)");

				MemoryService.Write(renderModePointer, (int)RenderModes.Unload, "Actor Refresh (2 / 5)");
				await Task.Delay(75);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw, "Actor Refresh (3 / 5)");
				await Task.Delay(75);
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.Player, "Actor Refresh (4 / 5)");
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw, "Actor Refresh (5 / 5)");
			}
			else
			{
				MemoryService.Write(renderModePointer, (int)RenderModes.Unload, "Actor Refresh (1 / 2)");
				await Task.Delay(75);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw, "Actor Refresh (2 / 2)");
			}

			await Task.Delay(75);

			this.IsRefreshing = false;
			this.MemoryMode = oldMode;
		}

		public async Task ConvertToPlayer()
		{
			this.Nickname = this.Name + " (" + this.ObjectKind + ")";

			if (this.ObjectKind == ActorTypes.EventNpc)
			{
				this.ObjectKind = ActorTypes.Player;
				await this.RefreshAsync();

				if (this.ModelType != 0)
				{
					this.ModelType = 0;
				}
			}

			// Carbuncles get model type set to player (but not actor type!)
			if (this.ObjectKind == ActorTypes.BattleNpc && (this.ModelType == 409 || this.ModelType == 410 || this.ModelType == 412))
			{
				this.ModelType = 0;
			}

			await DefaultCharacterFile.Default.Apply(this, Files.CharacterFile.SaveModes.All);
		}

		protected override void OnViewToModel(string fieldName, object? value)
		{
			// Do not allow actor view model changes to push into memory while we are refreshing.
			if (this.IsRefreshing)
				return;

			base.OnViewToModel(fieldName, value);
		}

		protected override bool HandleViewToModelUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			if (this.AutomaticRefreshEnabled && RefreshAttribute.IsSet(viewModelProperty))
			{
				Log.Debug($"Triggering actor refresh due to changed property: {viewModelProperty.Name}");
				this.Refresh();
			}

			return base.HandleViewToModelUpdate(viewModelProperty, modelField);
		}

		private async Task RefreshTask()
		{
			// Double loops to handle case where a refresh delay was added
			// while the refresh was running
			while (this.refreshDelay > 0)
			{
				this.PendingRefresh = true;

				while (this.refreshDelay > 0)
				{
					await Task.Delay(10);
					this.refreshDelay -= 10;
				}

				this.PendingRefresh = false;
				await this.RefreshAsync();
			}
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
