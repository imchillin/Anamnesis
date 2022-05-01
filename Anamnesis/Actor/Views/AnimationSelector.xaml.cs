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
		this.CurrentFilter = GlobalFilter;
		this.CurrentFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public AnimationFilter CurrentFilter { get; private set; }

	private static AnimationFilter GlobalFilter { get; set; } = new()
	{
		SlotsLocked = false,
	};

	public void ChangeFilter(AnimationFilter filter)
	{
		this.CurrentFilter.PropertyChanged -= this.OnSelfPropertyChanged;
		this.CurrentFilter = filter;
		this.CurrentFilter.PropertyChanged += this.OnSelfPropertyChanged;
	}

	protected override Task LoadItems()
	{
		if (GameDataService.Emotes != null)
			this.LoadEmotes();

		if (GameDataService.ActionTimelines != null)
			this.AddItems(GameDataService.Actions);

		if (GameDataService.ActionTimelines != null)
			this.AddItems(GameDataService.ActionTimelines);

		return Task.CompletedTask;
	}

	protected override bool Filter(IAnimation animation, string[]? search = null)
	{
		// Filter out any that are clearly invalid
		if (string.IsNullOrEmpty(animation.DisplayName) || animation.Timeline == null || string.IsNullOrEmpty(animation.Timeline.Key))
			return false;

		if (!this.CurrentFilter.IncludeEmotes && animation is EmoteEntry)
			return false;

		if (!this.CurrentFilter.IncludeActions && animation is Action)
			return false;

		if (!this.CurrentFilter.IncludeRaw && animation is ActionTimeline)
			return false;

		if (!this.CurrentFilter.IncludeBlendable && animation.Timeline.Slot != Memory.AnimationMemory.AnimationSlots.FullBody)
			return false;

		if (!this.CurrentFilter.IncludeFullBody && animation.Timeline.Slot == Memory.AnimationMemory.AnimationSlots.FullBody)
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
			this.AddItem(new EmoteEntry(emote.DisplayName, emote.LoopTimeline, IAnimation.AnimationPurpose.Standard, emote.Icon));

			// Check for an intro animation
			if (emote.IntroTimeline != null && emote.IntroTimeline != emote.LoopTimeline)
			{
				this.AddItem(new EmoteEntry(emote.DisplayName, emote.IntroTimeline, IAnimation.AnimationPurpose.Intro, emote.Icon));
			}

			// Check for an upper body variant
			if (emote.UpperBodyTimeline != null && emote.UpperBodyTimeline != emote.LoopTimeline)
			{
				this.AddItem(new EmoteEntry(emote.DisplayName, emote.UpperBodyTimeline, IAnimation.AnimationPurpose.Blend, emote.Icon));
			}

			// Check for a ground specific animation
			if (emote.GroundTimeline != null && emote.GroundTimeline != emote.LoopTimeline && emote.GroundTimeline != emote.UpperBodyTimeline)
			{
				this.AddItem(new EmoteEntry(emote.DisplayName, emote.GroundTimeline, IAnimation.AnimationPurpose.Ground, emote.Icon));
			}

			// Check for a chair specific animation
			if (emote.ChairTimeline != null && emote.ChairTimeline != emote.LoopTimeline && emote.ChairTimeline != emote.UpperBodyTimeline)
			{
				this.AddItem(new EmoteEntry(emote.DisplayName, emote.ChairTimeline, IAnimation.AnimationPurpose.Chair, emote.Icon));
			}
		}
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.CurrentFilter))
			return;

		this.FilterItems();
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
	public class AnimationFilter : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IncludeEmotes { get; set; } = true;
		public bool IncludeActions { get; set; } = true;
		public bool IncludeRaw { get; set; } = true;
		public bool IncludeFullBody { get; set; } = true;
		public bool IncludeBlendable { get; set; } = true;
		public bool SlotsLocked { get; set; } = true;
	}
}