// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.GameData.Interfaces;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using Anamnesis.Utils;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public partial class AnimationEditor : UserControl
	{
		private readonly Dictionary<ActorRef<ActorMemory>, AnimationOverride> activeOverrides = new();

		public AnimationEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public PoseService PoseService => PoseService.Instance;
		public GposeService GposeService => GposeService.Instance;

		public ActorRef<ActorMemory>? ActorRef { get; private set; }
		public AnimationService AnimationService => AnimationService.Instance;
		public ushort AnimationId { get; set; } = 8047;
		public float AnimationSpeed { get; set; } = 1.0f;

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.DataContext is ActorMemory actor)
			{
				this.ActorRef = new ActorRef<ActorMemory>(actor);
			}
			else
			{
				this.ActorRef = null;
			}

			this.RefreshUI();
		}

		private void RefreshUI()
		{
			this.activeOverrides.Where((i) => i.Key.IsValid).ToList().ForEach((i) => this.activeOverrides.Remove(i.Key));

			AnimationOverride? animOverride = null;

			if (this.ActorRef != null)
			{
				this.activeOverrides.TryGetValue(this.ActorRef, out animOverride);
			}

			this.AnimationId = animOverride?.AnimationId ?? 8047;
			this.AnimationSpeed = animOverride?.AnimationSpeed ?? 1.0f;
		}

		private void OnSearchClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<AnimationSelector, IAnimation>(null, (animation) =>
			{
				if (animation == null)
					return;

				this.AnimationId = (ushort)animation.ActionTimelineRowId;
				this.OnPlayClicked(null, null);
			});
		}

		private void OnDrawWeaponClicked(object sender, RoutedEventArgs e)
		{
			if (this.ActorRef == null)
				return;

			this.AnimationService.DrawWeapon(this.ActorRef);
		}

		private void OnIdleCharacterClicked(object sender, RoutedEventArgs e)
		{
			if (this.ActorRef == null)
				return;

			this.AnimationService.IdleCharacter(this.ActorRef);
		}

		private void OnPlayClicked(object? sender, RoutedEventArgs? e) => this.ApplyCurrentAnimation(true);

		private void OnQueueClicked(object? sender, RoutedEventArgs? e) => this.ApplyCurrentAnimation(false);

		private void OnPauseClicked(object? sender, RoutedEventArgs? e)
		{
			if (this.ActorRef == null)
				return;

			this.AnimationService.TogglePaused(this.ActorRef, this.AnimationSpeed);
		}

		private void ApplyCurrentAnimation(bool interrupt)
		{
			if (this.ActorRef == null || !this.ActorRef.IsValid)
				return;

			AnimationOverride animOverride = new()
			{
				AnimationId = this.AnimationId,
				AnimationSpeed = this.AnimationSpeed,
			};

			this.AnimationService.PlayAnimation(this.ActorRef, animOverride.AnimationId, animOverride.AnimationSpeed, interrupt);
			this.activeOverrides[this.ActorRef] = animOverride;
		}

		private void OnResetClicked(object sender, RoutedEventArgs e)
		{
			if (this.ActorRef == null)
				return;

			this.AnimationService.ResetAnimationOverride(this.ActorRef);
		}

		public class AnimationOverride
		{
			public ushort? AnimationId { get; set; } = null;
			public float? AnimationSpeed { get; set; } = null;
		}
	}
}