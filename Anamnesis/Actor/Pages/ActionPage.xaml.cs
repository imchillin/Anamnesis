// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Pages;

using Anamnesis.Actor.Views;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Interfaces;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

[AddINotifyPropertyChangedInterface]
public partial class ActionPage : UserControl
{
	public ActionPage()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		this.LipSyncTypes = GenerateLipList();

		HotkeyService.RegisterHotkeyHandler("ActionPage.ResumeAll", this.OnResumeAll);
		HotkeyService.RegisterHotkeyHandler("ActionPage.PauseAll", this.OnPauseAll);
	}

	public static GposeService GposeService => GposeService.Instance;
	public static AnimationService AnimationService => AnimationService.Instance;
	public static PoseService PoseService => PoseService.Instance;
	public ObjectHandle<ActorMemory>? Actor { get; private set; }
	public IEnumerable<ActionTimeline> LipSyncTypes { get; private set; }

	public UserAnimationOverride AnimationOverride { get; private set; } = new();

	public ConditionalWeakTable<ObjectHandle<ActorMemory>, UserAnimationOverride> UserAnimationOverrides { get; private set; } = [];

	private static IEnumerable<ActionTimeline> GenerateLipList()
	{
		// Grab "no animation" and all "speak/" animations, which are the only ones valid in this slot
		IEnumerable<ActionTimeline> lips = GameDataService.ActionTimelines.Where(x => x.AnimationId == 0 || (x.Key?.StartsWith("speak/") ?? false));
		return lips;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ObjectHandle<ActorMemory>);
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ObjectHandle<ActorMemory>);
	}

	private void OnActorChanged(ObjectHandle<ActorMemory>? actorHandle)
	{
		if (this.Actor != null) // Save the current settings
			this.UserAnimationOverrides.AddOrUpdate(this.Actor, this.AnimationOverride);

		this.Actor = actorHandle;

		Application.Current.Dispatcher.InvokeAsync(() =>
		{
			this.IsEnabled = this.Actor?.Do(a => a.ObjectKind.IsSupportedType()) == true;
		});

		if (actorHandle != null)
		{
			if (this.UserAnimationOverrides.TryGetValue(actorHandle, out UserAnimationOverride? userAnimationOverride))
			{
				this.AnimationOverride = userAnimationOverride;
			}
			else
			{
				ushort? baseAnimId = actorHandle?.Do(a =>
				{
					if (a.Animation == null || a.Animation.AnimationIds == null)
						return default;

					return a.Animation!.AnimationIds![(int)AnimationMemory.AnimationSlots.FullBody].Value;
				});

				this.AnimationOverride = new()
				{
					BaseAnimationId = baseAnimId ?? 0,
					BlendAnimationId = 0,
				};
			}
		}
	}

	private void OnBaseAnimationSearchClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		AnimationSelector animSelector = SelectorDrawer.Show<AnimationSelector, IAnimation>(null, (animation) =>
		{
			if (animation == null || animation.Timeline == null)
				return;

			this.AnimationOverride.BaseAnimationId = animation.Timeline.Value.AnimationId;
		});

		animSelector.LocalAnimationSlotFilter = new()
		{
			IncludeBlendable = false,
			IncludeFullBody = true,
			SlotsLocked = true,
		};
	}

	private void OnBlendAnimationSearchClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		AnimationSelector animSelector = SelectorDrawer.Show<AnimationSelector, IAnimation>(null, (animation) =>
		{
			if (animation == null || animation.Timeline == null)
				return;

			this.AnimationOverride.BlendAnimationId = animation.Timeline.Value.AnimationId;
		});

		animSelector.LocalAnimationSlotFilter = new()
		{
			IncludeBlendable = true,
			IncludeFullBody = true,
			SlotsLocked = false,
		};
	}

	private void OnApplyOverrideAnimation(object sender, RoutedEventArgs e)
	{
		this.Actor?.Do(actor =>
		{
			AnimationService.ApplyAnimationOverride(actor, this.AnimationOverride.BaseAnimationId, this.AnimationOverride.Interrupt);
		});
	}

	private void OnDrawWeaponOverrideAnimation(object sender, RoutedEventArgs e)
	{
		this.Actor?.Do(AnimationService.DrawWeapon);
	}

	private async void OnBlendAnimation(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		await this.Actor.DoAsync(async actor =>
		{
			await AnimationService.BlendAnimation(actor, this.AnimationOverride.BlendAnimationId);
			this.AnimationBlendButton.Focus(); // Refocus on the button as the blend lock changes state in the function call above
		});
	}

	private void OnIdleOverrideAnimation(object sender, RoutedEventArgs e)
	{
		this.Actor?.Do(AnimationService.ApplyIdle);
	}

	private void OnResetOverrideAnimation(object sender, RoutedEventArgs e)
	{
		this.Actor?.Do(AnimationService.ResetAnimationOverride);
	}

	private void OnResumeAll(object sender, RoutedEventArgs e) => this.OnResumeAll();

	private void OnResumeAll()
	{
		if (!GposeService.Instance.IsGpose)
			return;

		AnimationService.Instance.SpeedControlEnabled = true;

		foreach (var target in TargetService.Instance.PinnedActors.ToList())
		{
			if (!target.IsValid)
				continue;

			target.Memory?.Do(a => a.Animation!.LinkSpeeds = true);
			target.Memory?.Do(a => a.Animation!.Speeds![0].Value = 1.0f);
		}
	}

	private void OnPauseAll(object sender, RoutedEventArgs e) => this.OnPauseAll();

	private void OnPauseAll()
	{
		if (!GposeService.Instance.IsGpose)
			return;

		AnimationService.Instance.SpeedControlEnabled = true;

		foreach (var target in TargetService.Instance.PinnedActors.ToList())
		{
			if (!target.IsValid)
				continue;

			target.Memory?.Do(a => a.Animation!.LinkSpeeds = true);
			target.Memory?.Do(a => a.Animation!.Speeds![0].Value = 0.0f);
		}
	}

	[AddINotifyPropertyChangedInterface]
	public class UserAnimationOverride
	{
		public ushort BaseAnimationId { get; set; } = 0;
		public ushort BlendAnimationId { get; set; } = 0;
		public bool Interrupt { get; set; } = true;
	}
}
