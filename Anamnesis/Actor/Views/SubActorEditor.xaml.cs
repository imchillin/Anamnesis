// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Anamnesis.Actor.Utilities;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for SubActorEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class SubActorEditor : UserControl
{
	public static readonly IBind<ActorMemory?> ActorDp = Binder.Register<ActorMemory?, SubActorEditor>(nameof(Actor), OnActorChanged, BindMode.OneWay);
	public static readonly IBind<ActorMemory?> SubActorDp = Binder.Register<ActorMemory?, SubActorEditor>(nameof(SubActor), OnChanged, BindMode.TwoWay);
	public static readonly IBind<Types> TypeDp = Binder.Register<Types, SubActorEditor>(nameof(SubActorType), OnTypeChanged);

	private static readonly NpcSelector.NpcFilter MountFilter = new NpcSelector.NpcFilter()
	{
		TypesLocked = true,
		IncludeMount = true,
	};

	private static readonly NpcSelector.NpcFilter CompanionFilter = new NpcSelector.NpcFilter()
	{
		TypesLocked = true,
		IncludeCompanion = true,
	};

	private static readonly NpcSelector.NpcFilter OrnamentFilter = new NpcSelector.NpcFilter()
	{
		TypesLocked = true,
		IncludeOrnament = true,
	};

	public SubActorEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		OnTypeChanged(this, this.SubActorType);
	}

	public enum Types
	{
		Mount,
		Companion,
		Ornament,
	}

	public ActorMemory? Actor
	{
		get => ActorDp.Get(this);
		set => ActorDp.Set(this, value);
	}

	public ActorMemory? SubActor
	{
		get => SubActorDp.Get(this);
		set => SubActorDp.Set(this, value);
	}

	public Types SubActorType
	{
		get => TypeDp.Get(this);
		set => TypeDp.Set(this, value);
	}

	public INpcBase? Npc { get; set; }
	public ImageSource? IconSource { get; set; }
	public string TypeKey => "SubActor_" + this.SubActorType;

	private static void OnActorChanged(SubActorEditor sender, ActorMemory? oldValue, ActorMemory? newValue)
	{
		if (oldValue != null)
		{
			oldValue.PropertyChanged -= sender.OnActorPropertyChanged;
		}

		if (newValue != null)
		{
			newValue.PropertyChanged += sender.OnActorPropertyChanged;
		}
	}

	private static void OnChanged(SubActorEditor sender, ActorMemory? oldValue, ActorMemory? newValue)
	{
		if (oldValue != null)
			oldValue.PropertyChanged -= sender.OnActorPropertyChanged;

		if (newValue == null || sender.Actor == null)
		{
			sender.Npc = null;
			return;
		}

		newValue.PropertyChanged += sender.OnActorPropertyChanged;

		if (sender.SubActorType == Types.Companion)
		{
			sender.Npc = GameDataService.Companions.GetRow(newValue.DataId);
		}
		else if (sender.SubActorType == Types.Mount)
		{
			sender.Npc = GameDataService.Mounts.GetRow(sender.Actor.MountId);
		}
		else if (sender.SubActorType == Types.Ornament)
		{
			sender.Npc = GameDataService.Ornaments.GetRow(sender.Actor.CharacterModeInput);
		}
	}

	private static void OnTypeChanged(SubActorEditor sender, Types value)
	{
		try
		{
			BitmapImage logo = new BitmapImage();
			logo.BeginInit();
			logo.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
			logo.UriSource = new Uri("pack://application:,,,/Anamnesis;component/Assets/Slots/" + value.ToString() + ".png");
			logo.EndInit();
			sender.IconSource = logo;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to get icon for sub actor type: {value}", ex);
		}
	}

	private async void OnActorPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ActorMemory.MountId)
			|| e.PropertyName == nameof(ActorMemory.CharacterModeInput)
			|| e.PropertyName == nameof(ActorMemory.DataId))
		{
			await Dispatch.MainThread();
			OnChanged(this, this.SubActor, this.SubActor);
		}
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		throw new NotImplementedException();

		/*if (this.SubActorType == Types.Mount)
		{
			if (!this.Actor.IsMounted || this.Actor.Mount == null)
				return;

			NpcSelector view = SelectorControl.Show<NpcSelector, INpcBase>(null, this.Apply);
			view.ChangeFilter(MountFilter);
		}
		else if (this.SubActorType == Types.Companion)
		{
			if (!this.Actor.HasCompanion || this.Actor.Companion == null)
				return;

			NpcSelector view = SelectorControl.Show<NpcSelector, INpcBase>(null, this.Apply);
			view.ChangeFilter(CompanionFilter);
		}
		else if (this.SubActorType == Types.Ornament)
		{
			if (!this.Actor.IsUsingOrnament || this.Actor.Ornament == null)
				return;

			NpcSelector view = SelectorControl.Show<NpcSelector, INpcBase>(null, this.Apply);
			view.ChangeFilter(OrnamentFilter);
		}*/
	}

	private async void Apply(INpcBase npc)
	{
		if (this.Actor == null || npc == null)
			return;

		CharacterFile apFile = npc.ToFile();

		if (npc is Mount mount)
		{
			await SubActorUtility.SwitchMount(this.Actor, mount);
		}
		else if (npc is Companion companion)
		{
			await SubActorUtility.SwitchCompanion(this.Actor, companion);
		}
		else if (npc is Ornament ornament)
		{
			await SubActorUtility.SwitchOrnament(this.Actor, ornament);
		}
	}
}
