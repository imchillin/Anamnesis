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

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class AnimationSelector : UserControl, SelectorDrawer.ISelectorView, INotifyPropertyChanged
{
	public AnimationSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
		this.Filter = GlobalFilter;
		this.Filter.PropertyChanged += this.OnSelfPropertyChanged;
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public event DrawerEvent? Close;
	public event DrawerEvent? SelectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;
	public AnimationFilter Filter { get; private set; }

	public IAnimation? Value
	{
		get
		{
			return (IAnimation?)this.Selector.Value;
		}

		set
		{
			this.Selector.Value = value;
		}
	}

	SelectorDrawer SelectorDrawer.ISelectorView.Selector
	{
		get
		{
			return this.Selector;
		}
	}

	private static AnimationFilter GlobalFilter { get; set; } = new()
	{
		SlotsLocked = false,
	};

	public void ChangeFilter(AnimationFilter filter)
	{
		this.Filter.PropertyChanged -= this.OnSelfPropertyChanged;
		this.Filter = filter;
		this.Filter.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public void OnClosed()
	{
	}

	private Task OnLoadItems()
	{
		if (GameDataService.Emotes != null)
			this.LoadEmotes();

		if (GameDataService.ActionTimelines != null)
			this.Selector.AddItems(GameDataService.Actions);

		if (GameDataService.ActionTimelines != null)
			this.Selector.AddItems(GameDataService.ActionTimelines);

		return Task.CompletedTask;
	}

	private void OnClose()
	{
		this.Close?.Invoke();
	}

	private void OnSelectionChanged()
	{
		this.SelectionChanged?.Invoke();
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

	private bool OnFilter(object obj, string[]? search = null)
	{
		if (obj is IAnimation animation)
		{
			// Filter out any that are clearly invalid
			if (string.IsNullOrEmpty(animation.DisplayName) || animation.Timeline == null || string.IsNullOrEmpty(animation.Timeline.Key))
				return false;

			if (!this.Filter.IncludeEmotes && animation is EmoteEntry)
				return false;

			if (!this.Filter.IncludeActions && animation is Action)
				return false;

			if (!this.Filter.IncludeRaw && animation is ActionTimeline)
				return false;

			if (!this.Filter.IncludeBlendable && animation.Timeline.Slot != Memory.AnimationMemory.AnimationSlots.FullBody)
				return false;

			if (!this.Filter.IncludeFullBody && animation.Timeline.Slot == Memory.AnimationMemory.AnimationSlots.FullBody)
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

		return false;
	}

	private int OnSort(object a, object b)
	{
		if (a == b)
			return 0;

		if (a is IAnimation animA && b is IAnimation animB)
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

		return 0;
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
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
