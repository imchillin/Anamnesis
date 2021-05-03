// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles.DependencyProperties;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for FacialFeaturesControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FacialFeaturesControl : UserControl
	{
		public static readonly IBind<Appearance.FacialFeature> ValueDp = Binder.Register<Appearance.FacialFeature, FacialFeaturesControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<Appearance.Genders> GenderDp = Binder.Register<Appearance.Genders, FacialFeaturesControl>(nameof(Gender), OnGenderChanged);
		public static readonly IBind<Appearance.Tribes> TribeDp = Binder.Register<Appearance.Tribes, FacialFeaturesControl>(nameof(Tribe), OnTribeChanged);
		public static readonly IBind<byte> HeadDp = Binder.Register<byte, FacialFeaturesControl>(nameof(Head), OnHeadChanged);

		private readonly List<Option> features = new List<Option>();
		private bool locked = false;

		public FacialFeaturesControl()
		{
			this.InitializeComponent();
			OnValueChanged(this, this.Value);
		}

		public Appearance.Genders Gender
		{
			get => GenderDp.Get(this);
			set => GenderDp.Set(this, value);
		}

		public Appearance.Tribes Tribe
		{
			get => TribeDp.Get(this);
			set => TribeDp.Set(this, value);
		}

		public byte Head
		{
			get => HeadDp.Get(this);
			set => HeadDp.Set(this, value);
		}

		public Appearance.FacialFeature Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		private static void OnGenderChanged(FacialFeaturesControl sender, Appearance.Genders value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnTribeChanged(FacialFeaturesControl sender, Appearance.Tribes value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnHeadChanged(FacialFeaturesControl sender, byte value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnValueChanged(FacialFeaturesControl sender, Appearance.FacialFeature value)
		{
			sender.locked = true;
			foreach (Option op in sender.features)
			{
				op.Selected = sender.Value.HasFlag(op.Value);
			}

			sender.locked = false;
		}

		private void GetFeatures()
		{
			this.locked = true;
			this.FeaturesList.ItemsSource = null;

			if (this.Tribe == 0)
				return;

			ImageSource[]? facialFeatures = null;
			if (GameDataService.CharacterMakeTypes != null)
			{
				foreach (ICharaMakeType set in GameDataService.CharacterMakeTypes)
				{
					if (set.Tribe != this.Tribe)
						continue;

					if (set.Gender != this.Gender)
						continue;

					facialFeatures = set.FacialFeatures.ToArray();
					break;
				}
			}

			if (facialFeatures == null)
				return;

			this.features.Clear();
			for (byte i = 0; i < 7; i++)
			{
				int id = (this.Head - 1) + (8 * i);

				if (id < 0 || id >= facialFeatures.Length)
					continue;

				Option op = new Option();
				op.Icon = facialFeatures[id];
				op.Value = this.GetValue(i);
				op.Index = id;
				this.features.Add(op);
			}

			Option legacyTattoo = new Option();
			legacyTattoo.Value = Appearance.FacialFeature.LegacyTattoo;
			this.features.Add(legacyTattoo);

			this.FeaturesList.ItemsSource = this.features;
			this.locked = false;
		}

		private Appearance.FacialFeature GetValue(int index)
		{
			switch (index)
			{
				case 0: return Appearance.FacialFeature.First;
				case 1: return Appearance.FacialFeature.Second;
				case 2: return Appearance.FacialFeature.Third;
				case 3: return Appearance.FacialFeature.Fourth;
				case 4: return Appearance.FacialFeature.Fifth;
				case 5: return Appearance.FacialFeature.Sixth;
				case 6: return Appearance.FacialFeature.Seventh;
			}

			throw new Exception("Invalid index value");
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.locked)
				return;

			Appearance.FacialFeature flags = Appearance.FacialFeature.None;
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
			public Appearance.FacialFeature Value { get; set; }
			public ImageSource? Icon { get; set; }
			public int Index { get; set; }
			public bool Selected { get; set; }
		}
	}
}
