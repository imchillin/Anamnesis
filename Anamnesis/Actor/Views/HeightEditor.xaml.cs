// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for HeightEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class HeightEditor : UserControl, INotifyPropertyChanged
{
	/// <summary>Dependency property for the height value.</summary>
	public static readonly IBind<byte> ValueDp = Binder.Register<byte, HeightEditor>(nameof(Value), OnValueChanged, BindMode.TwoWay);

	/// <summary>Dependency property for the tribe value.</summary>
	public static readonly IBind<ActorCustomizeMemory.Tribes> TribeDp = Binder.Register<ActorCustomizeMemory.Tribes, HeightEditor>(nameof(Tribe), OnTribeChanged, BindMode.OneWay);

	/// <summary>Dependency property for the gender value.</summary>
	public static readonly IBind<ActorCustomizeMemory.Genders> GenderDp = Binder.Register<ActorCustomizeMemory.Genders, HeightEditor>(nameof(Gender), OnGenderChanged, BindMode.OneWay);

	/// <summary>Dependency property for the age value.</summary>
	public static readonly IBind<ActorCustomizeMemory.Ages> AgeDp = Binder.Register<ActorCustomizeMemory.Ages, HeightEditor>(nameof(Age), OnAgeChanged, BindMode.OneWay);

	/// <summary>The default minimum height value.</summary>
	private const double DEFAULT_MIN_HEIGHT = 0.0;

	/// <summary>The default maximum height value.</summary>
	private const double DEFAULT_MAX_HEIGHT = 255.0;

	/// <summary>
	/// A lookup dictionary for the minimum and maximum heights of each tribe/gender combination.
	/// </summary>
	private static readonly Dictionary<(ActorCustomizeMemory.Tribes, ActorCustomizeMemory.Genders), (double Min, double Max)> s_heightRanges = new(capacity: 32)
	{
		// Hyur
		{ (ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Masculine), (168.0f, 182.0f) },
		{ (ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Feminine), (157.4f, 170.5f) },
		{ (ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Masculine), (184.8f, 200.2f) },
		{ (ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Feminine), (173.1f, 187.6f) },

		// Elezen
		{ (ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Masculine), (194.1f, 209.8f) },
		{ (ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Feminine), (183.5f, 198.4f) },
		{ (ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Masculine), (194.1f, 209.8f) },
		{ (ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Feminine), (183.5f, 198.4f) },

		// Lalafell
		{ (ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Masculine), (86.9f, 97.0f) },
		{ (ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Feminine), (86.9f, 97.0f) },
		{ (ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Masculine), (86.9f, 97.0f) },
		{ (ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Feminine), (86.9f, 97.0f) },

		// Miqo'te
		{ (ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine), (159.2f, 173.2f) },
		{ (ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine), (149.7f, 162.2f) },
		{ (ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine), (159.2f, 173.2f) },
		{ (ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine), (149.7f, 162.2f) },

		// Roegadyn
		{ (ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Masculine), (213.5f, 230.4f) },
		{ (ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Feminine), (192.0f, 222.7f) },
		{ (ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Masculine), (213.5f, 230.4f) },
		{ (ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Feminine), (192.0f, 222.7f) },

		// Au Ra
		{ (ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Masculine), (203.0f, 217.0f) },
		{ (ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Feminine), (146.0f, 158.5f) },
		{ (ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Masculine), (203.0f, 217.0f) },
		{ (ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Feminine), (146.0f, 158.5f) },

		// Hrothgar
		{ (ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Masculine), (196.2f, 212.9f) },
		{ (ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Feminine), (184.4f, 199.4f) },
		{ (ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Masculine), (196.2f, 212.9f) },
		{ (ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Feminine), (184.4f, 199.4f) },

		// Viera
		{ (ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine), (172.2f, 186.5f) },
		{ (ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine), (178.8f, 191.4f) },
		{ (ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine), (172.2f, 186.5f) },
		{ (ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine), (178.8f, 191.4f) },
	};

	/// <summary>
	/// Initializes a new instance of the <see cref="HeightEditor"/> class.
	/// </summary>
	public HeightEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	/// <summary>Event that is raised when a property value changes.</summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Gets or sets the height value (byte value).</summary>
	[AlsoNotifyFor(nameof(ValueCm))]
	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	/// <summary>Gets or sets the tribe value.</summary>
	public ActorCustomizeMemory.Tribes Tribe
	{
		get => TribeDp.Get(this);
		set => TribeDp.Set(this, value);
	}

	/// <summary>Gets or sets the gender value.</summary>
	public ActorCustomizeMemory.Genders Gender
	{
		get => GenderDp.Get(this);
		set => GenderDp.Set(this, value);
	}

	/// <summary>Gets or sets the age value.</summary>
	public ActorCustomizeMemory.Ages Age
	{
		get => AgeDp.Get(this);
		set => AgeDp.Set(this, value);
	}

	/// <summary>Gets or sets the height value in centimetres (cm).</summary>
	[AlsoNotifyFor(nameof(Value))]
	public int ValueCm
	{
		get => this.HeightToCm(this.Value);
		set => this.Value = this.CmToHeight(value);
	}

	/// <summary>
	/// Gets the minimum height of a given tribe and gender.
	/// </summary>
	/// <param name="tribe">The tribe.</param>
	/// <param name="gender">The gender.</param>
	/// <returns>The minimum height.</returns>
	/// <remarks>
	/// If a tribe and gender combination is not found, the height will be 0.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double GetMinHeight(ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender)
	{
		return s_heightRanges.TryGetValue((tribe, gender), out var range) ? range.Min : DEFAULT_MIN_HEIGHT;
	}

	/// <summary>
	/// Gets the maximum height of a given tribe and gender.
	/// </summary>
	/// <param name="tribe">The tribe.</param>
	/// <param name="gender">The gender.</param>
	/// <returns>The maximum height.</returns>
	/// <remarks>
	/// If a tribe and gender combination is not found, the height will be 0.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double GetMaxHeight(ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender)
	{
		return s_heightRanges.TryGetValue((tribe, gender), out var range) ? range.Max : DEFAULT_MAX_HEIGHT;
	}

	/// <summary>
	/// Gets the minimum and maximum height of a given tribe, gender, and age.
	/// </summary>
	/// <param name="tribe">The tribe.</param>
	/// <param name="gender">The gender.</param>
	/// <param name="age">The age.</param>
	/// <returns>The minimum and maximum height.</returns>
	/// <remarks>
	/// Ages for non-playable characters are not supported and will result in a height range of 0.
	/// Similarly, if a tribe and gender combination is not found, the height range will be 0.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (double Min, double Max) GetHeightRange(ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender, ActorCustomizeMemory.Ages age)
	{
		// No support for old or young height calculations
		if (age != ActorCustomizeMemory.Ages.Normal)
			return (0, 0);

		return s_heightRanges.TryGetValue((tribe, gender), out var range) ? range : (DEFAULT_MIN_HEIGHT, DEFAULT_MAX_HEIGHT);
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

	/// <summary>
	/// Converts a character's height from a byte value to centimetres (cm).
	/// </summary>
	/// <param name="height">The height as a byte value.</param>
	/// <returns>The height in centimetres.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int HeightToCm(byte height)
	{
		(double min, double max) = GetHeightRange(this.Tribe, this.Gender, this.Age);
		return (int)Math.Round(min + (height / 100.0 * (max - min)));
	}

	/// <summary>
	/// Converts a character's height from centimetres (cm) to a byte value.
	/// </summary>
	/// <param name="cm">The height in centimetres.</param>
	/// <returns>The height as a byte value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private byte CmToHeight(int cm)
	{
		(double min, double max) = GetHeightRange(this.Tribe, this.Gender, this.Age);
		cm = (int)Math.Clamp(cm, min, max);
		return (byte)((cm - min) / (max - min) * 100);
	}
}
