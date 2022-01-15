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
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public partial class AnimationEditor : UserControl
	{
		private readonly Dictionary<ActorMemory, AnimationOverride> activeOverrides = new();

		public AnimationEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public WorldService WorldService => WorldService.Instance;

		public ActorMemory? Actor { get; private set; }
		public AnimationService AnimationService => AnimationService.Instance;
		public ushort AnimationId { get; set; } = 8047;
		public float AnimationSpeed { get; set; } = 1.0f;

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.DataContext is ActorMemory actor)
			{
				this.Actor = actor;
			}
			else
			{
				this.Actor = null;
			}

			this.RefreshUI();
		}

		private void RefreshUI()
		{
			this.activeOverrides.Where((i) => i.Key.Address == IntPtr.Zero || !TargetService.IsActorInActorTable(i.Key.Address)).ToList().ForEach((i) => this.activeOverrides.Remove(i.Key));

			AnimationOverride? animOverride = null;

			if (this.Actor != null)
			{
				this.activeOverrides.TryGetValue(this.Actor, out animOverride);
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
			if (this.Actor == null)
				return;

			this.AnimationService.DrawWeapon(this.Actor);
		}

		private void OnPlayClicked(object? sender, RoutedEventArgs? e) => this.ApplyCurrentAnimation(true);

		private void OnQueueClicked(object? sender, RoutedEventArgs? e) => this.ApplyCurrentAnimation(false);

		private void ApplyCurrentAnimation(bool interrupt)
		{
			if (this.Actor == null)
				return;

			AnimationOverride animOverride = new()
			{
				AnimationId = this.AnimationId,
				AnimationSpeed = this.AnimationSpeed,
			};

			this.AnimationService.ApplyAnimationOverride(this.Actor, animOverride.AnimationId, animOverride.AnimationSpeed, interrupt);
			this.activeOverrides[this.Actor] = animOverride;
		}

		private void OnResetClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			this.AnimationService.ResetAnimationOverride(this.Actor);
		}

		public class AnimationOverride
		{
			public ushort? AnimationId { get; set; } = null;
			public float? AnimationSpeed { get; set; } = null;
		}
	}
}