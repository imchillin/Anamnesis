// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.ComponentModel;
using System.Windows.Controls;
using Anamnesis.Memory;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for HeightEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class HeightEditor : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<byte> ValueDp = Binder.Register<byte, HeightEditor>(nameof(Value), OnValueChanged);

	public static readonly IBind<ActorCustomizeMemory.Tribes> TribeDp = Binder.Register<ActorCustomizeMemory.Tribes, HeightEditor>(nameof(Tribe), OnTribeChanged);
	public static readonly IBind<ActorCustomizeMemory.Genders> GenderDp = Binder.Register<ActorCustomizeMemory.Genders, HeightEditor>(nameof(Gender), OnGenderChanged);
	public static readonly IBind<ActorCustomizeMemory.Ages> AgeDp = Binder.Register<ActorCustomizeMemory.Ages, HeightEditor>(nameof(Age), OnAgeChanged);

	public HeightEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[AlsoNotifyFor(nameof(ValueCm))]
	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public ActorCustomizeMemory.Tribes Tribe
	{
		get => TribeDp.Get(this);
		set => TribeDp.Set(this, value);
	}

	public ActorCustomizeMemory.Genders Gender
	{
		get => GenderDp.Get(this);
		set => GenderDp.Set(this, value);
	}

	public ActorCustomizeMemory.Ages Age
	{
		get => AgeDp.Get(this);
		set => AgeDp.Set(this, value);
	}

	[AlsoNotifyFor(nameof(Value))]
	public int ValueCm
	{
		get => this.HeightToCm(this.Value);
		set => this.Value = this.CmToHeight(value);
	}

	private static void OnValueChanged(HeightEditor sender, byte value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ValueCm)));
	}

	private static void OnTribeChanged(HeightEditor sender, ActorCustomizeMemory.Tribes value)
	{
		OnValueChanged(sender, 0);
	}

	private static void OnGenderChanged(HeightEditor sender, ActorCustomizeMemory.Genders value)
	{
		OnValueChanged(sender, 0);
	}

	private static void OnAgeChanged(HeightEditor sender, ActorCustomizeMemory.Ages value)
	{
		OnValueChanged(sender, 0);
	}

	private int HeightToCm(byte height)
	{
		(double min, double max) = this.GetHeightRange(this.Tribe, this.Gender, this.Age);

		return (int)Math.Round(min + ((height / 100.0) * (max - min)));
	}

	private byte CmToHeight(int cm)
	{
		(double min, double max) = this.GetHeightRange(this.Tribe, this.Gender, this.Age);
		cm = (int)Math.Clamp(cm, min, max);

		return (byte)(((cm - min) / (max - min)) * 100);
	}

	private (double min, double max) GetHeightRange(ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender, ActorCustomizeMemory.Ages age)
	{
		// no support for old or young height calculations
		if (age != ActorCustomizeMemory.Ages.Normal)
			return (0, 0);

		double min = this.GetMinHeight(gender, tribe);
		double max = this.GetMaxHeight(gender, tribe);

		return (min, max);
	}

	private double GetMinHeight(ActorCustomizeMemory.Genders gender, ActorCustomizeMemory.Tribes tribe)
	{
		bool isMale = gender == ActorCustomizeMemory.Genders.Masculine;

		switch (tribe)
		{
			// Hyur
			case ActorCustomizeMemory.Tribes.Midlander: return isMale ? 168 : 157.4;
			case ActorCustomizeMemory.Tribes.Highlander: return isMale ? 184.8 : 173.1;

			// Elezen
			case ActorCustomizeMemory.Tribes.Wildwood:
			case ActorCustomizeMemory.Tribes.Duskwight: return isMale ? 194.1 : 183.5;

			// Lalafel
			case ActorCustomizeMemory.Tribes.Plainsfolk:
			case ActorCustomizeMemory.Tribes.Dunesfolk: return 86.9;

			// Miqo'te
			case ActorCustomizeMemory.Tribes.SeekerOfTheSun:
			case ActorCustomizeMemory.Tribes.KeeperOfTheMoon: return isMale ? 159.2 : 149.7;

			// Roegadyn
			case ActorCustomizeMemory.Tribes.SeaWolf:
			case ActorCustomizeMemory.Tribes.Hellsguard: return isMale ? 213.5 : 192;

			// Au Ra
			case ActorCustomizeMemory.Tribes.Raen:
			case ActorCustomizeMemory.Tribes.Xaela: return isMale ? 203 : 146;

			// Hrothgar
			case ActorCustomizeMemory.Tribes.Helions:
			case ActorCustomizeMemory.Tribes.TheLost: return isMale ? 196.2 : 0;

			// Viera
			case ActorCustomizeMemory.Tribes.Rava:
			case ActorCustomizeMemory.Tribes.Veena: return isMale ? 172.2 : 178.8;
		}

		return 0;
	}

	private double GetMaxHeight(ActorCustomizeMemory.Genders gender, ActorCustomizeMemory.Tribes tribe)
	{
		bool isMale = gender == ActorCustomizeMemory.Genders.Masculine;

		switch (tribe)
		{
			// Hyur
			case ActorCustomizeMemory.Tribes.Midlander: return isMale ? 182 : 170.5;
			case ActorCustomizeMemory.Tribes.Highlander: return isMale ? 200.2 : 187.6;

			// Elezen
			case ActorCustomizeMemory.Tribes.Wildwood:
			case ActorCustomizeMemory.Tribes.Duskwight: return isMale ? 209.8 : 198.4;

			// Lalafel
			case ActorCustomizeMemory.Tribes.Plainsfolk:
			case ActorCustomizeMemory.Tribes.Dunesfolk: return 97;

			// Miqo'te
			case ActorCustomizeMemory.Tribes.SeekerOfTheSun:
			case ActorCustomizeMemory.Tribes.KeeperOfTheMoon: return isMale ? 173.2 : 162.2;

			// Roegadyn
			case ActorCustomizeMemory.Tribes.SeaWolf:
			case ActorCustomizeMemory.Tribes.Hellsguard: return isMale ? 230.4 : 222.7;

			// Au Ra
			case ActorCustomizeMemory.Tribes.Raen:
			case ActorCustomizeMemory.Tribes.Xaela: return isMale ? 217 : 158.5;

			// Hrothgar
			case ActorCustomizeMemory.Tribes.Helions:
			case ActorCustomizeMemory.Tribes.TheLost: return isMale ? 212.9 : 0;

			// Viera
			case ActorCustomizeMemory.Tribes.Rava:
			case ActorCustomizeMemory.Tribes.Veena: return isMale ? 186.5 : 191.4;
		}

		return 0;
	}
}
