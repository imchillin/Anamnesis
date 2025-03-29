// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using System.ComponentModel;
using System.Threading.Tasks;
using XivToolsWpf;

public abstract class AnimationSelectorDrawer : SelectorDrawer<IAnimation>
{
}

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class AnimationSelector : AnimationSelectorDrawer
{
	public AnimationSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
		GlobalAnimationTypeFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.LocalAnimationSlotFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public static AnimationTypeFilter GlobalAnimationTypeFilter { get; set; } = new();
	public AnimationSlotFilter LocalAnimationSlotFilter { get; set; } = new();

	protected override Task LoadItems()
	{
		if (GameDataService.Emotes != null)
			this.LoadEmotes();

		if (GameDataService.ActionTimelines != null)
			this.AddItems(GameDataService.Actions.ToEnumerable());

		if (GameDataService.ActionTimelines != null)
			this.AddItems(GameDataService.ActionTimelines.ToEnumerable());

		return Task.CompletedTask;
	}

	protected override bool Filter(IAnimation animation, string[]? search = null)
	{
		// Filter out any that are clearly invalid
		if (string.IsNullOrEmpty(animation.Name) || animation.Timeline == null || string.IsNullOrEmpty(animation.Timeline.Value.Key))
			return false;

		if (!GlobalAnimationTypeFilter.IncludeEmotes && animation is EmoteEntry)
			return false;

		if (!GlobalAnimationTypeFilter.IncludeActions && animation is Action)
			return false;

		if (!GlobalAnimationTypeFilter.IncludeRaw && animation is ActionTimeline)
			return false;

		if (!this.LocalAnimationSlotFilter.IncludeBlendable && animation.Timeline.Value.Slot != Memory.AnimationMemory.AnimationSlots.FullBody)
			return false;

		if (!this.LocalAnimationSlotFilter.IncludeFullBody && animation.Timeline.Value.Slot == Memory.AnimationMemory.AnimationSlots.FullBody)
			return false;

		bool matches = false;
		matches |= SearchUtility.Matches(animation.Name, search);

		if (animation.Timeline != null)
		{
			matches |= SearchUtility.Matches(animation.Timeline.Value.Key, search);
			matches |= SearchUtility.Matches(animation.Timeline.Value.RowId.ToString(), search);
		}

		return matches;
	}

	protected override int Compare(IAnimation animA, IAnimation animB)
	{
		// Emotes and Actions to the top
		if (animA is EmoteEntry && animB is not EmoteEntry)
			return -1;

		if (animA is not EmoteEntry && animB is EmoteEntry)
			return 1;

		if (animA is Action && animB is not Action)
			return -1;

		if (animA is not Action && animB is Action)
			return 1;

		if (animA.Name == null)
			return 1;

		if (animB.Name == null)
			return -1;

		return -animB.Name.CompareTo(animA.Name);
	}

	private void LoadEmotes()
	{
		if (GameDataService.Emotes == null)
			return;

		foreach (var emote in GameDataService.Emotes)
		{
			if (emote.Name == null)
				continue;

			if (emote.LoopTimeline == null)
				continue;

			// Add the loop
			this.AddItem(new EmoteEntry(emote.Name, emote.LoopTimeline.Value, IAnimation.AnimationPurpose.Standard, emote.Icon));

			// Check for an intro animation
			if (emote.IntroTimeline != null && emote.IntroTimeline.Value.RowId != emote.LoopTimeline.Value.RowId)
			{
				this.AddItem(new EmoteEntry(emote.Name, (ActionTimeline)emote.IntroTimeline, IAnimation.AnimationPurpose.Intro, emote.Icon));
			}

			// Check for an upper body variant
			if (emote.UpperBodyTimeline != null && emote.UpperBodyTimeline.Value.RowId != emote.LoopTimeline.Value.RowId)
			{
				this.AddItem(new EmoteEntry(emote.Name, (ActionTimeline)emote.UpperBodyTimeline, IAnimation.AnimationPurpose.Blend, emote.Icon));
			}

			// Check for a ground specific animation
			if (emote.GroundTimeline != null && emote.UpperBodyTimeline != null && emote.GroundTimeline.Value.RowId != emote.LoopTimeline.Value.RowId && emote.GroundTimeline.Value.RowId != emote.UpperBodyTimeline.Value.RowId)
			{
				this.AddItem(new EmoteEntry(emote.Name, (ActionTimeline)emote.GroundTimeline, IAnimation.AnimationPurpose.Ground, emote.Icon));
			}

			// Check for a chair specific animation
			if (emote.ChairTimeline != null && emote.UpperBodyTimeline != null && emote.ChairTimeline.Value.RowId != emote.LoopTimeline.Value.RowId && emote.ChairTimeline.Value.RowId != emote.UpperBodyTimeline.Value.RowId)
			{
				this.AddItem(new EmoteEntry(emote.Name, (ActionTimeline)emote.ChairTimeline, IAnimation.AnimationPurpose.Chair, emote.Icon));
			}
		}
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.LocalAnimationSlotFilter))
			return;

		this.FilterItems();
	}

	public class EmoteEntry : IAnimation
	{
		public EmoteEntry(string displayName, ActionTimeline timeline, IAnimation.AnimationPurpose purpose, ImageReference? icon)
		{
			this.Name = displayName;
			this.Purpose = purpose;
			this.Timeline = timeline;
			this.Icon = icon;
		}

		public string Name { get; private set; }

		public IAnimation.AnimationPurpose Purpose { get; private set; }

		public ImageReference? Icon { get; private set; }

		public ActionTimeline? Timeline { get; private set; }
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