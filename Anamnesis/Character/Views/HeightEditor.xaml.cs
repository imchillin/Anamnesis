// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.ComponentModel;
	using System.Windows;
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

		public static readonly IBind<Customize.Tribes> TribeDp = Binder.Register<Customize.Tribes, HeightEditor>(nameof(Tribe), OnTribeChanged);
		public static readonly IBind<Customize.Genders> GenderDp = Binder.Register<Customize.Genders, HeightEditor>(nameof(Gender), OnGenderChanged);
		public static readonly IBind<Customize.Ages> AgeDp = Binder.Register<Customize.Ages, HeightEditor>(nameof(Age), OnAgeChanged);

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

		public Customize.Tribes Tribe
		{
			get => TribeDp.Get(this);
			set => TribeDp.Set(this, value);
		}

		public Customize.Genders Gender
		{
			get => GenderDp.Get(this);
			set => GenderDp.Set(this, value);
		}

		public Customize.Ages Age
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

		private static void OnTribeChanged(HeightEditor sender, Customize.Tribes value)
		{
			OnValueChanged(sender, 0);
		}

		private static void OnGenderChanged(HeightEditor sender, Customize.Genders value)
		{
			OnValueChanged(sender, 0);
		}

		private static void OnAgeChanged(HeightEditor sender, Customize.Ages value)
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

		private (double min, double max) GetHeightRange(Customize.Tribes tribe, Customize.Genders gender, Customize.Ages age)
		{
			// no support for old or young height calculations
			if (age != Customize.Ages.Normal)
				return (0, 0);

			double min = this.GetMinHeight(gender, tribe);
			double max = this.GetMaxHeight(gender, tribe);

			return (min, max);
		}

		private double GetMinHeight(Customize.Genders gender, Customize.Tribes tribe)
		{
			bool isMale = gender == Customize.Genders.Masculine;

			switch (tribe)
			{
				// Hyur
				case Customize.Tribes.Midlander: return isMale ? 168 : 157.4;
				case Customize.Tribes.Highlander: return isMale ? 184.8 : 173.1;

				// Elezen
				case Customize.Tribes.Wildwood:
				case Customize.Tribes.Duskwight: return isMale ? 194.1 : 183.5;

				// Lalafel
				case Customize.Tribes.Plainsfolk:
				case Customize.Tribes.Dunesfolk: return 86.9;

				// Miqo'te
				case Customize.Tribes.SeekerOfTheSun:
				case Customize.Tribes.KeeperOfTheMoon: return isMale ? 159.2 : 149.7;

				// Roegadyn
				case Customize.Tribes.SeaWolf:
				case Customize.Tribes.Hellsguard: return isMale ? 213.5 : 192;

				// Au Ra
				case Customize.Tribes.Raen:
				case Customize.Tribes.Xaela: return isMale ? 203 : 146;

				// Hrothgar
				case Customize.Tribes.Helions:
				case Customize.Tribes.TheLost: return isMale ? 196.2 : 0;

				// Viera
				case Customize.Tribes.Rava:
				case Customize.Tribes.Veena: return isMale ? 172.2 : 178.8;
			}

			return 0;
		}

		private double GetMaxHeight(Customize.Genders gender, Customize.Tribes tribe)
		{
			bool isMale = gender == Customize.Genders.Masculine;

			switch (tribe)
			{
				// Hyur
				case Customize.Tribes.Midlander: return isMale ? 182 : 170.5;
				case Customize.Tribes.Highlander: return isMale ? 200.2 : 187.6;

				// Elezen
				case Customize.Tribes.Wildwood:
				case Customize.Tribes.Duskwight: return isMale ? 209.8 : 198.4;

				// Lalafel
				case Customize.Tribes.Plainsfolk:
				case Customize.Tribes.Dunesfolk: return 97;

				// Miqo'te
				case Customize.Tribes.SeekerOfTheSun:
				case Customize.Tribes.KeeperOfTheMoon: return isMale ? 173.2 : 162.2;

				// Roegadyn
				case Customize.Tribes.SeaWolf:
				case Customize.Tribes.Hellsguard: return isMale ? 230.4 : 222.7;

				// Au Ra
				case Customize.Tribes.Raen:
				case Customize.Tribes.Xaela: return isMale ? 217 : 158.5;

				// Hrothgar
				case Customize.Tribes.Helions:
				case Customize.Tribes.TheLost: return isMale ? 212.9 : 0;

				// Viera
				case Customize.Tribes.Rava:
				case Customize.Tribes.Veena: return isMale ? 186.5 : 191.4;
			}

			return 0;
		}
	}
}
