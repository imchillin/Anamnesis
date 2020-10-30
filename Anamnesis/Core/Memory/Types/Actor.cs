// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
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
		[FieldOffset(0x1704)] public float Transparency;
		[FieldOffset(0x1708)] public Equipment Equipment;
		[FieldOffset(0x17B8)] public Appearance Customize;
		[FieldOffset(0x17F8)] public int BattleNpcTargetActorId;
		[FieldOffset(0x1868)] public int NameId;
		[FieldOffset(0x1888)] public int ModelType;

		public string Id => this.Name + this.NameId;
	}

	[AddINotifyPropertyChangedInterface]
	public class ActorViewModel : MemoryViewModelBase<Actor>
	{
		private const short RefreshDelay = 250;

		private static Dictionary<string, string> nicknameLookup = new Dictionary<string, string>();

		private short refreshDelay;
		private Task? refreshTask;

		public ActorViewModel(IntPtr pointer)
			: base(pointer)
		{
		}

		[ModelField] public string Name { get; set; } = string.Empty;
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
		[ModelField] public float Transparency { get; set; }
		[ModelField] public EquipmentViewModel? Equipment { get; set; }
		[ModelField] public WeaponViewModel? MainHand { get; set; }
		[ModelField] public WeaponViewModel? OffHand { get; set; }

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

		public bool IsCustomizable()
		{
			return ModelTypeService.IsCustomizable(this.ModelType) && this.Customize != null;
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

		/// <summary>
		/// Refresh the actor to force the game to load any changed values for appearance.
		/// </summary>
		public async Task RefreshAsync()
		{
			if (this.Pointer == null)
				return;

			this.IsRefreshing = true;
			MemoryModes oldMode = this.MemoryMode;
			this.MemoryMode = MemoryModes.Read;

			// Use direct pointers here so that we can write the values we need to update
			// for the refresh without the rest of the model's values, as writing most of the other
			// values will crash the game.
			IntPtr actorPointer = (IntPtr)this.Pointer;
			IntPtr objectKindPointer = actorPointer + 0x008c;
			IntPtr renderModePointer = actorPointer + 0x0104;

			if (this.ObjectKind == ActorTypes.Player)
			{
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.BattleNpc);

				MemoryService.Write(renderModePointer, (int)RenderModes.Unload);
				await Task.Delay(50);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw);
				await Task.Delay(50);
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.Player);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw);
			}
			else
			{
				MemoryService.Write(renderModePointer, (int)RenderModes.Unload);
				await Task.Delay(50);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw);
			}

			await Task.Delay(50);

			this.IsRefreshing = false;
			this.MemoryMode = oldMode;
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
			if (this.AutomaticRefreshEnabled && ShouldRefreshForProperty(viewModelProperty))
			{
				this.Refresh();
			}

			return base.HandleViewToModelUpdate(viewModelProperty, modelField);
		}

		private static bool ShouldRefreshForProperty(PropertyInfo property)
		{
			if (property.Name == nameof(ActorViewModel.Transparency))
				return false;

			return true;
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
	}
}
