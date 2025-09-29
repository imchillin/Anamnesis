// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for FacialFeaturesControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class FacialFeaturesControl : UserControl
{
	public static readonly IBind<ActorCustomizeMemory.FacialFeature> ValueDp = Binder.Register<ActorCustomizeMemory.FacialFeature, FacialFeaturesControl>(nameof(Value), OnValueChanged);
	public static readonly IBind<ActorCustomizeMemory.Genders> GenderDp = Binder.Register<ActorCustomizeMemory.Genders, FacialFeaturesControl>(nameof(Gender), OnGenderChanged);
	public static readonly IBind<ActorCustomizeMemory.Tribes> TribeDp = Binder.Register<ActorCustomizeMemory.Tribes, FacialFeaturesControl>(nameof(Tribe), OnTribeChanged);
	public static readonly IBind<byte> FaceDp = Binder.Register<byte, FacialFeaturesControl>(nameof(Face), OnHeadChanged);

	private readonly List<Option> features = new();
	private bool locked = false;

	public FacialFeaturesControl()
	{
		this.InitializeComponent();
		OnValueChanged(this, this.Value);
	}

	public ActorCustomizeMemory.Genders Gender
	{
		get => GenderDp.Get(this);
		set => GenderDp.Set(this, value);
	}

	public ActorCustomizeMemory.Tribes Tribe
	{
		get => TribeDp.Get(this);
		set => TribeDp.Set(this, value);
	}

	public byte Face
	{
		get => FaceDp.Get(this);
		set => FaceDp.Set(this, value);
	}

	public ActorCustomizeMemory.FacialFeature Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	private static void OnGenderChanged(FacialFeaturesControl sender, ActorCustomizeMemory.Genders value)
	{
		sender.GetFeatures();
		OnValueChanged(sender, sender.Value);
	}

	private static void OnTribeChanged(FacialFeaturesControl sender, ActorCustomizeMemory.Tribes value)
	{
		sender.GetFeatures();
		OnValueChanged(sender, sender.Value);
	}

	private static void OnHeadChanged(FacialFeaturesControl sender, byte value)
	{
		sender.GetFeatures();
		OnValueChanged(sender, sender.Value);
	}

	private static void OnValueChanged(FacialFeaturesControl sender, ActorCustomizeMemory.FacialFeature value)
	{
		sender.locked = true;
		foreach (Option op in sender.features)
		{
			op.Selected = sender.Value.HasFlagUnsafe(op.Value);

			if (op.Selected)
			{
				sender.FeaturesList.SelectedItems.Add(op);
			}
		}

		sender.locked = false;
	}

	private static ActorCustomizeMemory.FacialFeature GetValue(int index)
	{
		return index switch
		{
			0 => ActorCustomizeMemory.FacialFeature.First,
			1 => ActorCustomizeMemory.FacialFeature.Second,
			2 => ActorCustomizeMemory.FacialFeature.Third,
			3 => ActorCustomizeMemory.FacialFeature.Fourth,
			4 => ActorCustomizeMemory.FacialFeature.Fifth,
			5 => ActorCustomizeMemory.FacialFeature.Sixth,
			6 => ActorCustomizeMemory.FacialFeature.Seventh,
			_ => throw new Exception("Invalid index value"),
		};
	}

	private void GetFeatures()
	{
		this.locked = true;
		this.FeaturesList.ItemsSource = null;

		if (this.Tribe == 0)
			return;

		ImgRef[]? facialFeatures = null;
		if (GameDataService.CharacterMakeTypes != null)
		{
			foreach (CharaMakeType set in GameDataService.CharacterMakeTypes)
			{
				if (set.CustomizeTribe != this.Tribe)
					continue;

				if (set.Gender != this.Gender)
					continue;

				if (set.FacialFeatures == null)
					throw new Exception("Missing facial features");

				facialFeatures = set.FacialFeatures.ToArray();
				break;
			}
		}

		if (facialFeatures == null)
			return;

		this.features.Clear();

		// TODO: Handle hrothgar <-> other races offsets correctly.

		// Add an offset Fem Hrothgar to show the facial features icons properly.
		// Fem Hrothgar use faces 5-8, always offset.
		// Masc Hrothgar use faces 1-4 and 5-8, offset only if we're between 5 and 8.
		int hrothOffset = 0;
		bool isHroth = this.Tribe == ActorCustomizeMemory.Tribes.Helions || this.Tribe == ActorCustomizeMemory.Tribes.TheLost;
		if (isHroth && (this.Gender == ActorCustomizeMemory.Genders.Feminine || (this.Face >= 5 && this.Face <= 8)))
		{
			hrothOffset = 4;
		}

		for (byte i = 0; i < 7; i++)
		{
			int id = ((this.Face - (1 + hrothOffset)) * 7) + i;

			if (id < 0 || id >= facialFeatures.Length)
				continue;

			var op = new Option
			{
				Icon = facialFeatures[id],
				Value = GetValue(i),
				Index = id,
			};
			this.features.Add(op);
		}

		var legacyTattoo = new Option
		{
			Value = ActorCustomizeMemory.FacialFeature.LegacyTattoo,
		};
		this.features.Add(legacyTattoo);

		this.FeaturesList.ItemsSource = this.features;
		this.locked = false;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.locked)
			return;

		ActorCustomizeMemory.FacialFeature flags = ActorCustomizeMemory.FacialFeature.None;
		foreach (Option? op in this.FeaturesList.SelectedItems)
		{
			if (op == null)
				continue;

			flags |= op.Value;
		}

		this.Value = flags;
	}

	[AddINotifyPropertyChangedInterface]
	private class Option
	{
		public ActorCustomizeMemory.FacialFeature Value { get; set; }
		public ImgRef? Icon { get; set; }
		public int Index { get; set; }
		public bool Selected { get; set; }
	}
}
