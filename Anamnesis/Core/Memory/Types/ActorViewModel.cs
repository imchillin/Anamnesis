// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using System.Threading.Tasks;
	using Anamnesis.Character;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ActorViewModel : ActorBasicViewModel
	{
		private const short RefreshDelay = 250;

		private short refreshDelay;
		private Task? refreshTask;
		private IntPtr? previousPlayerPointerBeforeGPose;

		public ActorViewModel(IntPtr pointer)
			: base(pointer)
		{
		}

		[ModelField] [Refresh] public CustomizeViewModel? Customize { get; set; }
		[ModelField] [Refresh] public EquipmentViewModel? Equipment { get; set; }
		[ModelField] [Refresh] public WeaponViewModel? MainHand { get; set; }
		[ModelField] [Refresh] public WeaponViewModel? OffHand { get; set; }
		[ModelField] public ModelViewModel? ModelObject { get; set; }

		public bool AutomaticRefreshEnabled { get; set; } = true;
		public bool IsRefreshing { get; set; } = false;
		public bool PendingRefresh { get; set; } = false;

		public bool CanTick()
		{
			return !this.PendingRefresh && !this.IsRefreshing;
		}

		public override void OnRetargeted()
		{
			base.OnRetargeted();

			if (this.Customize == null)
				return;

			GposeService gpose = GposeService.Instance;

			if (gpose.IsGpose && gpose.IsChangingState)
			{
				// Entering gpose
				if (this.ObjectKind == ActorTypes.Player)
				{
					this.previousPlayerPointerBeforeGPose = this.Pointer;
					this.SetObjectKindDirect(ActorTypes.BattleNpc, this.Pointer);

					// Sanity check that we do get turned back into a player
					Task.Run(async () =>
					{
						await Task.Delay(3000);
						this.SetObjectKindDirect(ActorTypes.Player, this.previousPlayerPointerBeforeGPose);
					});
				}
			}
			else if (gpose.IsGpose && !gpose.IsChangingState)
			{
				// Entered gpose
				if (this.previousPlayerPointerBeforeGPose != null)
				{
					this.SetObjectKindDirect(ActorTypes.Player, this.previousPlayerPointerBeforeGPose);
					this.SetObjectKindDirect(ActorTypes.Player, this.Pointer);
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

		/// <summary>
		/// Refresh the actor to force the game to load any changed values for appearance.
		/// </summary>
		public async Task RefreshAsync()
		{
			lock (this)
			{
				if (this.IsRefreshing)
				{
					return;
				}
			}

			if (this.Pointer == null)
				return;

			// Don't perform actor refreshes while in gpose
			if (GposeService.Instance.IsGpose)
				return;

			this.IsRefreshing = true;
			MemoryModes oldMode = this.MemoryMode;
			this.MemoryMode = MemoryModes.Read;

			// Use direct pointers here so that we can write the values we need to update
			// for the refresh without the rest of the model's values, as writing most of the other
			// values will crash the game.
			IntPtr actorPointer = (IntPtr)this.Pointer;
			IntPtr objectKindPointer = actorPointer + this.ObjectKindOffset;
			IntPtr renderModePointer = actorPointer + this.RenderModeOffset;

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
			if (this.Pointer == null)
				return;

			this.MemoryMode = MemoryModes.None;

			IntPtr actorPointer = (IntPtr)this.Pointer;
			IntPtr objectKindPointer = actorPointer + this.ObjectKindOffset;
			IntPtr modelTypePointer = actorPointer + this.ModelTypeOffset;

			// Carbuncles get model type set to player (but not actor type!)
			if (this.ObjectKind == ActorTypes.BattleNpc && (this.ModelType == 409 || this.ModelType == 410 || this.ModelType == 412))
			{
				MemoryService.Write(modelTypePointer, 0, "Convert to player");
			}

			this.MemoryMode = MemoryModes.ReadWrite;

			await DefaultCharacterFile.Default.Apply(this, Files.CharacterFile.SaveModes.All);

			if (this.ObjectKind == ActorTypes.EventNpc)
			{
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.Player, "Convert to player");
				await this.RefreshAsync();
			}
		}

		protected override void OnViewToModel(string fieldName, object? value)
		{
			// Do not allow actor view model changes to push into memory while we are refreshing.
			if (this.IsRefreshing)
				return;

			////if ((fieldName == nameof(Actor.MainHand) || fieldName == nameof(Actor.OffHand))
			////	&& this.Model?.ClassJob >= 8 && this.Model?.ClassJob <= 18)
			////	return;

			base.OnViewToModel(fieldName, value);
		}

		protected override bool HandleViewToModelUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			if (this.AutomaticRefreshEnabled && RefreshAttribute.IsSet(viewModelProperty))
			{
				Log.Debug($"Triggering actor refresh due to changed property: {viewModelProperty.Name}");
				this.Refresh();
			}

			////if ((modelField.Name == nameof(Actor.MainHand) || modelField.Name == nameof(Actor.OffHand))
			////	&& this.Model?.ClassJob >= 8 && this.Model?.ClassJob <= 18)
			////	return false;

			return base.HandleViewToModelUpdate(viewModelProperty, modelField);
		}

		private async Task RefreshTask()
		{
			// Double loops to handle case where a refresh delay was added
			// while the refresh was running
			while (this.refreshDelay > 0)
			{
				lock (this)
					this.PendingRefresh = true;

				while (this.refreshDelay > 0)
				{
					await Task.Delay(10);
					this.refreshDelay -= 10;
				}

				lock (this)
					this.PendingRefresh = false;

				await this.RefreshAsync();
			}
		}
	}
}
