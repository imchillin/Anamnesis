// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Pages
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Character.Views;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Interfaces;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;
	using Anamnesis.Styles;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public partial class ActionPage : UserControl
	{
		public ActionPage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			// Grab "no animation" and all "speak/" animations, which are the only ones valid in this slot
			this.LipSyncTypes = GameDataService.ActionTimelines.Where(x => x.RowId == 0 || (x.Name?.StartsWith("speak/") ?? false));
		}

		public GposeService GposeService => GposeService.Instance;
		public AnimationService AnimationService => AnimationService.Instance;
		public PoseService PoseService => PoseService.Instance;
		public ActorMemory? Actor { get; private set; }
		public IEnumerable<ActionTimeline> LipSyncTypes { get; private set; }
		public UserAnimationOverride AnimationOverride { get; private set; } = new();

		public IAnimation? LastAnimationSelected { get; set; } = null;
		public ConditionalWeakTable<ActorMemory, UserAnimationOverride> UserAnimationOverrides { get; private set; } = new();

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorMemory);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!this.IsVisible)
				return;

			this.OnActorChanged(this.DataContext as ActorMemory);
		}

		private void OnActorChanged(ActorMemory? actor)
		{
			if(this.Actor != null) // Save the current settings
				this.UserAnimationOverrides.AddOrUpdate(this.Actor, this.AnimationOverride);

			this.Actor = actor;

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				bool hasValidSelection = actor != null && actor.ObjectKind.IsSupportedType();
				this.IsEnabled = hasValidSelection;
			});

			if(actor != null)
			{
				if (this.UserAnimationOverrides.TryGetValue(actor, out UserAnimationOverride? userAnimationOverride))
				{
					this.AnimationOverride = userAnimationOverride;
				}
				else
				{
					if (actor.IsAnimationOverriden == true)
					{
						this.AnimationOverride = new();
						this.AnimationOverride.AnimationId = actor.AnimationOverride;
						this.AnimationOverride.Speed = actor.AnimationSpeed;
					}
					else
					{
						this.AnimationOverride = new();
					}
				}
			}
		}

		private void OnAnimationSearchClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			SelectorDrawer.Show<AnimationSelector, IAnimation>(this.LastAnimationSelected, (animation) =>
			{
				if (animation == null)
					return;

				this.LastAnimationSelected = animation;
				this.AnimationOverride.AnimationId = (ushort)animation.ActionTimelineRowId;
			});
		}

		private void OnApplyOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.ApplyAnimationOverride(this.Actor, this.AnimationOverride.AnimationId, this.AnimationOverride.Speed, this.AnimationOverride.Interrupt);
		}

		private void OnDrawWeaponOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.DrawWeapon(this.Actor);
		}

		private void OnIdleOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.ApplyIdle(this.Actor);
		}

		private void OnResetOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.ResetAnimationOverride(this.Actor);
		}

		private void OnPauseOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			if (this.Actor.AnimationSpeed == 0)
				return;

			this.AnimationService.PauseActor(this.Actor);
		}

		private void OnUnpauseOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			if (this.Actor.AnimationSpeed != 0)
				return;

			this.AnimationService.UnpauseActor(this.Actor);
		}

		[AddINotifyPropertyChangedInterface]
		public class UserAnimationOverride
		{
			public ushort AnimationId { get; set; } = 0;
			public float Speed { get; set; } = 1.0f;
			public bool Interrupt { get; set; } = true;
		}
	}
}
