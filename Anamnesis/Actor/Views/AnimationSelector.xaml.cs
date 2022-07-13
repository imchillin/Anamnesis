// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using XivToolsWpf;
using XivToolsWpf.Selectors;

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class AnimationSelector : UserControl, INotifyPropertyChanged
{
	public AnimationSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
		GlobalAnimationTypeFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.LocalAnimationSlotFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public static AnimationTypeFilter GlobalAnimationTypeFilter { get; set; } = new();
	public AnimationSlotFilter LocalAnimationSlotFilter { get; set; } = new();

	protected Task LoadItems()
	{
		if (GameDataService.Emotes != null)
			this.LoadEmotes();

		if (GameDataService.ActionTimelines != null)
			this.Selector.AddItems(GameDataService.Actions);

		if (GameDataService.ActionTimelines != null)
			this.Selector.AddItems(GameDataService.ActionTimelines);

		return Task.CompletedTask;
	}

	private bool Filter(object item, string[]? search)
	{
		if (item is not IAnimation animation)
			return false;

		// Filter out any that are clearly invalid
		if (string.IsNullOrEmpty(animation.DisplayName) || animation.Timeline == null || string.IsNullOrEmpty(animation.Timeline.Key))
			return false;

		if (!GlobalAnimationTypeFilter.IncludeEmotes && animation is EmoteEntry)
			return false;

		if (!GlobalAnimationTypeFilter.IncludeActions && animation is Action)
			return false;

		if (!GlobalAnimationTypeFilter.IncludeRaw && animation is ActionTimeline)
			return false;

		if (!this.LocalAnimationSlotFilter.IncludeBlendable && animation.Timeline.Slot != Memory.AnimationMemory.AnimationSlots.FullBody)
			return false;

		if (!this.LocalAnimationSlotFilter.IncludeFullBody && animation.Timeline.Slot == Memory.AnimationMemory.AnimationSlots.FullBody)
			return false;

		bool matches = false;
		matches |= SearchUtility.Matches(animation.DisplayName, search);

		if (animation.Timeline != null)
		{
			matches |= SearchUtility.Matches(animation.Timeline.Key, search);
			matches |= SearchUtility.Matches(animation.Timeline.AnimationId.ToString(), search);
		}

		return matches;
	}

	private int Sort(object itemA, object itemB)
	{
		if (itemA is not IAnimation animA)
			return 0;

		if (itemB is not IAnimation animB)
			return 0;

		// Emotes and Actions to the top
		if (animA is EmoteEntry && animB is not EmoteEntry)
			return -1;

		if (animA is not EmoteEntry && animB is EmoteEntry)
			return 1;

		if (animA is Action && animB is not Action)
			return -1;

		if (animA is not Action && animB is Action)
			return 1;

		if (animA.DisplayName == null)
			return 1;

		if (animB.DisplayName == null)
			return -1;

		return -animB.DisplayName.CompareTo(animA.DisplayName);
	}

	private void LoadEmotes()
	{
		if (GameDataService.Emotes == null)
			return;

		foreach (var emote in GameDataService.Emotes)
		{
			if (emote == null)
				continue;

			if (emote.DisplayName == null)
				continue;

			if (emote.LoopTimeline == null)
				continue;

			// Add the loop
			this.Selector.AddItem(new EmoteEntry(emote.DisplayName, emote.LoopTimeline, IAnimation.AnimationPurpose.Standard, emote.Icon));

			// Check for an intro animation
			if (emote.IntroTimeline != null && emote.IntroTimeline != emote.LoopTimeline)
			{
				this.Selector.AddItem(new EmoteEntry(emote.DisplayName, emote.IntroTimeline, IAnimation.AnimationPurpose.Intro, emote.Icon));
			}

			// Check for an upper body variant
			if (emote.UpperBodyTimeline != null && emote.UpperBodyTimeline != emote.LoopTimeline)
			{
				this.Selector.AddItem(new EmoteEntry(emote.DisplayName, emote.UpperBodyTimeline, IAnimation.AnimationPurpose.Blend, emote.Icon));
			}

			// Check for a ground specific animation
			if (emote.GroundTimeline != null && emote.GroundTimeline != emote.LoopTimeline && emote.GroundTimeline != emote.UpperBodyTimeline)
			{
				this.Selector.AddItem(new EmoteEntry(emote.DisplayName, emote.GroundTimeline, IAnimation.AnimationPurpose.Ground, emote.Icon));
			}

			// Check for a chair specific animation
			if (emote.ChairTimeline != null && emote.ChairTimeline != emote.LoopTimeline && emote.ChairTimeline != emote.UpperBodyTimeline)
			{
				this.Selector.AddItem(new EmoteEntry(emote.DisplayName, emote.ChairTimeline, IAnimation.AnimationPurpose.Chair, emote.Icon));
			}
		}
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.LocalAnimationSlotFilter))
			return;

		this.Selector.FilterItems();
	}

	public class EmoteEntry : IAnimation
	{
		public EmoteEntry(string displayName, ActionTimeline timeline, IAnimation.AnimationPurpose purpose, ImageReference? icon)
		{
			this.DisplayName = displayName;
			this.Purpose = purpose;
			this.Timeline = timeline;
			this.Icon = icon;
		}

		public string DisplayName { get; private set; }

		public IAnimation.AnimationPurpose Purpose { get; private set; }

		public ImageReference? Icon { get; private set; }

		public ActionTimeline Timeline { get; private set; }
	}

	[AddINotifyPropertyChangedInterface]
	public class AnimationTypeFilter : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IncludeEmotes { get; set; } = true;
		public bool IncludeActions { get; set; } = true;
		public bool IncludeRaw { get; set; } = true;
	}

	[AddINotifyPropertyChangedInterface]
	public class AnimationSlotFilter : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IncludeBlendable { get; set; } = true;
		public bool IncludeFullBody { get; set; } = true;
		public bool SlotsLocked { get; set; } = true;
	}
}