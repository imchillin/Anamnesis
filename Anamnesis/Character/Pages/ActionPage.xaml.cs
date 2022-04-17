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
			this.LipSyncTypes = GameDataService.ActionTimelines.Where(x => x.AnimationId == 0 || (x.Key?.StartsWith("speak/") ?? false));
		}

		public GposeService GposeService => GposeService.Instance;
		public AnimationService AnimationService => AnimationService.Instance;
		public PoseService PoseService => PoseService.Instance;
		public ActorMemory? Actor { get; private set; }
		public IEnumerable<ActionTimeline> LipSyncTypes { get; private set; }
		public UserAnimationOverride AnimationOverride { get; private set; } = new();

		public ConditionalWeakTable<ActorMemory, UserAnimationOverride> UserAnimationOverrides { get; private set; } = new();

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorMemory);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
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
					if (actor.IsAnimationOverridden == true)
					{
						this.AnimationOverride = new();
						this.AnimationOverride.BaseAnimationId = actor.BaseAnimationOverride;
						this.AnimationOverride.BlendAnimationId = 0;
					}
					else
					{
						this.AnimationOverride = new();
					}
				}
			}
		}

		private void OnBaseAnimationSearchClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			SelectorDrawer.Show<AnimationSelector, IAnimation>(null, (animation) =>
			{
				if (animation == null || animation.Timeline == null)
					return;

				this.AnimationOverride.BaseAnimationId = animation.Timeline.AnimationId;
			});
		}

		private void OnBlendAnimationSearchClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			SelectorDrawer.Show<AnimationSelector, IAnimation>(null, (animation) =>
			{
				if (animation == null || animation.Timeline == null)
					return;

				this.AnimationOverride.BlendAnimationId = animation.Timeline.AnimationId;
			});
		}

		private void OnApplyOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.ApplyAnimationOverride(this.Actor, this.AnimationOverride.BaseAnimationId, this.AnimationOverride.Interrupt);
		}

		private void OnDrawWeaponOverrideAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.DrawWeapon(this.Actor);
		}

		private void OnBlendAnimation(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.AnimationService.BlendAnimation(this.Actor, this.AnimationOverride.BlendAnimationId);
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

		private void OnPauseBase(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.Actor.BaseAnimationSpeed = 0.0f;
		}

		private void OnResumeBase(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.Actor.BaseAnimationSpeed = 1.0f;
		}

		private void OnPauseLips(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.Actor.LipAnimationSpeed = 0.0f;
		}

		private void OnResumeLips(object sender, RoutedEventArgs e)
		{
			if (this.Actor?.IsValid != true)
				return;

			this.Actor.LipAnimationSpeed = 1.0f;
		}

		[AddINotifyPropertyChangedInterface]
		public class UserAnimationOverride
		{
			public ushort BaseAnimationId { get; set; } = 0;
			public ushort BlendAnimationId { get; set; } = 0;
			public bool Interrupt { get; set; } = true;
		}
	}
}
